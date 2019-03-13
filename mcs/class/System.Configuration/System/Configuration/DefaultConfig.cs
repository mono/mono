#if MONO 

using System;
using System.Runtime.CompilerServices;

// DefaultConfig is here for now in order to comply with how the ICall to 
// get_machine_config_path is mapped.  
namespace System.Configuration {
    internal static class DefaultConfig {

        internal static string MachineConfigPath {
            get => get_machine_config_path();
        }

        internal static string BundledMachineConfig {
            get => get_bundled_machine_config();
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static string get_machine_config_path();

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal extern static string get_bundled_machine_config();
    }  
}

#endif