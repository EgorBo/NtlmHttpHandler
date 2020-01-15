using System;
using System.Net.Http;
using System.Reflection;

namespace NtlmHttpHandler
{
    public class NtlmHttpHandlerFactory
    {
        public static HttpClientHandler Create()
        {
            Type androidenvType = Type.GetType("Android.Runtime.AndroidEnvironment, Mono.Android");
            if (androidenvType == null)
            {
                // For non-Android systems the default HttpClientHandler works fine with NTLM via GSS
                // e.g. GSS.frameworks for macOS and iOS and krb5 on Linux (requires `gss-ntlmssp` package installed)
                return new HttpClientHandler();
            }

            Type monoHandlerType = Type.GetType("System.Net.Http.MonoWebRequestHandler, System.Net.Http");
            if (monoHandlerType == null)
                throw new InvalidOperationException("`System.Net.Http.MonoWebRequestHandler` was not found in System.Net.Http.");

            ConstructorInfo[] internalMonoHandlerCtors = monoHandlerType.GetConstructors(
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (internalMonoHandlerCtors.Length < 1)
                throw new InvalidOperationException("Internal parameter-less constructor for `System.Net.Http.MonoWebRequestHandler` was not found.");

            object internalMonoHandler = internalMonoHandlerCtors[0].Invoke(null);

            ConstructorInfo[] httpClientHandlerCtors =
                typeof(HttpClientHandler).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            if (httpClientHandlerCtors.Length < 1)
                throw new InvalidOperationException("`internal HttpClientHandler(IMonoHttpClientHandler)` constructor was not found in `System.Net.Http.HttpClientHandler`.");

            return (HttpClientHandler)httpClientHandlerCtors[0].Invoke(new [] { internalMonoHandler });
        }
    }
}
