#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM stuartferguson/txnprocbase:latest AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["FileProcessor/NuGet.Config", "."]
COPY ["FileProcessor/FileProcessor.csproj", "FileProcessor/"]
RUN dotnet restore "FileProcessor/FileProcessor.csproj"
COPY . .
WORKDIR "/src/FileProcessor"
RUN dotnet build "FileProcessor.csproj" -c Release -o /app/build

# Sort out certificate stuff here
RUN openssl x509 -inform DER -in /src/FileProcessor/aspnetapp-root-cert.cer -out /src/FileProcessor/aspnetapp-root-cert.crt
RUN cp /src/FileProcessor/aspnetapp-root-cert.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates

FROM build AS publish
RUN dotnet publish "FileProcessor.csproj" -c Release -o /app/publish

FROM base AS mkdir
WORKDIR /home
RUN mkdir -p /home/txnproc/bulkfiles

FROM mkdir AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileProcessor.dll"]