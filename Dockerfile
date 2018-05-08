FROM microsoft/dotnet:2.1-sdk AS builder
WORKDIR /source
COPY ./MediaServer.sln .

COPY ./MediaServer/*.csproj ./MediaServer/
RUN dotnet restore

COPY ./MediaServer ./MediaServer

RUN dotnet publish "./MediaServer/MediaServer.csproj" --output "../dist" --configuration Release --no-restore

FROM microsoft/dotnet:2.1-aspnetcore-runtime
WORKDIR /app
COPY --from=builder /source/dist .
EXPOSE 5000
ENTRYPOINT ["dotnet", "MediaServer.dll"]