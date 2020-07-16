using Fiddler;
using LawinServer.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using static Fiddler.FiddlerApplication;
using static LawinServer.Core.Logger;
using static LawinServer.Win32;

namespace LawinServer.Net
{
    internal class Proxy
    {
        #region VARIABLES
        private const string Proxy_Server = "ProxyServer";

        private const string Proxy_Enable = "ProxyEnable";

        private readonly AppRegistry appRegistry;

        private readonly string proxyKey = @$"{Registry.CurrentUser}\SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings";

        private readonly FiddlerCoreStartupSettings startupSettings;

        private (object _currentProxyServer, int _defaultUserProxyState) proxySettings;

        private (string _fiddlerCert, string _privateKey) _fiddlerCertInfos;

        private int count = 0;
        #endregion

        public Proxy() : this(9999) { } //default port

        public Proxy(ushort port)
        {
            appRegistry = new AppRegistry();

            GetDefaultProxySettingsValue();
            ConfigureFiddlerSettings(out bool fiddlerCertRegKeysExist);
            startupSettings = new FiddlerCoreStartupSettingsBuilder()
                    .ListenOnPort(port)
                    .RegisterAsSystemProxy()
                    .DecryptSSL()
                    .OptimizeThreadPool()
                    .Build();
            if (!CertificateHandler(fiddlerCertRegKeysExist))
            {
                LogError("[Install Manifest Error] Could not install the manifest. Please restart the app and try again!");
                StopProxy();
                Environment.Exit(69);
            }
        }

        #region CONFIGURATION
        private void GetDefaultProxySettingsValue()
        {
            proxySettings._currentProxyServer = Registry.GetValue(proxyKey, Proxy_Server, null);
            proxySettings._defaultUserProxyState = (int)Registry.GetValue(proxyKey, Proxy_Enable, 0);
        }

        private void ConfigureFiddlerSettings(out bool fiddlerCertRegKeysExist)
        {
            CONFIG.IgnoreServerCertErrors = false;
            Prefs.SetBoolPref("fiddler.network.streaming.abortifclientaborts", true);
            Prefs.SetBoolPref("fiddler.certmaker.PreferCertEnroll", true);
            _fiddlerCertInfos._fiddlerCert = appRegistry.GetRegistryValue<string>("FiddlerCert");
            _fiddlerCertInfos._privateKey = appRegistry.GetRegistryValue<string>("PrivateKey");
            fiddlerCertRegKeysExist = _fiddlerCertInfos._fiddlerCert != null && _fiddlerCertInfos._privateKey != null;
            if (fiddlerCertRegKeysExist)
            {
                Prefs.SetStringPref("fiddler.certmaker.bc.cert", _fiddlerCertInfos._fiddlerCert);
                Prefs.SetStringPref("fiddler.certmaker.bc.key", _fiddlerCertInfos._privateKey);
            }
        }

        private bool CertificateHandler(bool valueExist)
        {
            if (!CertMaker.rootCertExists())
            {
                if (!CertMaker.createRootCert())
                {
                    return false;
                }
            }
            bool certificateSuccess = CertMaker.rootCertIsTrusted() || CertMaker.trustRootCert();
            if (!certificateSuccess)
            {
                return false;
            }
            _fiddlerCertInfos._fiddlerCert ??= Prefs.GetStringPref("fiddler.certmaker.bc.cert", null);
            _fiddlerCertInfos._privateKey ??= Prefs.GetStringPref("fiddler.certmaker.bc.key", null);
            if (!valueExist)
            {
                List<RegistryInfo> registryInfo = new List<RegistryInfo>()
                {
                    new RegistryInfo()
                    {
                        Name = "FiddlerCert",
                        Value = _fiddlerCertInfos._fiddlerCert,
                        RegistryValueKind = RegistryValueKind.String
                    },
                    new RegistryInfo()
                    {
                        Name = "PrivateKey",
                        Value = _fiddlerCertInfos._privateKey,
                        RegistryValueKind = RegistryValueKind.String
                    }
                };
                appRegistry.UpdateRegistry(registryInfo);
            }
            return true;
        }
        #endregion

        public void StartProxy()
        {
            appRegistry.Dispose();
            BeforeRequest += OnBeforeRequest;
            AfterSessionComplete += OnAfterSessionComplete;
            Startup(startupSettings);
            Console.WriteLine($"Proxy started listening on port {startupSettings.ListenPort}.");
        }

        public bool StopProxy()
        {
            DeletePrivateKeys();
            return ResetProxySettings();
        }

        #region EVENT_HANDLERS
        private void OnBeforeRequest(Session oSession)
        {
            if (oSession.hostname.Contains(".ol.epicgames.com"))
            {
                if (oSession.HTTPMethodIs("CONNECT"))
                {
                    oSession["x-replywithtunnel"] = "FortniteTunnel";
                    return;
                }
                oSession.fullUrl = "https://lawinserverfinal.herokuapp.com" + oSession.PathAndQuery;

            }
        }

        private void OnAfterSessionComplete(Session oSession)
        {
            if (oSession.hostname != "lawinserverfinal.herokuapp.com")
            {
                return;
            }
            if (oSession.responseCode >= 400)
            {
                string fullUrl = oSession.fullUrl;
                string requestHeaders = oSession.oRequest.headers.ToString();
                int responseCode = oSession.responseCode;

                LogError($"[Endpoint] {fullUrl}\n" +
                        $"[ResponseCode] {responseCode}\n" +
                        $"[RequestHeader] { requestHeaders}\n\n"
                        );
            }
            Console.Title = $"LawinServer - Redirected {++count} endpoints";
        }
        #endregion

        #region CLEANUP
        private bool ResetProxySettings()
        {
            Registry.SetValue(proxyKey, Proxy_Server, proxySettings._currentProxyServer, RegistryValueKind.String);
            if (proxySettings._defaultUserProxyState == 0)
            {
                Registry.SetValue(proxyKey, Proxy_Enable, 0, RegistryValueKind.DWord);
                bool successfulyChanged = InternetSetOption(IntPtr.Zero, InternetOptions.INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
                bool successfulyRefreshed = InternetSetOption(IntPtr.Zero, InternetOptions.INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
                return successfulyChanged && successfulyRefreshed;
            }
            return true;
        }

        private void DeletePrivateKeys()
        {
            if (!(CertMaker.oCertProvider is ICertificateProvider4 certProvider))
            {
                return;
            }
            PrivateKeyDeleters privateKeyDeleter = new PrivateKeyDeleters();
            IDictionary<string, X509Certificate2> certs = certProvider.CertCache;
            foreach (X509Certificate2 cert in certs.Values)
            {
                privateKeyDeleter.DeletePrivateKey(cert.PrivateKey);
            }
        }
        #endregion
    }
}