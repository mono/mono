/*
  Copyright (C) 2009 Jeroen Frijters

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
using IKVM.Reflection.Reader;

namespace IKVM.Reflection
{
	public sealed class AssemblyName : ICloneable
	{
		private readonly System.Reflection.AssemblyName name;
		private string culture;

		private AssemblyName(System.Reflection.AssemblyName name, string culture)
		{
			this.name = name;
			this.culture = culture;
		}

		public AssemblyName()
		{
			name = new System.Reflection.AssemblyName();
		}

		public AssemblyName(string assemblyName)
		{
			name = new System.Reflection.AssemblyName(assemblyName);
		}

		public override string ToString()
		{
			string str = name.ToString();
			if (culture != null)
			{
				str = str.Replace("Culture=neutral", "Culture=" + culture);
			}
			return str;
		}

		public string Name
		{
			get { return name.Name; }
			set { name.Name = value; }
		}

		public CultureInfo CultureInfo
		{
			get { return name.CultureInfo; }
			set
			{
				name.CultureInfo = value;
				culture = null;
			}
		}

		internal string Culture
		{
			set
			{
				culture = value;
				name.CultureInfo = CultureInfo.InvariantCulture;
			}
		}

		public Version Version
		{
			get { return name.Version; }
			set { name.Version = value; }
		}

		public StrongNameKeyPair KeyPair
		{
			get { return name.KeyPair == null ?  null : new StrongNameKeyPair(name.KeyPair); }
			set { name.KeyPair = value == null ? null : value.keyPair; }
		}

		public string CodeBase
		{
			get { return name.CodeBase; }
			set { name.CodeBase = value; }
		}

		public ProcessorArchitecture ProcessorArchitecture
		{
			get { return (ProcessorArchitecture)name.ProcessorArchitecture; }
			set { name.ProcessorArchitecture = (System.Reflection.ProcessorArchitecture)value; }
		}

		public AssemblyNameFlags Flags
		{
			get { return (AssemblyNameFlags)name.Flags; }
			set { name.Flags = (System.Reflection.AssemblyNameFlags)value; }
		}

		public AssemblyVersionCompatibility VersionCompatibility
		{
			get { return name.VersionCompatibility; }
			set { name.VersionCompatibility = value; }
		}

		public byte[] GetPublicKey()
		{
			return name.GetPublicKey();
		}

		public void SetPublicKey(byte[] publicKey)
		{
			name.SetPublicKey(publicKey);
		}

		public byte[] GetPublicKeyToken()
		{
			return name.GetPublicKeyToken();
		}

		public void SetPublicKeyToken(byte[] publicKeyToken)
		{
			name.SetPublicKeyToken(publicKeyToken);
		}

		public AssemblyHashAlgorithm HashAlgorithm
		{
			get { return name.HashAlgorithm; }
			set { name.HashAlgorithm = value; }
		}

		public string FullName
		{
			get
			{
				string str = name.FullName;
				if (culture != null)
				{
					str = str.Replace("Culture=neutral", "Culture=" + culture);
				}
				return str;
			}
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
			return new AssemblyName((System.Reflection.AssemblyName)name.Clone(), culture);
		}

		public static bool ReferenceMatchesDefinition(AssemblyName reference, AssemblyName definition)
		{
			return System.Reflection.AssemblyName.ReferenceMatchesDefinition(reference.name, definition.name);
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
	}
}
