using System;
using System.Net.Http;
using System.Reflection;

namespace NtlmHttpHandler
{
    public class NtlmHttpHandlerFactory
    {
        public const string LinkerTipText = @"

Make sure you you've added the following types to your linker.xml:

<linker>
    <assembly fullname=""System.Net.Http"">
        <type fullname=""System.Net.Http.HttpClientHandler*"" />
        <type fullname=""System.Net.Http.MonoWebRequestHandler*"" />
    </assembly>
</linker>";

        public static HttpClientHandler Create(bool ntlmV2Only = false)
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
                throw new InvalidOperationException("`System.Net.Http.MonoWebRequestHandler` was not found in System.Net.Http." + LinkerTipText);

            ConstructorInfo[] internalMonoHandlerCtors = monoHandlerType.GetConstructors(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (internalMonoHandlerCtors.Length < 1)
                throw new InvalidOperationException("Internal parameter-less constructor for `System.Net.Http.MonoWebRequestHandler` was not found." + LinkerTipText);

            if (ntlmV2Only)
            {
                /* The MonoWebRequestHandler will look at the static NtlmSettings.DefaultAuthLevel property when determining
                 * which LM/NTLM authentication level to use.
                 * 
                 * When the server doing the NTLM authentication has been set up to NTLM V2 Only and to refuse LM and NTLM then authentication will fail unless the AuthLevel of the client has been set to NTLMv2Only.
                 */
                var ntlmSettingsType = Type.GetType("Mono.Security.Protocol.Ntlm.NtlmSettings, Mono.Security");
                var defaultAuthLevelProperty = ntlmSettingsType?.GetProperty("DefaultAuthLevel", BindingFlags.Public | BindingFlags.Static);
                if (defaultAuthLevelProperty != null)
                {
                    /* Auth level enum values: LM_and_NTLM, LM_and_NTLM_and_try_NTLMv2_Session, NTLM_only, NTLMv2_only */
                    defaultAuthLevelProperty.SetValue(null, Enum.Parse(defaultAuthLevelProperty.PropertyType, "NTLMv2_only", ignoreCase: true));
                }
            }

            object internalMonoHandler = internalMonoHandlerCtors[0].Invoke(null);

            ConstructorInfo[] httpClientHandlerCtors =
                typeof(HttpClientHandler).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);

            if (httpClientHandlerCtors.Length < 1)
                throw new InvalidOperationException("`internal HttpClientHandler(IMonoHttpClientHandler)` constructor was not found in `System.Net.Http.HttpClientHandler`." + LinkerTipText);

            return (HttpClientHandler)httpClientHandlerCtors[0].Invoke(new [] { internalMonoHandler });
        }
    }
}
