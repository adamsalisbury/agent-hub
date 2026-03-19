using System.CommandLine;
using AgentHub.Cli;

// Global options
var hubUrlOption = new Option<string>(
    "--hub-url",
    description: "Agent Hub base URL (overrides AGENT_HUB_URL environment variable)",
    getDefaultValue: () => Environment.GetEnvironmentVariable("AGENT_HUB_URL") ?? "http://localhost:5050");

var prettyOption = new Option<bool>(
    "--pretty",
    description: "Format output as pretty-printed JSON");

HubApiClient CreateClient(string hubUrl) => new(hubUrl);

// ── Root command ──────────────────────────────────────────────────────────────
var rootCommand = new RootCommand("Agent Hub CLI - communicate with the Agent Hub system");
rootCommand.AddGlobalOption(hubUrlOption);
rootCommand.AddGlobalOption(prettyOption);

// ── agents ───────────────────────────────────────────────────────────────────
var agentsCommand = new Command("agents", "Manage agents");
rootCommand.AddCommand(agentsCommand);

// agents list
var agentsListCommand = new Command("list", "List all registered agents");
agentsCommand.AddCommand(agentsListCommand);
agentsListCommand.SetHandler(async (hubUrl, pretty) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.GetAgentsAsync();
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption);

// agents register
var agentsRegisterCommand = new Command("register", "Register a new agent");
var registerNameOption = new Option<string>("--name", "Agent name") { IsRequired = true };
var registerDescriptionOption = new Option<string>("--description", "Agent description") { IsRequired = true };
var registerJobTitleOption = new Option<string?>("--job-title", "Agent job title (optional)");
var registerAvatarSvgOption = new Option<string?>("--avatar-svg", "Agent avatar as inline SVG (optional)");
agentsRegisterCommand.AddOption(registerNameOption);
agentsRegisterCommand.AddOption(registerDescriptionOption);
agentsRegisterCommand.AddOption(registerJobTitleOption);
agentsRegisterCommand.AddOption(registerAvatarSvgOption);
agentsCommand.AddCommand(agentsRegisterCommand);
agentsRegisterCommand.SetHandler(async (hubUrl, pretty, name, description, jobTitle, avatarSvg) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.RegisterAgentAsync(name, description, jobTitle, avatarSvg);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, registerNameOption, registerDescriptionOption, registerJobTitleOption, registerAvatarSvgOption);

// agents checkin
var agentsCheckinCommand = new Command("checkin", "Send a heartbeat check-in for an agent");
var checkinIdOption = new Option<Guid>("--id", "Agent ID") { IsRequired = true };
agentsCheckinCommand.AddOption(checkinIdOption);
agentsCommand.AddCommand(agentsCheckinCommand);
agentsCheckinCommand.SetHandler(async (hubUrl, pretty, id) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.CheckInAsync(id);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, checkinIdOption);

// agents task
var agentsTaskCommand = new Command("task", "Set or update the agent's current task");
var taskIdOption = new Option<Guid>("--id", "Agent ID") { IsRequired = true };
var taskDescriptionOption = new Option<string>("--description", "Description of the task being worked on") { IsRequired = true };
agentsTaskCommand.AddOption(taskIdOption);
agentsTaskCommand.AddOption(taskDescriptionOption);
agentsCommand.AddCommand(agentsTaskCommand);
agentsTaskCommand.SetHandler(async (hubUrl, pretty, id, description) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.SetAgentTaskAsync(id, description);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, taskIdOption, taskDescriptionOption);

