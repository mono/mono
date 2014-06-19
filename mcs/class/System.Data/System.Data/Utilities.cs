using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

static class Utilities {
	internal static object CreateInstance(Type type) {
#if !WINDOWS_PHONE && !NETFX_CORE
		return Activator.CreateInstance (type, true);
#else
		return Activator.CreateInstance (type);
#endif
	}
}
