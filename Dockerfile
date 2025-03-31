# SDK imajı
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["TentecimApi.csproj", "./"]
RUN dotnet restore "TentecimApi.csproj"
COPY . .
RUN dotnet build "TentecimApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TentecimApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TentecimApi.dll"]
