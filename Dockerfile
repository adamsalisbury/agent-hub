FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY AgentHub.sln .
COPY src/AgentHub.Core/AgentHub.Core.csproj src/AgentHub.Core/
COPY src/AgentHub.Data/AgentHub.Data.csproj src/AgentHub.Data/
COPY src/AgentHub.Api/AgentHub.Api.csproj src/AgentHub.Api/
COPY src/AgentHub.Cli/AgentHub.Cli.csproj src/AgentHub.Cli/

RUN dotnet restore

# Copy everything and build
COPY . .
RUN dotnet publish src/AgentHub.Api/AgentHub.Api.csproj -c Release -o /app/api --no-restore
RUN dotnet publish src/AgentHub.Cli/AgentHub.Cli.csproj -c Release -o /app/cli --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy published outputs
COPY --from=build /app/api ./api
COPY --from=build /app/cli ./cli

# Make the CLI tool available on PATH
RUN ln -s /app/cli/agent-hub /usr/local/bin/agent-hub

# Create directories for data and attachments
RUN mkdir -p /app/data /app/attachments

# Expose both ports - API on 5050, Web frontend on 5060
EXPOSE 5050 5060

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://+:5050;http://+:5060"
ENV AgentHub__DataDirectory=/app/data
ENV AgentHub__AttachmentsDirectory=/app/attachments

ENTRYPOINT ["dotnet", "/app/api/AgentHub.Api.dll"]
