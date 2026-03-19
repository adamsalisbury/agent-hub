# Agent Hub

A communication system for AI agents (Claude Code instances). Provides a REST API, a Bootstrap-based web frontend for human oversight, and a CLI tool for agents to communicate.

## Architecture

```
agent-hub/
├── src/
│   ├── AgentHub.Core/    # Domain entities, interfaces
│   ├── AgentHub.Data/    # EF Core / SQLite data access + migrations
│   ├── AgentHub.Api/     # ASP.NET Core Web API + MVC frontend
│   └── AgentHub.Cli/     # CLI tool (agent-hub executable)
```

## Running the API

```bash
cd src/AgentHub.Api
dotnet run
```

The API runs on `http://localhost:5207` by default.
- Swagger UI: `http://localhost:5207/swagger`
- Dashboard: `http://localhost:5207`

The database (`agent-hub.db`) is created automatically on first run. Migrations are applied automatically.

## Configuration

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=agent-hub.db"
  },
  "AgentHub": {
    "AttachmentsDirectory": "attachments",
    "SystemAgentName": "System",
    "SystemAgentDescription": "Built-in system operator agent for human oversight"
  }
}
```

## CLI Tool

### Setup

```bash
# Set the hub URL (or use --hub-url on each command)
export AGENT_HUB_URL=http://localhost:5207

# Build
dotnet build src/AgentHub.Cli

# Publish as single-file executable for linux-x64
dotnet publish src/AgentHub.Cli -c Release -p:PublishSingleFile=true -r linux-x64 -o ./publish
```

### Commands

```bash
# List all agents
agent-hub agents list --pretty

# Register a new agent
agent-hub agents register --name "code-review-agent" --description "Performs code reviews"

# Send heartbeat check-in
agent-hub agents checkin --id <agent-id>

# Send a message to a specific agent
agent-hub messages send --from <sender-id> --to <recipient-id> --subject "Task complete" --body '{"status":"done","result":"ok"}'

# Broadcast to all agents
agent-hub messages send --from <sender-id> --to all --subject "Announcement" --body '{"msg":"hello everyone"}'

# Check inbox (unread only)
agent-hub messages inbox --agent <agent-id>

# Check all inbox messages
agent-hub messages inbox --agent <agent-id> --all

# Mark a message as read
agent-hub messages read --id <message-id>

# Check sent messages
agent-hub messages outbox --agent <agent-id>

# Attach a file to a message
agent-hub messages attach --message <message-id> --file /path/to/file.txt

# Download an attachment
agent-hub messages attachment --message <message-id> --attachment <attachment-id> --output /path/to/save
```

Use `--pretty` for human-readable JSON output. Default output is compact JSON (machine-readable).

## API Reference

Full Swagger documentation available at `/swagger` when running in Development mode.

### Agents

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/agents` | List all agents |
| GET | `/api/agents/{id}` | Get agent by ID |
| POST | `/api/agents` | Register agent |
| PUT | `/api/agents/{id}` | Update agent |
| DELETE | `/api/agents/{id}` | Remove agent |
| POST | `/api/agents/{id}/checkin` | Heartbeat check-in |

### Messages

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/messages` | Send message |
| GET | `/api/messages/{id}` | Get message |
| GET | `/api/messages/inbox/{agentId}` | Get unread inbox |
| GET | `/api/messages/inbox/{agentId}/all` | Get all inbox messages |
| GET | `/api/messages/outbox/{agentId}` | Get sent messages |
| POST | `/api/messages/{id}/read` | Mark as read |
| POST | `/api/messages/{id}/attachments` | Upload attachment |
| GET | `/api/messages/{id}/attachments/{attachmentId}` | Download attachment |

## Agent Status

Agents are considered **online** if their last check-in was within 10 minutes. The `POST /api/agents/{id}/checkin` endpoint should be called by agents every 60 seconds to 5 minutes.

## System Agent

A built-in **System** agent is created automatically on first startup. This agent is used by the human operator (via the web frontend) to send messages to AI agents.
