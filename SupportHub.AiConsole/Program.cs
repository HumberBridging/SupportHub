using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using SupportHub.AiConsole;
using System.ClientModel;
using System.Text.Json;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().AddEnvironmentVariables().Build();

//Endpoint/Deployment name/api key for the OpenAI API
//Please note that all are read from User secrets or environment variables,
//You can set them in your development environment or in your deployment environment as needed.
//Managed Identity would be better solution for production, but for simplicity, we are using API key in this sample.
string endpoint = config["Foundry:Endpoint"] ?? "", deployment = config["Foundry:Deployment"] ?? "", key = config["Foundry:ApiKey"] ?? "";

if (endpoint == "" || deployment == "" || key == "")
{
    Console.Error.WriteLine("Set Foundry:Endpoint, Foundry:Deployment and Foundry:ApiKey with dotnet user-secrets first.");
    return 1;
}

// The provider-specific part: three lines. Everything below talks to IChatClient.
IChatClient model = new OpenAIClient(new ApiKeyCredential(key), new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    .GetChatClient(deployment)
    .AsIChatClient();

//Obtaining a ticket from DemoData for demo purposes.
var ticket = DemoData.DuplicateCharge;
var choice = args.FirstOrDefault() ?? null;

while (true)
{
    if(choice == null)
    {
        Console.WriteLine();
        Console.WriteLine($"Ticket: \"{ticket.Description}\"");
        Console.WriteLine("  1 Chat    2 Structured   3 Tools   4 Agent   q Quit");
        Console.Write("> ");
        choice = Console.ReadLine()?.Trim();
    }

    switch (choice)
    {
        case "1":
            await Level1Chat();
            break;
        case "2":
            await Level2Structured();
            break;
        case "3":
            await Level3Tools();
            break;
        case "4":
            await Level4Agent();
            break;
        case "q":
            return 0;
    }

    if (args.Length > 0) return 0;
    choice = null;
}

// ---------------------------------------------------------------- 1 · CHAT
async Task Level1Chat()
{
    Helpers.Heading("1 · CHAT: a friendly paragraph. Impressive, and useless to an API.");
    
    ChatResponse reply = await model.GetResponseAsync(
        $"A customer wrote this support ticket. How should we handle it?\n\n{ticket.Description}");

    Console.WriteLine(reply.Text);
    
    Helpers.Usage(reply.ModelId!, reply.Usage!.InputTokenCount , reply.Usage!.OutputTokenCount);
}

// ---------------------------------------------------------------- 2 · STRUCTURED
async Task Level2Structured()
{
    Helpers.Heading("2 · STRUCTURED: the model honours a contract.");

    ChatResponse<Triage> reply = await model.GetResponseAsync<Triage>(
        [
            new(ChatRole.System, DemoData.TriageInstructions),
            new(ChatRole.User, $"<ticket>{ticket.Description}</ticket>")
        ],
        new ChatOptions { AdditionalProperties = new() { ["strict"] = true } });

    Console.WriteLine("Raw JSON:  " + reply.Text);

    if (reply.TryGetResult(out var triage))
        Console.WriteLine($"C# record: Category={triage.Category}, Priority={triage.Priority}, Summary=\"{triage.Summary}\"");
    
    Helpers.Usage(reply.ModelId!, reply.Usage!.InputTokenCount , reply.Usage!.OutputTokenCount);
}

// ---------------------------------------------------------------- 3 · TOOLS

//We decide which tools to call
async Task Level3Tools()
{
    Helpers.Heading("3 · TOOLS: the model asks to use our code, then decides.");

    IChatClient withTools = model.AsBuilder().UseFunctionInvocation().Build();

    var options = new ChatOptions
    {
        //A tool is a C# method, plus a name and description, that the model is allowed to ask for.
        Tools = [AIFunctionFactory.Create(method: DemoData.GetCustomerTickets, name: "get_customer_tickets",
            description: "Returns the other recent tickets for a customer id.")],
        AdditionalProperties = new() { ["strict"] = true }
    };

    //Memory: None (default) means the model doesn't remember anything from previous calls.
    ChatResponse<Triage> reply = await withTools.GetResponseAsync<Triage>(
        [
            new(ChatRole.System, DemoData.TriageInstructions +
                " Before you decide, call get_customer_tickets for the ticket's customer. If the same problem was reported before, raise the priority."),
            new(ChatRole.User, $"<ticket>CustomerId: {ticket.CustomerId}\n{ticket.Description}</ticket>")
        ],
        options);

    // Seethe round trip: the tool call, the tool result, then the answer.
    foreach (var message in reply.Messages)
        foreach (var content in message.Contents)
        {
            if (content is FunctionCallContent call)
                Console.WriteLine($"  model -> call {call.Name}({JsonSerializer.Serialize(call.Arguments)})");
            else if (content is FunctionResultContent result)
                Console.WriteLine($"  our code -> {JsonSerializer.Serialize(result.Result)}");
        }

    Console.WriteLine("Answer: " + reply.Text);
    Helpers.Usage(reply.ModelId!, reply.Usage!.InputTokenCount, reply.Usage!.OutputTokenCount);
}

// ---------------------------------------------------------------- 4 · AGENT

//Agent decides which tool it picks, in what order, how many times etc
async Task Level4Agent()
{
    Helpers.Heading("4 · AGENT: instructions + tools + a loop. It picks the steps.");

    AIAgent agent = new ChatClientAgent(
        model,
        instructions: "You are SupportHub's triage assistant. Use the tools to answer questions about tickets. Be brief.",
        name: "TriageAgent",
        tools:
        [
            AIFunctionFactory.Create(method: DemoData.FindCustomer, name: "find_customer", description: "Finds a customer id by company name."),
            AIFunctionFactory.Create(method: DemoData.GetCustomerTickets, name: "get_customer_tickets", description: "Returns the recent tickets for a customer id.")
        ]);

    //Memory: The agent remembers the conversation in this session, so it can answer follow-up questions.
    AgentSession session = await agent.CreateSessionAsync();

    Console.WriteLine("You: Anything urgent from Globex this week?");

    // (3) TURN 1: the agent has to plan. It only knows "Globex", but the tickets tool needs an id.
    await AskAgentAsync(agent, session, "Anything urgent from Globex this week?");

    Console.WriteLine("You: And how many tickets have they opened in total?");

    // (4) TURN 2, SAME SESSION: "they" only makes sense because the session remembers turn 1.
    await AskAgentAsync(agent, session, "And how many tickets have they opened in total?");
}

//Helpers
static void ShowToolCalls(IEnumerable<ChatMessage> messages)
{
    foreach (ChatMessage message in messages)
        foreach (AIContent content in message.Contents)
        {
            if (content is FunctionCallContent call)
                Console.WriteLine($"  model    -> call {call.Name}({JsonSerializer.Serialize(call.Arguments)})");
            else if (content is FunctionResultContent result)
                Console.WriteLine($"  our code -> {JsonSerializer.Serialize(result.Result)}");
        }
}

static async Task AskAgentAsync(AIAgent agent, AgentSession session, string question)
{
    Console.WriteLine();
    Console.WriteLine("You: " + question);

    AgentResponse response = await agent.RunAsync(question, session);

    ShowToolCalls(response.Messages);
    Console.WriteLine("Agent: " + response.Text);
    Console.WriteLine($"  [messages this turn: {response.Messages.Count} · tokens: {response.Usage?.TotalTokenCount}]");
}