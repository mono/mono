/*
  Copyright (C) 2009-2011 Jeroen Frijters

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
using System.Globalization;
using System.Configuration.Assemblies;
using System.IO;
using System.Text;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class AssemblyName : ICloneable
	{
		private string name;
		private string culture;
		private Version version;
		private byte[] publicKeyToken;
		private byte[] publicKey;
		private StrongNameKeyPair keyPair;
		private AssemblyNameFlags flags;
		private AssemblyHashAlgorithm hashAlgorithm;
		private AssemblyVersionCompatibility versionCompatibility = AssemblyVersionCompatibility.SameMachine;
		private string codeBase;
		internal byte[] hash;

		public AssemblyName()
		{
		}

		public AssemblyName(string assemblyName)
		{
			if (assemblyName == null)
			{
				throw new ArgumentNullException("assemblyName");
			}
			if (assemblyName == "")
			{
				throw new ArgumentException();
			}
			ParsedAssemblyName parsed;
			switch (Fusion.ParseAssemblyName(assemblyName, out parsed))
			{
				case ParseAssemblyResult.GenericError:
					throw new FileLoadException();
				case ParseAssemblyResult.DuplicateKey:
					throw new System.Runtime.InteropServices.COMException();
			}
			name = parsed.Name;
			if (parsed.Culture != null)
			{
				if (parsed.Culture.Equals("neutral", StringComparison.InvariantCultureIgnoreCase))
				{
					culture = "";
				}
				else if (parsed.Culture == "")
				{
					throw new FileLoadException();
				}
				else
				{
					culture = new CultureInfo(parsed.Culture).Name;
				}
			}
			if (parsed.Version != null && parsed.Version.Major != 65535 && parsed.Version.Minor != 65535)
			{
				// our Fusion parser returns -1 for build and revision for incomplete version numbers (and we want 65535)
				version = new Version(parsed.Version.Major, parsed.Version.Minor, parsed.Version.Build & 0xFFFF, parsed.Version.Revision & 0xFFFF);
			}
			if (parsed.PublicKeyToken != null)
			{
				if (parsed.PublicKeyToken.Equals("null", StringComparison.InvariantCultureIgnoreCase))
				{
					publicKeyToken = Empty<byte>.Array;
				}
				else if (parsed.PublicKeyToken.Length != 16)
				{
					throw new FileLoadException();
				}
				else
				{
					publicKeyToken = new byte[8];
					for (int i = 0, pos = 0; i < publicKeyToken.Length; i++, pos += 2)
					{
						publicKeyToken[i] = (byte)("0123456789abcdef".IndexOf(char.ToLowerInvariant(parsed.PublicKeyToken[pos])) * 16
							+ "0123456789abcdef".IndexOf(char.ToLowerInvariant(parsed.PublicKeyToken[pos + 1])));
					}
				}
			}
			if (parsed.Retargetable.HasValue)
			{
				if (parsed.Culture == null || parsed.PublicKeyToken == null || parsed.Version == null || parsed.Version.Build == -1 || parsed.Version.Revision == -1)
				{
					throw new FileLoadException();
				}
				if (parsed.Retargetable.Value)
				{
					flags |= AssemblyNameFlags.Retargetable;
				}
			}
			ProcessorArchitecture = parsed.ProcessorArchitecture;
		}

		public override string ToString()
		{
			return FullName;
		}

		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public CultureInfo CultureInfo
		{
			get { return culture == null ? null : new CultureInfo(culture); }
			set { culture = value == null ? null : value.Name; }
		}

		internal string Culture
		{
			get { return culture; }
			set { culture = value; }
		}

		public Version Version
		{
			get { return version; }
			set { version = value; }
		}

		public StrongNameKeyPair KeyPair
		{
			get { return keyPair; }
			set { keyPair = value; }
		}

		public string CodeBase
		{
			get { return codeBase; }
			set { codeBase = value; }
		}

		public string EscapedCodeBase
		{
			get
			{
				// HACK use the real AssemblyName to escape the codebase
				System.Reflection.AssemblyName tmp = new System.Reflection.AssemblyName();
				tmp.CodeBase = codeBase;
				return tmp.EscapedCodeBase;
			}
		}

		public ProcessorArchitecture ProcessorArchitecture
		{
			get { return (ProcessorArchitecture)(((int)flags & 0x70) >> 4); }
			set
			{
				if (value >= ProcessorArchitecture.None && value <= ProcessorArchitecture.Arm)
				{
					flags = (flags & ~(AssemblyNameFlags)0x70) | (AssemblyNameFlags)((int)value << 4);
				}
			}
		}

		public AssemblyNameFlags Flags
		{
			get { return flags & (AssemblyNameFlags)~0xF0; }
			set { flags = (flags & (AssemblyNameFlags)0xF0) | (value & (AssemblyNameFlags)~0xF0); }
		}

		public AssemblyVersionCompatibility VersionCompatibility
		{
			get { return versionCompatibility; }
			set { versionCompatibility = value; }
		}

		public byte[] GetPublicKey()
		{
			return publicKey;
		}

		public void SetPublicKey(byte[] publicKey)
		{
			this.publicKey = publicKey;
			flags = (flags & ~AssemblyNameFlags.PublicKey) | (publicKey == null ? 0 : AssemblyNameFlags.PublicKey);
		}

		public byte[] GetPublicKeyToken()
		{
			if (publicKeyToken == null && publicKey != null)
			{
				// note that GetPublicKeyToken() has a side effect in this case, because we retain this token even after the public key subsequently gets changed
				publicKeyToken = ComputePublicKeyToken(publicKey);
			}
			return publicKeyToken;
		}

		public void SetPublicKeyToken(byte[] publicKeyToken)
		{
			this.publicKeyToken = publicKeyToken;
		}

		public AssemblyHashAlgorithm HashAlgorithm
		{
			get { return hashAlgorithm; }
			set { hashAlgorithm = value; }
		}

		public string FullName
		{
			get
			{
				if (name == null)
				{
					return "";
				}
				StringBuilder sb = new StringBuilder();
				bool doubleQuotes = name.StartsWith(" ") || name.EndsWith(" ") || name.IndexOf('\'') != -1;
				bool singleQuotes = name.IndexOf('"') != -1;
				if (singleQuotes)
				{
					sb.Append('\'');
				}
				else if (doubleQuotes)
				{
					sb.Append('"');
				}
				if (name.IndexOf(',') != -1 || name.IndexOf('\\') != -1 || name.IndexOf('=') != -1 || (singleQuotes && name.IndexOf('\'') != -1))
				{
					for (int i = 0; i < name.Length; i++)
					{
						char c = name[i];
						if (c == ',' || c == '\\' || c == '=' || (singleQuotes && c == '\''))
						{
							sb.Append('\\');
						}
						sb.Append(c);
					}
				}
				else
				{
					sb.Append(name);
				}
				if (singleQuotes)
				{
					sb.Append('\'');
				}
				else if (doubleQuotes)
				{
					sb.Append('"');
				}
				if (version != null)
				{
					if ((version.Major & 0xFFFF) != 0xFFFF)
					{
						sb.AppendFormat(", Version={0}", version.Major & 0xFFFF);
						if ((version.Minor & 0xFFFF) != 0xFFFF)
						{
							sb.AppendFormat(".{0}", version.Minor & 0xFFFF);
							if ((version.Build & 0xFFFF) != 0xFFFF)
							{
								sb.AppendFormat(".{0}", version.Build & 0xFFFF);
								if ((version.Revision & 0xFFFF) != 0xFFFF)
								{
									sb.AppendFormat(".{0}", version.Revision & 0xFFFF);
								}
							}
						}
					}
				}
				if (culture != null)
				{
					sb.Append(", Culture=").Append(culture == "" ? "neutral" : culture);
				}
				byte[] publicKeyToken = this.publicKeyToken;
				if ((publicKeyToken == null || publicKeyToken.Length == 0) && publicKey != null)
				{
					publicKeyToken = ComputePublicKeyToken(publicKey);
				}
				if (publicKeyToken != null)
				{
					sb.Append(", PublicKeyToken=");
					if (publicKeyToken.Length == 0)
					{
						sb.Append("null");
					}
					else
					{
						for (int i = 0; i < publicKeyToken.Length; i++)
						{
							sb.AppendFormat("{0:x2}", publicKeyToken[i]);
						}
					}
				}
				if ((Flags & AssemblyNameFlags.Retargetable) != 0)
				{
					sb.Append(", Retargetable=Yes");
				}
				return sb.ToString();
			}
		}

		private static byte[] ComputePublicKeyToken(byte[] publicKey)
		{
			if (publicKey.Length == 0)
			{
				return publicKey;
			}
			// HACK use the real AssemblyName to convert PublicKey to PublicKeyToken
			StringBuilder sb = new StringBuilder("Foo, PublicKey=", 20 + publicKey.Length * 2);
			for (int i = 0; i < publicKey.Length; i++)
			{
				sb.AppendFormat("{0:x2}", publicKey[i]);
			}
			return new System.Reflection.AssemblyName(sb.ToString()).GetPublicKeyToken();
		}

		public override bool Equals(object obj)
		{
			AssemblyName other = obj as AssemblyName;
			return other != null && other.FullName == this.FullName;
		}

		public override int GetHashCode()
		{
			return FullName.GetHashCode();
		}

		public object Clone()
		{
			AssemblyName copy = (AssemblyName)MemberwiseClone();
			copy.publicKey = Copy(publicKey);
			copy.publicKeyToken = Copy(publicKeyToken);
			return copy;
		}

		private static byte[] Copy(byte[] b)
		{
			return b == null || b.Length == 0 ? b : (byte[])b.Clone();
		}

		public static bool ReferenceMatchesDefinition(AssemblyName reference, AssemblyName definition)
		{
			// HACK use the real AssemblyName to implement the (broken) ReferenceMatchesDefinition method
			return System.Reflection.AssemblyName.ReferenceMatchesDefinition(new System.Reflection.AssemblyName(reference.FullName), new System.Reflection.AssemblyName(definition.FullName));
		}

		public static AssemblyName GetAssemblyName(string path)
		{
			try
			{
				path = Path.GetFullPath(path);
				using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					ModuleReader module = new ModuleReader(null, null, fs, path);
					if (module.Assembly == null)
					{
						throw new BadImageFormatException("Module does not contain a manifest");
					}
					return module.Assembly.GetName();
				}
			}
			catch (IOException x)
			{
				throw new FileNotFoundException(x.Message, x);
			}
			catch (UnauthorizedAccessException x)
			{
				throw new FileNotFoundException(x.Message, x);
			}
		}

		internal AssemblyNameFlags RawFlags
		{
			get { return flags; }
			set { flags = value; }
		}
	}
}
