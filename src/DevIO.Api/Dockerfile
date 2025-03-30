# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/DevIO.Api/DevIO.Api.csproj", "src/DevIO.Api/"]
COPY ["src/DevIO.Business/DevIO.Business.csproj", "src/DevIO.Business/"]
COPY ["src/DevIO.Data/DevIO.Data.csproj", "src/DevIO.Data/"]
RUN dotnet restore "./src/DevIO.Api/DevIO.Api.csproj"
COPY . .
WORKDIR "/src/src/DevIO.Api"
RUN dotnet build "./DevIO.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DevIO.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENV ASPNETCORE_URLS=http://*:80
ENTRYPOINT ["dotnet", "DevIO.Api.dll"]