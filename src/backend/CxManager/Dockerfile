FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia configurações de compilação e gerenciamento central de pacotes (CPM)
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

# Copia arquivos de projeto para cache de restauração
COPY ["CxManager.Api/CxManager.Api.csproj", "CxManager.Api/"]
COPY ["CxManager.Application/CxManager.Application.csproj", "CxManager.Application/"]
COPY ["CxManager.Contract/CxManager.Contract.csproj", "CxManager.Contract/"]
COPY ["CxManager.Domain/CxManager.Domain.csproj", "CxManager.Domain/"]
COPY ["CxManager.Infrastructure/CxManager.Infrastructure.csproj", "CxManager.Infrastructure/"]
COPY ["CxManager.ServiceDefaults/CxManager.ServiceDefaults.csproj", "CxManager.ServiceDefaults/"]

RUN dotnet restore "CxManager.Api/CxManager.Api.csproj"

# Copia código-fonte e compila para produção
COPY . .
WORKDIR "/src/CxManager.Api"
RUN dotnet publish "CxManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagem de execução ASP.NET 10
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CxManager.Api.dll"]