// agents idle
var agentsIdleCommand = new Command("idle", "Clear the agent's current task (set to idle)");
var idleIdOption = new Option<Guid>("--id", "Agent ID") { IsRequired = true };
agentsIdleCommand.AddOption(idleIdOption);
agentsCommand.AddCommand(agentsIdleCommand);
agentsIdleCommand.SetHandler(async (hubUrl, pretty, id) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        await client.SetAgentIdleAsync(id);
        OutputFormatter.WriteMessage("Agent is now idle.", pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, idleIdOption);

// agents activities
var agentsActivitiesCommand = new Command("activities", "View the activity history for an agent");
var activitiesIdOption = new Option<Guid>("--id", "Agent ID") { IsRequired = true };
agentsActivitiesCommand.AddOption(activitiesIdOption);
agentsCommand.AddCommand(agentsActivitiesCommand);
agentsActivitiesCommand.SetHandler(async (hubUrl, pretty, id) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.GetAgentActivitiesAsync(id);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, activitiesIdOption);

// agents skills
var agentsSkillsCommand = new Command("skills", "List skills for an agent");
var skillsIdOption = new Option<Guid>("--id", "Agent ID") { IsRequired = true };
agentsSkillsCommand.AddOption(skillsIdOption);
agentsCommand.AddCommand(agentsSkillsCommand);
agentsSkillsCommand.SetHandler(async (hubUrl, pretty, id) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.GetAgentSkillsAsync(id);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, skillsIdOption);

// agents set-skills
var agentsSetSkillsCommand = new Command("set-skills", "Replace all skills for an agent");
var setSkillsIdOption = new Option<Guid>("--id", "Agent ID") { IsRequired = true };
var setSkillsJsonOption = new Option<string>("--json", "Skills as a JSON array, e.g. '[{\"name\":\"x\",\"description\":\"y\"}]'") { IsRequired = true };
agentsSetSkillsCommand.AddOption(setSkillsIdOption);
agentsSetSkillsCommand.AddOption(setSkillsJsonOption);
agentsCommand.AddCommand(agentsSetSkillsCommand);
agentsSetSkillsCommand.SetHandler(async (hubUrl, pretty, id, json) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.SetAgentSkillsAsync(id, json);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, setSkillsIdOption, setSkillsJsonOption);

// ── messages ──────────────────────────────────────────────────────────────────
var messagesCommand = new Command("messages", "Manage messages");
rootCommand.AddCommand(messagesCommand);

// messages send
var messagesSendCommand = new Command("send", "Send a message to an agent or broadcast to all");
var sendFromOption = new Option<Guid>("--from", "Sender agent ID") { IsRequired = true };
var sendToOption = new Option<string>("--to", "Recipient agent ID or 'all' for broadcast") { IsRequired = true };
var sendSubjectOption = new Option<string>("--subject", "Message subject") { IsRequired = true };
var sendBodyOption = new Option<string>("--body", "Message body as JSON string") { IsRequired = true };
var sendReplyToOption = new Option<Guid?>("--reply-to", "Message ID this is a reply to (optional)");
messagesSendCommand.AddOption(sendFromOption);
messagesSendCommand.AddOption(sendToOption);
messagesSendCommand.AddOption(sendSubjectOption);
messagesSendCommand.AddOption(sendBodyOption);
messagesSendCommand.AddOption(sendReplyToOption);
messagesCommand.AddCommand(messagesSendCommand);
messagesSendCommand.SetHandler(async (hubUrl, pretty, from, to, subject, body, replyTo) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.SendMessageAsync(from, to, subject, body, replyTo);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, sendFromOption, sendToOption, sendSubjectOption, sendBodyOption, sendReplyToOption);

// messages inbox
var messagesInboxCommand = new Command("inbox", "Get inbox messages for an agent");
var inboxAgentOption = new Option<Guid>("--agent", "Agent ID") { IsRequired = true };
var inboxAllOption = new Option<bool>("--all", "Include read messages");
messagesInboxCommand.AddOption(inboxAgentOption);
messagesInboxCommand.AddOption(inboxAllOption);
messagesCommand.AddCommand(messagesInboxCommand);
messagesInboxCommand.SetHandler(async (hubUrl, pretty, agentId, all) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.GetInboxAsync(agentId, all);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, inboxAgentOption, inboxAllOption);

// messages outbox
var messagesOutboxCommand = new Command("outbox", "Get sent messages for an agent");
var outboxAgentOption = new Option<Guid>("--agent", "Agent ID") { IsRequired = true };
messagesOutboxCommand.AddOption(outboxAgentOption);
messagesCommand.AddCommand(messagesOutboxCommand);
messagesOutboxCommand.SetHandler(async (hubUrl, pretty, agentId) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.GetOutboxAsync(agentId);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, outboxAgentOption);

// messages read
var messagesReadCommand = new Command("read", "Mark a message as read");
var readMessageIdOption = new Option<Guid>("--id", "Message ID") { IsRequired = true };
messagesReadCommand.AddOption(readMessageIdOption);
messagesCommand.AddCommand(messagesReadCommand);
messagesReadCommand.SetHandler(async (hubUrl, pretty, messageId) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.MarkReadAsync(messageId);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, readMessageIdOption);

// messages attach
var messagesAttachCommand = new Command("attach", "Attach a file to a message");
var attachMessageIdOption = new Option<Guid>("--message", "Message ID") { IsRequired = true };
var attachFileOption = new Option<string>("--file", "Path to the file to attach") { IsRequired = true };
messagesAttachCommand.AddOption(attachMessageIdOption);
messagesAttachCommand.AddOption(attachFileOption);
messagesCommand.AddCommand(messagesAttachCommand);
messagesAttachCommand.SetHandler(async (hubUrl, pretty, messageId, filePath) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        var result = await client.AttachFileAsync(messageId, filePath);
        OutputFormatter.WriteSuccess(result, pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, attachMessageIdOption, attachFileOption);

// messages attachment (download)
var messagesDownloadCommand = new Command("attachment", "Download an attachment from a message");
var downloadMessageIdOption = new Option<Guid>("--message", "Message ID") { IsRequired = true };
var downloadAttachmentIdOption = new Option<Guid>("--attachment", "Attachment ID") { IsRequired = true };
var downloadOutputOption = new Option<string>("--output", "Output file path") { IsRequired = true };
messagesDownloadCommand.AddOption(downloadMessageIdOption);
messagesDownloadCommand.AddOption(downloadAttachmentIdOption);
messagesDownloadCommand.AddOption(downloadOutputOption);
messagesCommand.AddCommand(messagesDownloadCommand);
messagesDownloadCommand.SetHandler(async (hubUrl, pretty, messageId, attachmentId, outputPath) =>
{
    try
    {
        var client = CreateClient(hubUrl);
        await client.DownloadAttachmentAsync(messageId, attachmentId, outputPath);
        OutputFormatter.WriteMessage($"Attachment downloaded to {outputPath}", pretty);
    }
    catch (Exception ex)
    {
        OutputFormatter.WriteError(ex.Message, pretty);
        Environment.Exit(1);
    }
}, hubUrlOption, prettyOption, downloadMessageIdOption, downloadAttachmentIdOption, downloadOutputOption);

// Run
return await rootCommand.InvokeAsync(args);
