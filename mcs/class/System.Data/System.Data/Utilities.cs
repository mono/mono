using System;

static class Utilities {
	internal static object CreateInstance(Type type) {
#if !WINDOWS_PHONE && !NETFX_CORE
		return Activator.CreateInstance (type, true);
#else
		return Activator.CreateInstance (type);
#endif
	}
}
