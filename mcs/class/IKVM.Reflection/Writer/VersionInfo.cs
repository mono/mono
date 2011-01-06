/*
  Copyright (C) 2008 Jeroen Frijters

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
using IKVM.Reflection.Emit;

namespace IKVM.Reflection.Writer
{
	sealed class VersionInfo
	{
		private AssemblyName name;
		private string fileName;
		internal string copyright;
		internal string trademark;
		internal string product;
		internal string company;
		private string description;
		private string title;
		internal string informationalVersion;
		private string culture;
		private string fileVersion;

		internal void SetName(AssemblyName name)
		{
			this.name = name;
		}

		internal void SetFileName(string assemblyFileName)
		{
			this.fileName = assemblyFileName;
		}

		internal void SetAttribute(CustomAttributeBuilder cab)
		{
			Universe u = cab.Constructor.Module.universe;
			Type type = cab.Constructor.DeclaringType;
			if (copyright == null && type == u.System_Reflection_AssemblyCopyrightAttribute)
			{
				copyright = (string)cab.GetConstructorArgument(0);
			}
			else if (trademark == null && type == u.System_Reflection_AssemblyTrademarkAttribute)
			{
				trademark = (string)cab.GetConstructorArgument(0);
			}
			else if (product == null && type == u.System_Reflection_AssemblyProductAttribute)
			{
				product = (string)cab.GetConstructorArgument(0);
			}
			else if (company == null && type == u.System_Reflection_AssemblyCompanyAttribute)
			{
				company = (string)cab.GetConstructorArgument(0);
			}
			else if (description == null && type == u.System_Reflection_AssemblyDescriptionAttribute)
			{
				description = (string)cab.GetConstructorArgument(0);
			}
			else if (title == null && type == u.System_Reflection_AssemblyTitleAttribute)
			{
				title = (string)cab.GetConstructorArgument(0);
			}
			else if (informationalVersion == null && type == u.System_Reflection_AssemblyInformationalVersionAttribute)
			{
				informationalVersion = (string)cab.GetConstructorArgument(0);
			}
			else if (culture == null && type == u.System_Reflection_AssemblyCultureAttribute)
			{
				culture  = (string)cab.GetConstructorArgument(0);
			}
			else if (fileVersion == null && type == u.System_Reflection_AssemblyFileVersionAttribute)
			{
				fileVersion = (string)cab.GetConstructorArgument(0);
			}
		}

		internal void Write(ByteBuffer bb)
		{
			if (fileVersion == null)
			{
				if (name.Version != null)
				{
					fileVersion = name.Version.ToString();
				}
				else
				{
					fileVersion = "0.0.0.0";
				}
			}

			int codepage = 1200;	// Unicode codepage
			int lcid = 0x7f;
			if (name.CultureInfo != null)
			{
				lcid = name.CultureInfo.LCID;
			}
			if (culture != null)
			{
				lcid = new CultureInfo(culture).LCID;
			}

			Version filever = ParseVersionRobust(fileVersion);
			int fileVersionMajor = filever.Major;
			int fileVersionMinor = filever.Minor;
			int fileVersionBuild = filever.Build;
			int fileVersionRevision = filever.Revision;

			int productVersionMajor = fileVersionMajor;
			int productVersionMinor = fileVersionMinor;
			int productVersionBuild = fileVersionBuild;
			int productVersionRevision = fileVersionRevision;
			if (informationalVersion != null)
			{
				Version productver = ParseVersionRobust(informationalVersion);
				productVersionMajor = productver.Major;
				productVersionMinor = productver.Minor;
				productVersionBuild = productver.Build;
				productVersionRevision = productver.Revision;
			}

			ByteBuffer stringTable = new ByteBuffer(512);
			stringTable.Write((short)0);	// wLength (placeholder)
			stringTable.Write((short)0);	// wValueLength
			stringTable.Write((short)1);	// wType
			WriteUTF16Z(stringTable, string.Format("{0:x4}{1:x4}", lcid, codepage));
			stringTable.Align(4);

			WriteString(stringTable, "Comments", description);
			WriteString(stringTable, "CompanyName", company);
			WriteString(stringTable, "FileDescription", title);
			WriteString(stringTable, "FileVersion", fileVersion);
			WriteString(stringTable, "InternalName", name.Name);
			WriteString(stringTable, "LegalCopyright", copyright);
			WriteString(stringTable, "LegalTrademarks", trademark);
			WriteString(stringTable, "OriginalFilename", fileName);
			WriteString(stringTable, "ProductName", product);
			WriteString(stringTable, "ProductVersion", informationalVersion);

			stringTable.Position = 0;
			stringTable.Write((short)stringTable.Length);

			ByteBuffer stringFileInfo = new ByteBuffer(512);
			stringFileInfo.Write((short)0);	// wLength (placeholder)
			stringFileInfo.Write((short)0);	// wValueLength
			stringFileInfo.Write((short)1);	// wType
			WriteUTF16Z(stringFileInfo, "StringFileInfo");
			stringFileInfo.Align(4);
			stringFileInfo.Write(stringTable);
			stringFileInfo.Position = 0;
			stringFileInfo.Write((short)stringFileInfo.Length);

			byte[] preamble1 = new byte[] {
			  // VS_VERSIONINFO (platform SDK)
			  0x34, 0x00,				// wValueLength
			  0x00, 0x00,				// wType
			  0x56, 0x00, 0x53, 0x00, 0x5F, 0x00, 0x56, 0x00, 0x45, 0x00, 0x52, 0x00, 0x53, 0x00, 0x49, 0x00, 0x4F, 0x00, 0x4E, 0x00, 0x5F, 0x00, 0x49, 0x00, 0x4E, 0x00, 0x46, 0x00, 0x4F, 0x00, 0x00, 0x00,  // "VS_VERSION_INFO\0"
			  0x00, 0x00,				// Padding1 (32 bit alignment)
			  // VS_FIXEDFILEINFO starts
			  0xBD, 0x04, 0xEF, 0xFE,	// dwSignature (0xFEEF04BD)
			  0x00, 0x00, 0x01, 0x00,	// dwStrucVersion
			};
			byte[] preamble2 = new byte[] {
			  0x3F, 0x00, 0x00, 0x00,	// dwFileFlagsMask (??)
			  0x00, 0x00, 0x00, 0x00,	// dwFileFlags (??)
			  0x04, 0x00, 0x00, 0x00,	// dwFileOS
			  0x02, 0x00, 0x00, 0x00,	// dwFileType
			  0x00, 0x00, 0x00, 0x00,	// dwFileSubtype
			  0x00, 0x00, 0x00, 0x00,	// dwFileDateMS
			  0x00, 0x00, 0x00, 0x00,	// dwFileDateLS
										// Padding2 (32 bit alignment)
			  // VarFileInfo
			  0x44, 0x00,				// wLength
			  0x00, 0x00,				// wValueLength
			  0x01, 0x00,				// wType
			  0x56, 0x00, 0x61, 0x00, 0x72, 0x00, 0x46, 0x00, 0x69, 0x00, 0x6C, 0x00, 0x65, 0x00, 0x49, 0x00, 0x6E, 0x00, 0x66, 0x00, 0x6F, 0x00, 0x00, 0x00,	// "VarFileInfo\0"
			  0x00, 0x00,				// Padding
			  // Var
			  0x24, 0x00,				// wLength
			  0x04, 0x00,				// wValueLength
			  0x00, 0x00,				// wType
			  0x54, 0x00, 0x72, 0x00, 0x61, 0x00, 0x6E, 0x00, 0x73, 0x00, 0x6C, 0x00, 0x61, 0x00, 0x74, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x6E, 0x00, 0x00, 0x00,	// "Translation\0"
			  0x00, 0x00,				// Padding (32 bit alignment)
			};
			bb.Write((short)(2 + preamble1.Length + 8 + 8 + preamble2.Length + 4 + stringFileInfo.Length));
			bb.Write(preamble1);
			bb.Write((short)fileVersionMinor);
			bb.Write((short)fileVersionMajor);
			bb.Write((short)fileVersionRevision);
			bb.Write((short)fileVersionBuild);
			bb.Write((short)productVersionMinor);
			bb.Write((short)productVersionMajor);
			bb.Write((short)productVersionRevision);
			bb.Write((short)productVersionBuild);
			bb.Write(preamble2);
			bb.Write((short)lcid);
			bb.Write((short)codepage);
			bb.Write(stringFileInfo);
		}

		private static void WriteUTF16Z(ByteBuffer bb, string str)
		{
			foreach (char c in str)
			{
				bb.Write((short)c);
			}
			bb.Write((short)0);
		}

		private static void WriteString(ByteBuffer bb, string name, string value)
		{
			value = value ?? " ";
			int pos = bb.Position;
			bb.Write((short)0);					// wLength (placeholder)
			bb.Write((short)(value.Length + 1));// wValueLength
			bb.Write((short)1);					// wType
			WriteUTF16Z(bb, name);
			bb.Align(4);
			WriteUTF16Z(bb, value);
			bb.Align(4);
			int savedPos = bb.Position;
			bb.Position = pos;
			bb.Write((short)(savedPos - pos));
			bb.Position = savedPos;
		}

		private static Version ParseVersionRobust(string ver)
		{
			int index = 0;
			ushort major = ParseVersionPart(ver, ref index);
			ushort minor = ParseVersionPart(ver, ref index);
			ushort build = ParseVersionPart(ver, ref index);
			ushort revision = ParseVersionPart(ver, ref index);
			return new Version(major, minor, build, revision);
		}

		private static ushort ParseVersionPart(string str, ref int pos)
		{
			ushort value = 0;
			while (pos < str.Length)
			{
				char c = str[pos];
				if (c == '.')
				{
					pos++;
					break;
				}
				else if (c >= '0' && c <= '9')
				{
					value *= 10;
					value += (ushort)(c - '0');
					pos++;
				}
				else
				{
					break;
				}
			}
			return value;
		}
	}
}
