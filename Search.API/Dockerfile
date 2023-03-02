FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Search/Search.csproj", "Search/"]
RUN dotnet restore "Search/Search.csproj"
COPY . .
WORKDIR "/src/Search"
RUN dotnet build "Search.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Search.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Search.dll"]
