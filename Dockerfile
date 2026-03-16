FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/FridayPos.Cloud66.Poc/FridayPos.Cloud66.Poc.csproj src/FridayPos.Cloud66.Poc/
RUN dotnet restore src/FridayPos.Cloud66.Poc/FridayPos.Cloud66.Poc.csproj

COPY src/FridayPos.Cloud66.Poc/. src/FridayPos.Cloud66.Poc/
RUN dotnet publish src/FridayPos.Cloud66.Poc/FridayPos.Cloud66.Poc.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FridayPos.Cloud66.Poc.dll"]
