FROM microsoft/aspnetcore-build:2.1.300-preview1 AS builder
WORKDIR /source
COPY ./SlackIntegration.sln .

COPY ./SlackIntegration/*.csproj ./SlackIntegration/
RUN dotnet restore

COPY ./SlackIntegration ./SlackIntegration

RUN dotnet publish "./SlackIntegration/SlackIntegration.csproj" --output "../dist" --configuration Release --no-restore

FROM microsoft/aspnetcore:2.1.0-preview1
WORKDIR /app
COPY --from=builder /source/dist .
EXPOSE 1338
ENTRYPOINT ["dotnet", "SlackIntegration.dll"]