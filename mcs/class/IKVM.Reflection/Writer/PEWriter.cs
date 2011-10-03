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
using System.IO;
using BYTE = System.Byte;
using WORD = System.UInt16;
using DWORD = System.UInt32;
using ULONGLONG = System.UInt64;
using IMAGE_DATA_DIRECTORY = IKVM.Reflection.Reader.IMAGE_DATA_DIRECTORY;

namespace IKVM.Reflection.Writer
{
	sealed class PEWriter
	{
		private readonly BinaryWriter bw;
		private readonly IMAGE_NT_HEADERS hdr = new IMAGE_NT_HEADERS();

		internal PEWriter(Stream stream)
		{
			bw = new BinaryWriter(stream);
			WriteMSDOSHeader();
		}

		public IMAGE_NT_HEADERS Headers
		{
			get { return hdr; }
		}

		public uint HeaderSize
		{
			get
			{
				return (uint)
					((8 * 16) +	// MSDOS header
					4 +				// signature
					20 +			// IMAGE_FILE_HEADER
					hdr.FileHeader.SizeOfOptionalHeader +
					hdr.FileHeader.NumberOfSections * 40);
			}
		}

		private void WriteMSDOSHeader()
		{
			bw.Write(new byte[] {
				0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00,
				0x04, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x00, 0x00,
				0xB8, 0x00, 0x00, 0x00, 0x00, 0x00,	0x00, 0x00,
				0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x00,
				0x0E, 0x1F, 0xBA, 0x0E, 0x00, 0xB4, 0x09, 0xCD,
				0x21, 0xB8, 0x01, 0x4C, 0xCD, 0x21, 0x54, 0x68,
				0x69, 0x73, 0x20, 0x70, 0x72, 0x6F, 0x67, 0x72,
				0x61, 0x6D, 0x20, 0x63, 0x61, 0x6E, 0x6E, 0x6F,
				0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 0x6E,
				0x20, 0x69, 0x6E, 0x20, 0x44, 0x4F, 0x53, 0x20,
				0x6D, 0x6F, 0x64, 0x65, 0x2E, 0x0D, 0x0D, 0x0A,
				0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
			});
		}

		internal void WritePEHeaders()
		{
			bw.Write(hdr.Signature);

			// IMAGE_FILE_HEADER
			bw.Write(hdr.FileHeader.Machine);
			bw.Write(hdr.FileHeader.NumberOfSections);
			bw.Write(hdr.FileHeader.TimeDateStamp);
			bw.Write(hdr.FileHeader.PointerToSymbolTable);
			bw.Write(hdr.FileHeader.NumberOfSymbols);
			bw.Write(hdr.FileHeader.SizeOfOptionalHeader);
			bw.Write(hdr.FileHeader.Characteristics);

			// IMAGE_OPTIONAL_HEADER
			hdr.OptionalHeader.Write(bw);
		}

		internal void WriteSectionHeader(SectionHeader sectionHeader)
		{
			byte[] name = new byte[8];
			System.Text.Encoding.UTF8.GetBytes(sectionHeader.Name, 0, sectionHeader.Name.Length, name, 0);
			bw.Write(name);
			bw.Write(sectionHeader.VirtualSize);
			bw.Write(sectionHeader.VirtualAddress);
			bw.Write(sectionHeader.SizeOfRawData);
			bw.Write(sectionHeader.PointerToRawData);
			bw.Write(sectionHeader.PointerToRelocations);
			bw.Write(sectionHeader.PointerToLinenumbers);
			bw.Write(sectionHeader.NumberOfRelocations);
			bw.Write(sectionHeader.NumberOfLinenumbers);
			bw.Write(sectionHeader.Characteristics);
		}

		internal uint ToFileAlignment(uint p)
		{
			return (p + (Headers.OptionalHeader.FileAlignment - 1)) & ~(Headers.OptionalHeader.FileAlignment - 1);
		}

		internal uint ToSectionAlignment(uint p)
		{
			return (p + (Headers.OptionalHeader.SectionAlignment - 1)) & ~(Headers.OptionalHeader.SectionAlignment - 1);
		}
	}

	sealed class IMAGE_NT_HEADERS
	{
		public DWORD Signature = 0x00004550;	// "PE\0\0"
		public IMAGE_FILE_HEADER FileHeader = new IMAGE_FILE_HEADER();
		public IMAGE_OPTIONAL_HEADER OptionalHeader = new IMAGE_OPTIONAL_HEADER();
	}

	sealed class IMAGE_FILE_HEADER
	{
		public const WORD IMAGE_FILE_MACHINE_I386 = 0x014c;
		public const WORD IMAGE_FILE_MACHINE_ARM = 0x01c4;
		public const WORD IMAGE_FILE_MACHINE_IA64 = 0x0200;
		public const WORD IMAGE_FILE_MACHINE_AMD64 = 0x8664;

		public const WORD IMAGE_FILE_32BIT_MACHINE = 0x0100;
		public const WORD IMAGE_FILE_EXECUTABLE_IMAGE = 0x0002;
		public const WORD IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x0020;
		public const WORD IMAGE_FILE_DLL = 0x2000;

		public WORD Machine;
		public WORD NumberOfSections;
		public DWORD TimeDateStamp = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
		public DWORD PointerToSymbolTable = 0;
		public DWORD NumberOfSymbols = 0;
		public WORD SizeOfOptionalHeader = 0xE0;
		public WORD Characteristics = IMAGE_FILE_EXECUTABLE_IMAGE;
	}

	sealed class IMAGE_OPTIONAL_HEADER
	{
		public const WORD IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10b;
		public const WORD IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20b;

		public const WORD IMAGE_SUBSYSTEM_WINDOWS_GUI = 2;
		public const WORD IMAGE_SUBSYSTEM_WINDOWS_CUI = 3;

