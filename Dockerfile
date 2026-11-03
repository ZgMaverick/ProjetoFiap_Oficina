# Etapa 1: compila e publica
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Oficina.Api/Oficina.Api.csproj -c Release -o /app

# Etapa 2: imagem final, só com o runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Oficina.Api.dll"]
