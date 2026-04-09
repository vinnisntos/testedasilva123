# Estágio de Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# O segredo está aqui: estamos copiando o arquivo de DENTRO da pasta
COPY ["Mocidade015/Mocidade015.csproj", "Mocidade015/"]
RUN dotnet restore "Mocidade015/Mocidade015.csproj"

# Copia o resto dos arquivos
COPY . .

# Entra na pasta do projeto para compilar
WORKDIR "/src/Mocidade015"
RUN dotnet publish "Mocidade015.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio de Execução
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Render usa porta variável, vamos deixar a 8080 como padrão
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Mocidade015.dll"]