FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /source
COPY ./SlackIntegration.sln .

COPY ./SlackIntegration/*.csproj ./SlackIntegration/
RUN dotnet restore

COPY ./SlackIntegration ./SlackIntegration

RUN dotnet publish "./SlackIntegration/SlackIntegration.csproj" --output "../dist" --configuration Release --no-restore

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /source/dist .
EXPOSE 1338
ENTRYPOINT ["dotnet", "SlackIntegration.dll"]