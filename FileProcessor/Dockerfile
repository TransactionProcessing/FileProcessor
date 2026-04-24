FROM stuartferguson/txnprocbase:latest AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
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
# Ensure runtime directories exist and are writable by the non-root user (see nlog.config and appsettings.json)
RUN mkdir -p /home/txnproc/config \
             /home/txnproc/trace \
             /home/txnproc/bulkfiles/temporary \
             /home/txnproc/bulkfiles/safaricom \
             /home/txnproc/bulkfiles/voucher \
    && chown -R 10001:10001 /home/txnproc

FROM mkdir AS final
WORKDIR /app
COPY --from=publish /app/publish .
# If anything writes under /app at runtime (temp keys, diagnostics, etc.), avoid EACCES under USER 10001:
RUN chown -R 10001:10001 /app
USER 10001
ENTRYPOINT ["dotnet", "FileProcessor.dll"]
