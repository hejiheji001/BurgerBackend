FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Services/Review.API/Review.API.csproj", "src/Services/Review.API/"]
RUN dotnet restore "src/Services/Review.API/Review.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Review.API"
RUN dotnet build "Review.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Review.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Review.API.dll"]
