FROM microsoft/aspnetcore-build:2.1.300-preview1 AS builder
WORKDIR /source
COPY ./CachePopulator.sln .

COPY ./CachePopulator/*.csproj ./CachePopulator/
RUN dotnet restore

COPY ./CachePopulator ./CachePopulator

RUN dotnet publish "./CachePopulator/CachePopulator.csproj" --output "../dist" --configuration Release --no-restore

FROM microsoft/aspnetcore:2.1.0-preview1
WORKDIR /app
COPY --from=builder /source/dist .
EXPOSE 5000
ENTRYPOINT ["dotnet", "CachePopulator.dll"]