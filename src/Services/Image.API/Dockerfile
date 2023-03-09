FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/Image.API/Image.API.csproj", "src/Services/Image.API/"]
RUN dotnet restore "src/Services/Image.API/Image.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Image.API"
RUN dotnet build "Image.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Image.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Image.API.dll"]
