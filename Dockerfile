FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY busline_project.slnx ./
COPY busline_project/ ./busline_project/

RUN dotnet restore busline_project/busline_project.csproj
RUN dotnet build busline_project/busline_project.csproj -c Release --no-restore
RUN dotnet publish busline_project/busline_project.csproj -c Release --no-build -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "busline_project.dll"]
