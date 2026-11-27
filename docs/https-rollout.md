# FileProcessor HTTPS deployment

FileProcessor serves both HTTP and HTTPS during the private-CA rollout. HTTP stays available for existing callers. The listener ports are 8009 (development HTTP), 8001 (development HTTPS), 5009 (staging HTTP), and 5001 (staging HTTPS). Production continues to use its existing HTTP listener until its rollout is scheduled.

Kestrel loads a CA-issued, password-protected PFX through `TransactionProcessing.CertificateDefaults`. Development and staging deploy jobs require these GitHub environment secrets:

- `FILEPROCESSOR_CERTIFICATE_PATH`: absolute path to the issued PFX on that service host.
- `FILEPROCESSOR_CERTIFICATE_PASSWORD`: PFX password.

The development PFX SAN must match `192.168.1.86`; staging must match `192.168.1.163`. The deployment jobs put the values in the Windows service's environment and verify the file exists before replacing the service. The service identity must be able to read the PFX. Install the public CA root in the service host's Windows trust store before deployment. Never commit a certificate, password, or private key.

For local development, set `ServiceOptions__KestrelOptions__Path` and `ServiceOptions__KestrelOptions__Password` in the process environment or user secrets to enable HTTPS. Without certificate settings, Kestrel uses HTTP only; production can therefore remain on its existing listener while the rollout is in progress.

## Validation

The development and staging deployment jobs probe `/health` over HTTPS with the platform's normal certificate validation. A successful TLS connection confirms that the host trusts the presented chain and that the certificate matches the probe IP. An HTTP health status can still report dependency health separately.

From a trusted Windows client, repeat the endpoint check:

```powershell
curl.exe --verbose https://192.168.1.86:8001/health
curl.exe --verbose https://192.168.1.163:5001/health
```

Confirm the checks fail when the CA root is absent from the client's trust store and when connecting to the same server IP with a hostname that is not in the certificate SAN. Do not use a TLS validation bypass. The CA client setup and current development/staging certificate SANs are documented in [SharedConfiguration's development CA setup](https://github.com/TransactionProcessing/SharedConfiguration/blob/main/docs/DevelopmentCa-Client-Setup.md).

HealthMonitoring keeps its existing URL until the SupportTools follow-up. EventStore settings are unchanged. Outbound service URLs are handled by FileProcessor #894.
