namespace System.IO.Compression
{
    using System.Diagnostics;

    internal enum CompressionTracingSwitchLevel {
        Off = 0,
        Informational = 1,
        Verbose = 2
    }

    // No tracing on Silverlight nor Windows Phone 7.
    internal class CompressionTracingSwitch
#if !FEATURE_NETCORE
        : Switch
#endif // !FEATURE_NETCORE
    {
        internal readonly static CompressionTracingSwitch tracingSwitch =
            new CompressionTracingSwitch("CompressionSwitch", "Compression Library Tracing Switch");

        internal CompressionTracingSwitch(string displayName, string description)
#if !FEATURE_NETCORE
            : base(displayName, description)
#endif // !FEATURE_NETCORE
        {
        }

        public static bool Verbose {
            get {
#if FEATURE_NETCORE
                return false;
#else
                return tracingSwitch.SwitchSetting >= (int)CompressionTracingSwitchLevel.Verbose;
#endif
            }
        }

        public static bool Informational {
            get {
#if FEATURE_NETCORE
                return false;
#else
                return tracingSwitch.SwitchSetting >= (int)CompressionTracingSwitchLevel.Informational;
#endif
            }
        }

#if ENABLE_TRACING
        public void SetSwitchSetting(CompressionTracingSwitchLevel level) {
            if (level < CompressionTracingSwitchLevel.Off || level > CompressionTracingSwitchLevel.Verbose) {
                throw new ArgumentOutOfRangeException("level");
            }
            this.SwitchSetting = (int)level;
        }
#endif

    }    
}

