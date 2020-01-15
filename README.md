# NtlmHttpHandler
NtlmHttpHandler allows to create NTLM-friendly http handlers for HttpClient.
It creates an instance of legacy WebRequest-based HttpHandler for Xamarin.Android and a modern SocketHandler-based `HttpClientHandler` for other platforms.

Nuget: https://www.nuget.org/packages/NtlmHttpHandler/

Usage:
```csharp
var handler = NtlmHttpHandlerFactory.Create();
handler.Credentials = new NetworkCredential("user", "psw", "domain"); // or via CredentialCache
var httpClient = new HttpClient(handler);
```

For `Link All` or `Link SDK assemblies only` modes you need to preserve the following items [via XML](https://docs.microsoft.com/en-us/xamarin/cross-platform/deploy-test/linker):
```xml
<linker>
    <assembly fullname="System.Net.Http">
        <type fullname="System.Net.Http.HttpClientHandler*" />
        <type fullname="System.Net.Http.MonoWebRequestHandler*" />
    </assembly>
</linker>
```

For `Release` mode and Xamarin.Android don't forget to enable "Internet" permission in Android Manifest.
