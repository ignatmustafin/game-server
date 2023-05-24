FROM mcr.microsoft.com/dotnet/sdk:7.0 AS migrator
WORKDIR /src
RUN dotnet tool install --global dotnet-ef

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["GameServer.csproj", "./"]
RUN dotnet restore "GameServer.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "GameServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GameServer.csproj" -c Release -o /app/publish
