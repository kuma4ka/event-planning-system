FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/EventPlanning.Web/EventPlanning.Web.csproj", "src/EventPlanning.Web/"]
COPY ["src/EventPlanning.Application/EventPlanning.Application.csproj", "src/EventPlanning.Application/"]
COPY ["src/EventPlanning.Domain/EventPlanning.Domain.csproj", "src/EventPlanning.Domain/"]
COPY ["src/EventPlanning.Infrastructure/EventPlanning.Infrastructure.csproj", "src/EventPlanning.Infrastructure/"]
RUN dotnet restore "./src/EventPlanning.Web/EventPlanning.Web.csproj"
COPY . .
WORKDIR "/src/src/EventPlanning.Web"
RUN dotnet build "./EventPlanning.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EventPlanning.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventPlanning.Web.dll"]
