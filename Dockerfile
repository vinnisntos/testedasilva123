# Estágio de Build - Usando SDK 10.0
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia o arquivo de projeto
COPY ["Mocidade015/Mocidade015.csproj", "Mocidade015/"]
RUN dotnet restore "Mocidade015/Mocidade015.csproj"

# Copia o restante dos arquivos
COPY . .

# Compila o projeto
WORKDIR "/src/Mocidade015"
RUN dotnet publish "Mocidade015.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Execução - Usando Runtime 10.0
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Configuração de porta para o Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mocidade015.dll"]