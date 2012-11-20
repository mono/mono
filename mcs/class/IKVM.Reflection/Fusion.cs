/*
  Copyright (C) 2010-2012 Jeroen Frijters
  Copyright (C) 2011 Marek Safar

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
		internal bool? Retargetable;
		internal ProcessorArchitecture ProcessorArchitecture;
		internal bool HasPublicKey;
		internal bool WindowsRuntime;
	}

	enum ParseAssemblyResult
	{
		OK,
		GenericError,
		DuplicateKey,
	}

	static class Fusion
	{
		internal static bool CompareAssemblyIdentityNative(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			bool equivalent;
			Marshal.ThrowExceptionForHR(CompareAssemblyIdentity(assemblyIdentity1, unified1, assemblyIdentity2, unified2, out equivalent, out result));
			return equivalent;
		}

		[DllImport("fusion", CharSet = CharSet.Unicode)]
		private static extern int CompareAssemblyIdentity(string pwzAssemblyIdentity1, bool fUnified1, string pwzAssemblyIdentity2, bool fUnified2, out bool pfEquivalent, out AssemblyComparisonResult pResult);

		// internal for use by mcs
		internal static bool CompareAssemblyIdentityPure(string assemblyIdentity1, bool unified1, string assemblyIdentity2, bool unified2, out AssemblyComparisonResult result)
		{
			ParsedAssemblyName name1;
			ParsedAssemblyName name2;

			ParseAssemblyResult r = ParseAssemblyName(assemblyIdentity1, out name1);
			if (r != ParseAssemblyResult.OK || (r = ParseAssemblyName(assemblyIdentity2, out name2)) != ParseAssemblyResult.OK)
			{
				result = AssemblyComparisonResult.NonEquivalent;
				switch (r)
				{
					case ParseAssemblyResult.DuplicateKey:
						throw new System.IO.FileLoadException();
					case ParseAssemblyResult.GenericError:
					default:
						throw new ArgumentException();
				}
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
				else if (IsFrameworkAssembly(name2))
				{
					result = partial ? AssemblyComparisonResult.EquivalentPartialFXUnified : AssemblyComparisonResult.EquivalentFXUnified;
					return true;
				}
				else if (name1.Version.Revision == -1 || name2.Version.Revision == -1)
				{
					result = AssemblyComparisonResult.NonEquivalent;
					throw new ArgumentException();
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
			else if (IsStrongNamed(name1))
			{
				result = AssemblyComparisonResult.NonEquivalent;
				return false;
			}
			else
			{
				result = partial ? AssemblyComparisonResult.EquivalentPartialWeakNamed : AssemblyComparisonResult.EquivalentWeakNamed;
				return true;
			}
		}

		static bool IsFrameworkAssembly(ParsedAssemblyName name)
		{
			// A list of FX assemblies which require some form of remapping
			// When 4.0 + 1 version  is release, assemblies introduced in v4.0
			// will have to be added
			switch (name.Name)
			{
				case "System":
				case "System.Core":
				case "System.Data":
				case "System.Data.DataSetExtensions":
				case "System.Data.Linq":
				case "System.Data.OracleClient":
				case "System.Data.Services":
				case "System.Data.Services.Client":
				case "System.IdentityModel":
				case "System.IdentityModel.Selectors":
				case "System.Runtime.Remoting":
				case "System.Runtime.Serialization":
				case "System.ServiceModel":
				case "System.Transactions":
				case "System.Windows.Forms":
				case "System.Xml":
				case "System.Xml.Linq":
					return name.PublicKeyToken == "b77a5c561934e089";

				case "System.Configuration":
				case "System.Configuration.Install":
				case "System.Design":
				case "System.DirectoryServices":
				case "System.Drawing":
				case "System.Drawing.Design":
				case "System.EnterpriseServices":
				case "System.Management":
				case "System.Messaging":
				case "System.Runtime.Serialization.Formatters.Soap":
				case "System.Security":
				case "System.ServiceProcess":
				case "System.Web":
				case "System.Web.Mobile":
				case "System.Web.Services":
					return name.PublicKeyToken == "b03f5f7f11d50a3a";

				case "System.ComponentModel.DataAnnotations":
				case "System.ServiceModel.Web":
				case "System.Web.Abstractions":
				case "System.Web.Extensions":
				case "System.Web.Extensions.Design":
				case "System.Web.DynamicData":
				case "System.Web.Routing":
					return name.PublicKeyToken == "31bf3856ad364e35";
			}

			return false;
		}

		internal static ParseAssemblyResult ParseAssemblySimpleName(string fullName, out int pos, out string simpleName)
		{
			StringBuilder sb = new StringBuilder();
			pos = 0;
			simpleName = null;
			while (pos < fullName.Length && char.IsWhiteSpace(fullName[pos]))
			{
				pos++;
			}
			char quoteOrComma = ',';
			if (pos < fullName.Length && (fullName[pos] == '\"' || fullName[pos] == '\''))
			{
				quoteOrComma = fullName[pos++];
			}
			while (pos < fullName.Length)
			{
				char ch = fullName[pos++];
				if (ch == '\\')
				{
					if (pos == fullName.Length)
					{
						return ParseAssemblyResult.GenericError;
					}
					ch = fullName[pos++];
					if (ch == '\\')
					{
						return ParseAssemblyResult.GenericError;
					}
				}
				else if (ch == quoteOrComma)
				{
					if (ch != ',')
					{
						while (pos != fullName.Length)
						{
							ch = fullName[pos++];
							if (ch == ',')
							{
								break;
							}
							if (!char.IsWhiteSpace(ch))
							{
								return ParseAssemblyResult.GenericError;
							}
						}
					}
					break;
				}
				else if (ch == '=' || (quoteOrComma == ',' && (ch == '\'' || ch == '"')))
				{
					return ParseAssemblyResult.GenericError;
				}
				sb.Append(ch);
			}
			simpleName = sb.ToString().Trim();
			if (simpleName.Length == 0)
			{
				return ParseAssemblyResult.GenericError;
			}
			if (pos == fullName.Length && fullName[fullName.Length - 1] == ',')
			{
				return ParseAssemblyResult.GenericError;
			}
			return ParseAssemblyResult.OK;
		}

		internal static ParseAssemblyResult ParseAssemblyName(string fullName, out ParsedAssemblyName parsedName)
		{
			parsedName = new ParsedAssemblyName();
			int pos;
			ParseAssemblyResult res = ParseAssemblySimpleName(fullName, out pos, out parsedName.Name);
			if (res != ParseAssemblyResult.OK || pos == fullName.Length)
			{
				return res;
			}
			else
			{
				System.Collections.Generic.Dictionary<string, string> unknownAttributes = null;
				bool hasProcessorArchitecture = false;
				bool hasContentType = false;
				string[] parts = fullName.Substring(pos).Split(',');
				for (int i = 0; i < parts.Length; i++)
				{
					string[] kv = parts[i].Split('=');
					if (kv.Length != 2)
					{
						return ParseAssemblyResult.GenericError;
					}
					switch (kv[0].Trim().ToLowerInvariant())
					{
						case "version":
							if (parsedName.Version != null)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							if (!ParseVersion(kv[1].Trim(), out parsedName.Version))
							{
								return ParseAssemblyResult.GenericError;
							}
							break;
						case "culture":
							if (parsedName.Culture != null)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							if (!ParseCulture(kv[1].Trim(), out parsedName.Culture))
							{
								return ParseAssemblyResult.GenericError;
							}
							break;
						case "publickeytoken":
							if (parsedName.PublicKeyToken != null)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							if (!ParsePublicKeyToken(kv[1].Trim(), out parsedName.PublicKeyToken))
							{
								return ParseAssemblyResult.GenericError;
							}
							break;
						case "publickey":
							if (parsedName.PublicKeyToken != null)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							if (!ParsePublicKey(kv[1].Trim(), out parsedName.PublicKeyToken))
							{
								return ParseAssemblyResult.GenericError;
							}
							parsedName.HasPublicKey = true;
							break;
						case "retargetable":
							if (parsedName.Retargetable.HasValue)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							switch (kv[1].Trim().ToLowerInvariant())
							{
								case "yes":
									parsedName.Retargetable = true;
									break;
								case "no":
									parsedName.Retargetable = false;
									break;
								default:
									return ParseAssemblyResult.GenericError;
							}
							break;
						case "processorarchitecture":
							if (hasProcessorArchitecture)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							hasProcessorArchitecture = true;
							switch (kv[1].Trim().ToLowerInvariant())
							{
								case "none":
									parsedName.ProcessorArchitecture = ProcessorArchitecture.None;
									break;
								case "msil":
									parsedName.ProcessorArchitecture = ProcessorArchitecture.MSIL;
									break;
								case "x86":
									parsedName.ProcessorArchitecture = ProcessorArchitecture.X86;
									break;
								case "ia64":
									parsedName.ProcessorArchitecture = ProcessorArchitecture.IA64;
									break;
								case "amd64":
									parsedName.ProcessorArchitecture = ProcessorArchitecture.Amd64;
									break;
								case "arm":
									parsedName.ProcessorArchitecture = ProcessorArchitecture.Arm;
									break;
								default:
									return ParseAssemblyResult.GenericError;
							}
							break;
						case "contenttype":
							if (hasContentType)
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							hasContentType = true;
							if (kv[1].Trim().ToLowerInvariant() != "windowsruntime")
							{
								return ParseAssemblyResult.GenericError;
							}
							parsedName.WindowsRuntime = true;
							break;
						default:
							if (kv[1].Trim() == "")
							{
								return ParseAssemblyResult.GenericError;
							}
							if (unknownAttributes == null)
							{
								unknownAttributes = new System.Collections.Generic.Dictionary<string, string>();
							}
							if (unknownAttributes.ContainsKey(kv[0].Trim().ToLowerInvariant()))
							{
								return ParseAssemblyResult.DuplicateKey;
							}
							unknownAttributes.Add(kv[0].Trim().ToLowerInvariant(), null);
							break;
					}
				}
			}
			return ParseAssemblyResult.OK;
		}

		private static bool ParseVersion(string str, out Version version)
		{
			string[] parts = str.Split('.');
			if (parts.Length < 2 || parts.Length > 4)
			{
				version = null;
				ushort dummy;
				// if the version consists of a single integer, it is invalid, but not invalid enough to fail the parse of the whole assembly name
				return parts.Length == 1 && ushort.TryParse(parts[0], System.Globalization.NumberStyles.Integer, null, out dummy);
			}
			if (parts[0] == "" || parts[1] == "")
			{
				// this is a strange scenario, the version is invalid, but not invalid enough to fail the parse of the whole assembly name
				version = null;
				return true;
			}
			ushort major, minor, build = 65535, revision = 65535;
			if (ushort.TryParse(parts[0], System.Globalization.NumberStyles.Integer, null, out major)
				&& ushort.TryParse(parts[1], System.Globalization.NumberStyles.Integer, null, out minor)
				&& (parts.Length <= 2 || parts[2] == "" || ushort.TryParse(parts[2], System.Globalization.NumberStyles.Integer, null, out build))
				&& (parts.Length <= 3 || parts[3] == "" || (parts[2] != "" && ushort.TryParse(parts[3], System.Globalization.NumberStyles.Integer, null, out revision))))
			{
				if (parts.Length == 4 && parts[3] != "" && parts[2] != "")
				{
					version = new Version(major, minor, build, revision);
				}
				else if (parts.Length == 3 && parts[2] != "")
				{
					version = new Version(major, minor, build);
				}
				else
				{
					version = new Version(major, minor);
				}
				return true;
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
			publicKeyToken = AssemblyName.ComputePublicKeyToken(str);
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
	}
}