		public const WORD IMAGE_DLLCHARACTERISTICS_DYNAMIC_BASE = 0x0040;
		public const WORD IMAGE_DLLCHARACTERISTICS_NX_COMPAT = 0x0100;
		public const WORD IMAGE_DLLCHARACTERISTICS_NO_SEH = 0x0400;
		public const WORD IMAGE_DLLCHARACTERISTICS_TERMINAL_SERVER_AWARE = 0x8000;

		public WORD Magic = IMAGE_NT_OPTIONAL_HDR32_MAGIC;
		public BYTE MajorLinkerVersion = 8;
		public BYTE MinorLinkerVersion = 0;
		public DWORD SizeOfCode;
		public DWORD SizeOfInitializedData;
		public DWORD SizeOfUninitializedData;
		public DWORD AddressOfEntryPoint;
		public DWORD BaseOfCode;
		public DWORD BaseOfData;
		public ULONGLONG ImageBase;
		public DWORD SectionAlignment = 0x2000;
		public DWORD FileAlignment = 0x200;
		public WORD MajorOperatingSystemVersion = 4;
		public WORD MinorOperatingSystemVersion = 0;
		public WORD MajorImageVersion = 0;
		public WORD MinorImageVersion = 0;
		public WORD MajorSubsystemVersion = 4;
		public WORD MinorSubsystemVersion = 0;
		public DWORD Win32VersionValue = 0;
		public DWORD SizeOfImage;
		public DWORD SizeOfHeaders;
		public DWORD CheckSum = 0;
		public WORD Subsystem;
		public WORD DllCharacteristics;
		public ULONGLONG SizeOfStackReserve;
		public ULONGLONG SizeOfStackCommit = 0x1000;
		public ULONGLONG SizeOfHeapReserve = 0x100000;
		public ULONGLONG SizeOfHeapCommit = 0x1000;
		public DWORD LoaderFlags = 0;
		public DWORD NumberOfRvaAndSizes = 16;
		public IMAGE_DATA_DIRECTORY[] DataDirectory = new IMAGE_DATA_DIRECTORY[16];

		internal void Write(BinaryWriter bw)
		{
			bw.Write(Magic);
			bw.Write(MajorLinkerVersion);
			bw.Write(MinorLinkerVersion);
			bw.Write(SizeOfCode);
			bw.Write(SizeOfInitializedData);
			bw.Write(SizeOfUninitializedData);
			bw.Write(AddressOfEntryPoint);
			bw.Write(BaseOfCode);
			if (Magic == IMAGE_NT_OPTIONAL_HDR32_MAGIC)
			{
				bw.Write(BaseOfData);
				bw.Write((DWORD)ImageBase);
			}
			else
			{
				bw.Write(ImageBase);
			}
			bw.Write(SectionAlignment);
			bw.Write(FileAlignment);
			bw.Write(MajorOperatingSystemVersion);
			bw.Write(MinorOperatingSystemVersion);
			bw.Write(MajorImageVersion);
			bw.Write(MinorImageVersion);
			bw.Write(MajorSubsystemVersion);
			bw.Write(MinorSubsystemVersion);
			bw.Write(Win32VersionValue);
			bw.Write(SizeOfImage);
			bw.Write(SizeOfHeaders);
			bw.Write(CheckSum);
			bw.Write(Subsystem);
			bw.Write(DllCharacteristics);
			if (Magic == IMAGE_NT_OPTIONAL_HDR32_MAGIC)
			{
				bw.Write((DWORD)SizeOfStackReserve);
			}
			else
			{
				bw.Write(SizeOfStackReserve);
			}
			if (Magic == IMAGE_NT_OPTIONAL_HDR32_MAGIC)
			{
				bw.Write((DWORD)SizeOfStackCommit);
			}
			else
			{
				bw.Write(SizeOfStackCommit);
			}
			if (Magic == IMAGE_NT_OPTIONAL_HDR32_MAGIC)
			{
				bw.Write((DWORD)SizeOfHeapReserve);
			}
			else
			{
				bw.Write(SizeOfHeapReserve);
			}
			if (Magic == IMAGE_NT_OPTIONAL_HDR32_MAGIC)
			{
				bw.Write((DWORD)SizeOfHeapCommit);
			}
			else
			{
				bw.Write(SizeOfHeapCommit);
			}
			bw.Write(LoaderFlags);
			bw.Write(NumberOfRvaAndSizes);
			for (int i = 0; i < DataDirectory.Length; i++)
			{
				bw.Write(DataDirectory[i].VirtualAddress);
				bw.Write(DataDirectory[i].Size);
			}
		}
	}

	class SectionHeader
	{
		public const DWORD IMAGE_SCN_CNT_CODE = 0x00000020;
		public const DWORD IMAGE_SCN_CNT_INITIALIZED_DATA = 0x00000040;
		public const DWORD IMAGE_SCN_MEM_DISCARDABLE = 0x02000000;
		public const DWORD IMAGE_SCN_MEM_EXECUTE = 0x20000000;
		public const DWORD IMAGE_SCN_MEM_READ = 0x40000000;
		public const DWORD IMAGE_SCN_MEM_WRITE = 0x80000000;

		public string Name;		// 8 byte UTF8 encoded 0-padded
		public DWORD VirtualSize;
		public DWORD VirtualAddress;
		public DWORD SizeOfRawData;
		public DWORD PointerToRawData;
#pragma warning disable 649 // the follow fields are never assigned to
		public DWORD PointerToRelocations;
		public DWORD PointerToLinenumbers;
		public WORD NumberOfRelocations;
		public WORD NumberOfLinenumbers;
#pragma warning restore 649
		public DWORD Characteristics;
	}
}
