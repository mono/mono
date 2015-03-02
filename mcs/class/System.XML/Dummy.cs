using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Schema;

namespace System.Xml
{
	static partial class Res
	{
		public static string GetString (string s, params object [] args)
		{
			return args == null ? s : string.Format (s, args);
		}
	}
}

namespace System.Xml.Utils
{
	static partial class Res
	{
		public static string GetString (string s, params object [] args)
		{
			return args == null || args.Length == 0 ? s : string.Format (s, args);
		}
	}
}

// workaround for missing members in corlib.
namespace System.Security.Policy
{
	static class EvidenceExtensions
	{
		public static void AddHostEvidence (this Evidence evidence, Url url)
		{
			throw new NotImplementedException ();
		}
		public static void AddHostEvidence (this Evidence evidence, Zone zone)
		{
			throw new NotImplementedException ();
		}
		public static void AddHostEvidence (this Evidence evidence, Site site)
		{
			throw new NotImplementedException ();
		}
		public static void AddHostEvidence (this Evidence evidence, EvidenceBase e)
		{
			throw new NotImplementedException ();
		}
	}
}
