#!/bin/bash
rm -r ./publish
set -e
dotnet publish src/Hjerpbakk.Media.Server/Hjerpbakk.Media.Server.csproj -o ../../publish -c Release
docker build -t mediaserver .