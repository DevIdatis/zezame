FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Zezame.Migrator/Zezame.Migrator.csproj", "Zezame.Migrator/"]
COPY ["Zezame.Domain/Zezame.Domain.csproj", "Zezame.Domain/"]
COPY ["Zezame.Infrastructure/Zezame.Infrastructure.csproj", "Zezame.Infrastructure/"]
RUN dotnet restore "./Zezame.Migrator/Zezame.Migrator.csproj"

COPY . .
WORKDIR "/src/Zezame.Migrator"
# Añade esta línea para diagnosticar
RUN ls -la
RUN dotnet publish "Zezame.Migrator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Zezame.Migrator.dll"]