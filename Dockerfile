FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY . .
WORKDIR "/src/app/StateStores.App.Blazor"
RUN dotnet build "StateStores.App.Blazor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StateStores.App.Blazor.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "StateStores.App.Blazor.dll"]
