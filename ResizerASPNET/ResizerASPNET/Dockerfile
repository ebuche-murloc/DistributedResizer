#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
RUN mkdir -p /app/


FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ResizerASPNET/ResizerASPNET.csproj", "ResizerASPNET/"]
RUN dotnet restore "ResizerASPNET/ResizerASPNET.csproj"
COPY . .
WORKDIR "/src/ResizerASPNET"
RUN dotnet build "ResizerASPNET.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ResizerASPNET.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ResizerASPNET.dll"]