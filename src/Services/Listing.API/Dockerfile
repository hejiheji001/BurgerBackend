FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/Listing.API/Listing.API.csproj", "src/Services/Listing.API/"]
RUN dotnet restore "src/Services/Listing.API/Listing.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Listing.API"
RUN dotnet build "Listing.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Listing.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Listing.API.dll"]
