FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Install Node.js
RUN curl -fsSL https://deb.nodesource.com/setup_14.x | bash - \
    && apt-get install -y \
        nodejs \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src
COPY ["Burger Backend/Burger Backend.csproj", "Burger Backend/"]
RUN dotnet restore "Burger Backend/Burger Backend.csproj"
COPY . .
WORKDIR "/src/Burger Backend"
RUN dotnet build "Burger Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Burger Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Burger Backend.dll"]
