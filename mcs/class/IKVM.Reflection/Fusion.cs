/*
  Copyright (C) 2010 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace IKVM.Reflection
{
	struct ParsedAssemblyName
	{
		internal string Name;
		internal Version Version;
		internal string Culture;
		internal string PublicKeyToken;
	}

	static class Fusion
	{
		private static readonly bool UseNativeFusion = Environment.OSVersion.Platform == PlatformID.Win32NT && System.Type.GetType("Mono.Runtime") == null && Environment.GetEnvironmentVariable("IKVM_DISABLE_FUSION") == null;

		internal static bool CompareAssemblyIdentity(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			if (UseNativeFusion)
			{
				bool equivalent;
				Marshal.ThrowExceptionForHR(CompareAssemblyIdentity(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out equivalent, out result));
				return equivalent;
			}
			else
			{
				return CompareAssemblyIdentityPure(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out result);
			}
		}

		[DllImport("fusion", CharSet = CharSet.Unicode)]
		private static extern int CompareAssemblyIdentity(string pwzAssemblyIdentity1, bool fUnified1, string pwzAssemblyIdentity2, bool fUnified2, out bool pfEquivalent, out AssemblyComparisonResult pResult);

		private static bool CompareAssemblyIdentityPure(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			ParsedAssemblyName name1;
			ParsedAssemblyName name2;

			if (!ParseAssemblyName(assemblyIdentity1, out name1)
				|| !ParseAssemblyName(assemblyIdentity2, out name2))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new ArgumentException();
			}

			bool partial = IsPartial(name1);

			if ((partial && unified1) || IsPartial(name2))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				throw new ArgumentException();
			}
			if (!name1.Name.Equals(name2.Name, StringComparison.InvariantCultureIgnoreCase))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			if (name1.Name.Equals("mscorlib", StringComparison.InvariantCultureIgnoreCase))
			{
				result = AssemblyComparisonResult.EquivalentFullMatch;
				return true;
			}
			if (partial && name1.Culture == null)
			{
			}
			else if (!name1.Culture.Equals(name2.Culture, StringComparison.InvariantCultureIgnoreCase))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			if (IsStrongNamed(name2))
			{
				if (partial && name1.PublicKeyToken == null)
				{
				}
				else if (name1.PublicKeyToken != name2.PublicKeyToken)
				{
					result = AssemblyComparisonResult.NonEquivalent;
					return false;
				}
				if (partial && name1.Version == null)
				{
					result = AssemblyComparisonResult.EquivalentPartialMatch;
					return true;
				}
				else if (name1.Version < name2.Version)
				{
					if (unified2)
					{
						result = partial ? AssemblyComparisonResult.EquivalentPartialUnified : AssemblyComparisonResult.EquivalentUnified;
						return true;
					}
					else
					{
						result = partial ? AssemblyComparisonResult.NonEquivalentPartialVersion : AssemblyComparisonResult.NonEquivalentVersion;
						return false;
					}
				}
				else if (name1.Version > name2.Version)
				{
					if (unified1)
					{
						result = partial ? AssemblyComparisonResult.EquivalentPartialUnified : AssemblyComparisonResult.EquivalentUnified;
						return true;
					}
					else
					{
						result = partial ? AssemblyComparisonResult.NonEquivalentPartialVersion : AssemblyComparisonResult.NonEquivalentVersion;
						return false;
					}
				}
				else
				{
					result = partial ? AssemblyComparisonResult.EquivalentPartialMatch : AssemblyComparisonResult.EquivalentFullMatch;
					return true;
				}
			}
			else
			{
				result = partial ? AssemblyComparisonResult.EquivalentPartialWeakNamed : AssemblyComparisonResult.EquivalentWeakNamed;
				return true;
			}
		}

		// note that this is the fusion specific parser, it is not the same as System.Reflection.AssemblyName
		private static bool ParseAssemblyName(string fullName, out ParsedAssemblyName parsedName)
		{
			parsedName = new ParsedAssemblyName();
			StringBuilder sb = new StringBuilder();
			int pos = 0;
			while (pos < fullName.Length)
			{
				char ch = fullName[pos++];
				if (ch == '\\')
				{
					if (pos == fullName.Length)
					{
						return false;
					}
					ch = fullName[pos++];
				}
				else if (ch == ',')
				{
					break;
				}
				sb.Append(ch);
			}
			parsedName.Name = sb.ToString().Trim();
			if (pos < fullName.Length)
			{
				string[] parts = fullName.Substring(pos).Split(',');
				for (int i = 0; i < parts.Length; i++)
				{
					string[] kv = parts[i].Split('=');
					if (kv.Length != 2)
					{
						return false;
					}
					switch (kv[0].Trim().ToLowerInvariant())
					{
						case "version":
							if (parsedName.Version != null)
							{
								return false;
							}
							if (!ParseVersion(kv[1].Trim(), out parsedName.Version))
							{
								return false;
							}
							break;
						case "culture":
							if (parsedName.Culture != null)
							{
								return false;
							}
							if (!ParseCulture(kv[1].Trim(), out parsedName.Culture))
							{
								return false;
							}
							break;
						case "publickeytoken":
							if (parsedName.PublicKeyToken != null)
							{
								return false;
							}
							if (!ParsePublicKeyToken(kv[1].Trim(), out parsedName.PublicKeyToken))
							{
								return false;
							}
							break;
						case "publickey":
							if (parsedName.PublicKeyToken != null)
							{
								return false;
							}
							if (!ParsePublicKey(kv[1].Trim(), out parsedName.PublicKeyToken))
							{
								return false;
							}
							break;
					}
				}
			}
			return true;
		}

		private static bool ParseVersion(string str, out Version version)
		{
			string[] parts = str.Split('.');
			if (parts.Length == 4)
			{
				ushort major, minor, build, revision;
				if (ushort.TryParse(parts[0], System.Globalization.NumberStyles.Integer, null, out major)
					&& ushort.TryParse(parts[1], System.Globalization.NumberStyles.Integer, null, out minor)
					&& ushort.TryParse(parts[2], System.Globalization.NumberStyles.Integer, null, out build)
					&& ushort.TryParse(parts[3], System.Globalization.NumberStyles.Integer, null, out revision))
				{
					version = new Version(major, minor, build, revision);
					return true;
				}
			}
			version = null;
			return false;
		}

		private static bool ParseCulture(string str, out string culture)
		{
			if (str == null)
			{
				culture = null;
				return false;
			}
			culture = str;
			return true;
		}

		private static bool ParsePublicKeyToken(string str, out string publicKeyToken)
		{
			if (str == null)
			{
				publicKeyToken = null;
				return false;
			}
			publicKeyToken = str.ToLowerInvariant();
			return true;
		}

		private static bool ParsePublicKey(string str, out string publicKeyToken)
		{
			if (str == null)
			{
				publicKeyToken = null;
				return false;
			}
			// HACK use AssemblyName to convert PublicKey to PublicKeyToken
			byte[] token = new AssemblyName("Foo, PublicKey=" + str).GetPublicKeyToken();
			StringBuilder sb = new StringBuilder(token.Length * 2);
			for (int i = 0; i < token.Length; i++)
			{
				sb.AppendFormat("{0:x2}", token[i]);
			}
			publicKeyToken = sb.ToString();
			return true;
		}

		private static bool IsPartial(ParsedAssemblyName name)
		{
			return name.Version == null || name.Culture == null || name.PublicKeyToken == null;
		}

		private static bool IsStrongNamed(ParsedAssemblyName name)
		{
			return name.PublicKeyToken != null && name.PublicKeyToken != "null";
		}

		private static bool IsEqual(byte[] b1, byte[] b2)
		{
			if (b1.Length != b2.Length)
			{
				return false;
			}
			for (int i = 0; i < b1.Length; i++)
			{
				if (b1[i] != b2[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
