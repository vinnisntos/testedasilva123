# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Mocidade015.csproj", "./"]
RUN dotnet restore "Mocidade015.csproj"
COPY . .
RUN dotnet publish "Mocidade015.csproj" -c Release -o /app/publish

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render usa uma porta variável, o .NET precisa ouvir nela
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mocidade015.dll"]