FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /source
COPY ./CachePopulator.sln .

COPY ./CachePopulator/*.csproj ./CachePopulator/
RUN dotnet restore

COPY ./CachePopulator ./CachePopulator

RUN dotnet publish "./CachePopulator/CachePopulator.csproj" --output "../dist" --configuration Release --no-restore

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /source/dist .
EXPOSE 1337
ENTRYPOINT ["dotnet", "CachePopulator.dll"]