# NtlmHttpHandler
NtlmHttpHandler allows to create NTLM-friendly http handlers for HttpClient.

Nuget: https://www.nuget.org/packages/NtlmHttpHandler/

Usage:
```csharp
var handler = NtlmHttpHandlerFactory.Create();
handler.Credentials = ...;
var httpClient = new HttpClient(handler);
```
