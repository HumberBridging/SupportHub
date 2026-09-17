using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using SupportHub.AiConsole;
using System.ClientModel;

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
        Console.WriteLine("  1 Chat    2 Structured   q Quit");
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
