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
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Impl;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	sealed class TextSection
	{
		private readonly PEWriter peWriter;
		private readonly CliHeader cliHeader;
		private readonly ModuleBuilder moduleBuilder;
		private readonly uint strongNameSignatureLength;

		internal TextSection(PEWriter peWriter, CliHeader cliHeader, ModuleBuilder moduleBuilder, int strongNameSignatureLength)
		{
			this.peWriter = peWriter;
			this.cliHeader = cliHeader;
			this.moduleBuilder = moduleBuilder;
			this.strongNameSignatureLength = (uint)strongNameSignatureLength;
		}

		internal uint PointerToRawData
		{
			get { return peWriter.ToFileAlignment(peWriter.HeaderSize); }
		}

		internal uint BaseRVA
		{
			get { return 0x2000; }
		}

		internal uint ImportAddressTableRVA
		{
			get { return BaseRVA; }
		}

		internal uint ImportAddressTableLength
		{
			get
			{
				if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_I386)
				{
					return 8;
				}
				else
				{
					return 16;
				}
			}
		}

		internal uint ComDescriptorRVA
		{
			get { return ImportAddressTableRVA + ImportAddressTableLength; }
		}

		internal uint ComDescriptorLength
		{
			get { return cliHeader.Cb; }
		}

		internal uint MethodBodiesRVA
		{
			get { return (ComDescriptorRVA + ComDescriptorLength + 7) & ~7U; }
		}

		private uint MethodBodiesLength
		{
			get { return (uint)moduleBuilder.methodBodies.Length; }
		}

		private uint ResourcesRVA
		{
			get
			{
				if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_I386)
				{
					return (MethodBodiesRVA + MethodBodiesLength + 3) & ~3U;
				}
				else
				{
					return (MethodBodiesRVA + MethodBodiesLength + 15) & ~15U;
				}
			}
		}

		private uint ResourcesLength
		{
			get { return (uint)moduleBuilder.manifestResources.Length; }
		}

		internal uint StrongNameSignatureRVA
		{
			get
			{
				return (ResourcesRVA + ResourcesLength + 3) & ~3U;
			}
		}

		internal uint StrongNameSignatureLength
		{
			get
			{
				return strongNameSignatureLength;
			}
		}

		private uint MetadataRVA
		{
			get
			{
				return (StrongNameSignatureRVA + StrongNameSignatureLength + 3) & ~3U;
			}
		}

		private uint MetadataLength
		{
			get { return (uint)moduleBuilder.MetadataLength; }
		}

		internal uint DebugDirectoryRVA
		{
			get { return MetadataRVA + MetadataLength; }
		}

		internal uint DebugDirectoryLength
		{
			get
			{
				if (DebugDirectoryContentsLength != 0)
				{
					return 28;
				}
				return 0;
			}
		}

		private uint DebugDirectoryContentsLength
		{
			get
			{
				if (moduleBuilder.symbolWriter != null)
				{
					IMAGE_DEBUG_DIRECTORY idd = new IMAGE_DEBUG_DIRECTORY();
					return (uint)SymbolSupport.GetDebugInfo(moduleBuilder.symbolWriter, ref idd).Length;
				}
				return 0;
			}
		}

		internal uint ImportDirectoryRVA
		{
			// on AMD64 (and probably IA64) the import directory needs to be 16 byte aligned (on I386 4 byte alignment is sufficient)
			get { return (DebugDirectoryRVA + DebugDirectoryLength + DebugDirectoryContentsLength + 15) & ~15U; }
		}

		internal uint ImportDirectoryLength
		{
			get { return (ImportHintNameTableRVA - ImportDirectoryRVA) + 27; }
		}

		private uint ImportHintNameTableRVA
		{
			get
			{
				if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_I386)
				{
					return (ImportDirectoryRVA + 48 + 15) & ~15U;
				}
				else
				{
					return (ImportDirectoryRVA + 48 + 4 + 15) & ~15U;
				}
			}
		}

		internal uint StartupStubRVA
		{
			get
			{
				if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_IA64)
				{
					// note that the alignment is driven by the requirement that the two relocation fixups are in a single page
					return (ImportDirectoryRVA + ImportDirectoryLength + 15U) & ~15U;
				}
				else
				{
					// the additional 2 bytes padding are to align the address in the jump (which is a relocation fixup)
					return 2 + ((ImportDirectoryRVA + ImportDirectoryLength + 3U) & ~3U);
				}
			}
		}

		internal uint StartupStubLength
		{
			get
			{
				if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_AMD64)
				{
					return 12;
				}
				else if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_IA64)
				{
					return 48;
				}
				else
				{
					return 6;
				}
			}
		}

		private void WriteRVA(MetadataWriter mw, uint rva)
		{
			if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_I386)
			{
				mw.Write(rva);
			}
			else
			{
				mw.Write((ulong)rva);
			}
		}

		internal void Write(MetadataWriter mw, int sdataRVA)
		{
			// Now that we're ready to start writing, we need to do some fix ups
			moduleBuilder.MethodDef.Fixup(this);
			moduleBuilder.MethodImpl.Fixup(moduleBuilder);
			moduleBuilder.MethodSemantics.Fixup(moduleBuilder);
			moduleBuilder.InterfaceImpl.Fixup();
			moduleBuilder.MemberRef.Fixup(moduleBuilder);
			moduleBuilder.Constant.Fixup(moduleBuilder);
			moduleBuilder.FieldMarshal.Fixup(moduleBuilder);
			moduleBuilder.DeclSecurity.Fixup(moduleBuilder);
			moduleBuilder.GenericParam.Fixup(moduleBuilder);
			moduleBuilder.CustomAttribute.Fixup(moduleBuilder);
			moduleBuilder.FieldLayout.Fixup(moduleBuilder);
			moduleBuilder.FieldRVA.Fixup(moduleBuilder, sdataRVA);
			moduleBuilder.ImplMap.Fixup(moduleBuilder);
			moduleBuilder.MethodSpec.Fixup(moduleBuilder);
			moduleBuilder.GenericParamConstraint.Fixup(moduleBuilder);

			// Import Address Table
			AssertRVA(mw, ImportAddressTableRVA);
			WriteRVA(mw, ImportHintNameTableRVA);
			WriteRVA(mw, 0);

			// CLI Header
			AssertRVA(mw, ComDescriptorRVA);
			cliHeader.MetaDataRVA = MetadataRVA;
			cliHeader.MetaDataSize = MetadataLength;
			if (ResourcesLength != 0)
			{
				cliHeader.ResourcesRVA = ResourcesRVA;
				cliHeader.ResourcesSize = ResourcesLength;
			}
			if (StrongNameSignatureLength != 0)
			{
				cliHeader.StrongNameSignatureRVA = StrongNameSignatureRVA;
				cliHeader.StrongNameSignatureSize = StrongNameSignatureLength;
			}
			cliHeader.Write(mw);

			// alignment padding
			for (int i = (int)(MethodBodiesRVA - (ComDescriptorRVA + ComDescriptorLength)); i > 0; i--)
			{
				mw.Write((byte)0);
			}

			// Method Bodies
			mw.Write(moduleBuilder.methodBodies);

			// alignment padding
			for (int i = (int)(ResourcesRVA - (MethodBodiesRVA + MethodBodiesLength)); i > 0; i--)
			{
				mw.Write((byte)0);
			}

			// Resources
			mw.Write(moduleBuilder.manifestResources);

			// The strong name signature live here (if it exists), but it will written later
			// and the following alignment padding will take care of reserving the space.

			// alignment padding
			for (int i = (int)(MetadataRVA - (ResourcesRVA + ResourcesLength)); i > 0; i--)
			{
				mw.Write((byte)0);
			}

			// Metadata
			AssertRVA(mw, MetadataRVA);
			moduleBuilder.WriteMetadata(mw);

			// Debug Directory
			AssertRVA(mw, DebugDirectoryRVA);
			WriteDebugDirectory(mw);

			// alignment padding
			for (int i = (int)(ImportDirectoryRVA - (DebugDirectoryRVA + DebugDirectoryLength + DebugDirectoryContentsLength)); i > 0; i--)
			{
				mw.Write((byte)0);
			}

			// Import Directory
			AssertRVA(mw, ImportDirectoryRVA);
			WriteImportDirectory(mw);

			// alignment padding
			for (int i = (int)(StartupStubRVA - (ImportDirectoryRVA + ImportDirectoryLength)); i > 0; i--)
			{
				mw.Write((byte)0);
			}

			// Startup Stub
			AssertRVA(mw, StartupStubRVA);
			if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_AMD64)
			{
				/*
				 *   48 A1 00 20 40 00 00 00 00 00        mov         rax,qword ptr [0000000000402000h]
				 *   FF E0                                jmp         rax
				 */
				mw.Write((ushort)0xA148);
				mw.Write(peWriter.Headers.OptionalHeader.ImageBase + ImportAddressTableRVA);
				mw.Write((ushort)0xE0FF);
			}
			else if (peWriter.Headers.FileHeader.Machine == IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_IA64)
			{
				mw.Write(new byte[] {
						0x0B, 0x48, 0x00, 0x02, 0x18, 0x10, 0xA0, 0x40, 0x24, 0x30, 0x28, 0x00, 0x00, 0x00, 0x04, 0x00,
						0x10, 0x08, 0x00, 0x12, 0x18, 0x10, 0x60, 0x50, 0x04, 0x80, 0x03, 0x00, 0x60, 0x00, 0x80, 0x00
					});
				mw.Write(peWriter.Headers.OptionalHeader.ImageBase + StartupStubRVA);
				mw.Write(peWriter.Headers.OptionalHeader.ImageBase + BaseRVA);
			}
			else
			{
				mw.Write((ushort)0x25FF);
				mw.Write((uint)peWriter.Headers.OptionalHeader.ImageBase + ImportAddressTableRVA);
			}
		}

		[Conditional("DEBUG")]
		private void AssertRVA(MetadataWriter mw, uint rva)
		{
			Debug.Assert(mw.Position - PointerToRawData + BaseRVA == rva);
		}

		private void WriteDebugDirectory(MetadataWriter mw)
		{
			if (DebugDirectoryLength != 0)
			{
				IMAGE_DEBUG_DIRECTORY idd = new IMAGE_DEBUG_DIRECTORY();
				idd.Characteristics = 0;
				idd.TimeDateStamp = peWriter.Headers.FileHeader.TimeDateStamp;
				byte[] buf = SymbolSupport.GetDebugInfo(moduleBuilder.symbolWriter, ref idd);
				idd.PointerToRawData = (DebugDirectoryRVA - BaseRVA) + DebugDirectoryLength + PointerToRawData;
				mw.Write(idd.Characteristics);
				mw.Write(idd.TimeDateStamp);
				mw.Write(idd.MajorVersion);
				mw.Write(idd.MinorVersion);
				mw.Write(idd.Type);
				mw.Write(idd.SizeOfData);
				mw.Write(idd.AddressOfRawData);
				mw.Write(idd.PointerToRawData);
				mw.Write(buf);
			}
		}

		private void WriteImportDirectory(MetadataWriter mw)
		{
			mw.Write(ImportDirectoryRVA + 40);		// ImportLookupTable
			mw.Write(0);							// DateTimeStamp
			mw.Write(0);							// ForwarderChain
			mw.Write(ImportHintNameTableRVA + 14);	// Name
			mw.Write(ImportAddressTableRVA);
			mw.Write(new byte[20]);
			// Import Lookup Table
			mw.Write(ImportHintNameTableRVA);		// Hint/Name Table RVA
			int size = 48;
			if (peWriter.Headers.FileHeader.Machine != IMAGE_FILE_HEADER.IMAGE_FILE_MACHINE_I386)
			{
				size += 4;
				mw.Write(0);
			}
			mw.Write(0);

			// alignment padding
			for (int i = (int)(ImportHintNameTableRVA - (ImportDirectoryRVA + size)); i > 0; i--)
			{
				mw.Write((byte)0);
			}

			// Hint/Name Table
			AssertRVA(mw, ImportHintNameTableRVA);
			mw.Write((ushort)0);		// Hint
			if ((peWriter.Headers.FileHeader.Characteristics & IMAGE_FILE_HEADER.IMAGE_FILE_DLL) != 0)
			{
				mw.Write(System.Text.Encoding.ASCII.GetBytes("_CorDllMain"));
			}
			else
			{
				mw.Write(System.Text.Encoding.ASCII.GetBytes("_CorExeMain"));
			}
			mw.Write((byte)0);
			// Name
			mw.Write(System.Text.Encoding.ASCII.GetBytes("mscoree.dll"));
			mw.Write((ushort)0);
		}

		internal int Length
		{
			get { return (int)(StartupStubRVA - BaseRVA + StartupStubLength); }
		}
	}
}
