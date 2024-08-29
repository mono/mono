//------------------------------------------------------------------------------
// <copyright file="TelemetryLogger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

 namespace System.Web {
    using System.Diagnostics.Tracing;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web.Util;

     [EventData(Name="TargetFrameworkSet")]
    struct TargetFrameworkTelemetryData {
        public string AppID { get; set; }
        public string TargetFramework { get; set; }
    }

     [EventData(Name="HandlerMapped")]
    struct HttpHandlerTelemetryData {
        public string AppID { get; set; }
        public string HttpHandlerType { get; set; }
    }

     [EventData(Name = "ProviderInitialized")]
    struct ProviderTelemetryData {
        public string AppID { get; set; }
        public string ProviderType { get; set; }
    }

     static class TelemetryLogger {
        // eventsource provider name
        private static readonly string WebFormsProviderName = "Microsoft.DOTNET.ASPNET.WebForms";

         // event name
        private static readonly string HttpHandlerEventName = "HandlerMapped";
        private static readonly string TargetFrameworkEventName = "TargetFrameworkSet";
        private static readonly string ProviderEventName = "ProviderInitialized";

         // telemetry eventsource
        private static EventSource s_TelemetryLogger = new TelemetryEventSource(WebFormsProviderName);
        private static readonly string s_AppID = GetAppID();

         public static void LogHttpHandler(Type httpHandlerType) {
            if (httpHandlerType == null) {
                return;
            }

             try { 
            s_TelemetryLogger.Write(
                    HttpHandlerEventName,
                    TelemetryEventSource.MeasuresOptions(),
                    new HttpHandlerTelemetryData() {
                        AppID = s_AppID,
                        HttpHandlerType = GetHashCode(httpHandlerType.AssemblyQualifiedName)
                    }
                );
            }
            catch { }
        }

         public static void LogTargetFramework(Version targetFrameworkVersion) {
            if (targetFrameworkVersion == null) {
                return;
            }

             try {
                s_TelemetryLogger.Write(
                    TargetFrameworkEventName,
                    TelemetryEventSource.MeasuresOptions(),
                    new TargetFrameworkTelemetryData() {
                        AppID = s_AppID,
                        TargetFramework = targetFrameworkVersion.ToString()
                    }
                );
            }
            catch { }
        }

         public static void LogProvider(Type providerType) {
            if (providerType == null) {
                return;
            }

             try {
                s_TelemetryLogger.Write(
                    ProviderEventName,
                   TelemetryEventSource.MeasuresOptions(),
                    new ProviderTelemetryData() {
                        AppID = s_AppID,
                        ProviderType = GetHashCode(providerType.AssemblyQualifiedName)
                    }
                );
            }
            catch { }
        }

         private static string GetAppID() {
            return HttpRuntime.AppDomainAppId == null ? string.Empty : GetHashCode(HttpRuntime.AppDomainAppId);
        }

         private static string GetHashCode(string str) {
            Debug.Assert(str != null);

             var bytes = Encoding.Unicode.GetBytes(str);
            using(var sha256 = new SHA256Managed()) {
                return Convert.ToBase64String(sha256.ComputeHash(bytes));
            }
        }
    }
}