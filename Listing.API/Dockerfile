FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Listing/Listing.csproj", "Listing/"]
RUN dotnet restore "Listing/Listing.csproj"
COPY . .
WORKDIR "/src/Listing"
RUN dotnet build "Listing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Listing.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Listing.dll"]
