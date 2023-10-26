#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
ENV ASPNETCORE_ENVIRONMENT = Development
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BalanceBackEnd/BalanceBackEnd.csproj", "BalanceBackEnd/"]
RUN dotnet restore "BalanceBackEnd/BalanceBackEnd.csproj"
COPY . .
WORKDIR "/src/BalanceBackEnd"
RUN dotnet build "BalanceBackEnd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BalanceBackEnd.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BalanceBackEnd.dll", "--environment=Development"]