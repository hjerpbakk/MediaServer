FROM microsoft/aspnetcore-build:2.1.300-preview1 AS builder
WORKDIR /source
COPY ./MediaServer.sln .

COPY ./MediaServer/*.csproj ./MediaServer/
RUN dotnet restore

COPY ./MediaServer ./MediaServer

RUN dotnet publish "./MediaServer/MediaServer.csproj" --output "../dist" --configuration Release --no-restore

FROM microsoft/aspnetcore:2.1.0-preview1
WORKDIR /app
COPY --from=builder /source/dist .
EXPOSE 5000
ENTRYPOINT ["dotnet", "MediaServer.dll"]