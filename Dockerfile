# TODO: Build and test in a container also, see https://www.hanselman.com/blog/NETAndDocker.aspx
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY ./publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "Hjerpbakk.Media.Server.dll"]