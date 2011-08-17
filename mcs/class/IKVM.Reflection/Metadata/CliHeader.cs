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
using System.IO;
using IMAGE_DATA_DIRECTORY = IKVM.Reflection.Reader.IMAGE_DATA_DIRECTORY;

namespace IKVM.Reflection.Metadata
{
	sealed class CliHeader
	{
		internal const uint COMIMAGE_FLAGS_ILONLY = 0x00000001;
		internal const uint COMIMAGE_FLAGS_32BITREQUIRED = 0x00000002;
		internal const uint COMIMAGE_FLAGS_STRONGNAMESIGNED = 0x00000008;
		internal const uint COMIMAGE_FLAGS_NATIVE_ENTRYPOINT = 0x00000010;

		internal uint Cb = 0x48;
		internal ushort MajorRuntimeVersion;
		internal ushort MinorRuntimeVersion;
		internal IMAGE_DATA_DIRECTORY MetaData;
		internal uint Flags;
		internal uint EntryPointToken;
		internal IMAGE_DATA_DIRECTORY Resources;
		internal IMAGE_DATA_DIRECTORY StrongNameSignature;
		internal IMAGE_DATA_DIRECTORY CodeManagerTable;
		internal IMAGE_DATA_DIRECTORY VTableFixups;
		internal IMAGE_DATA_DIRECTORY ExportAddressTableJumps;
		internal IMAGE_DATA_DIRECTORY ManagedNativeHeader;

		internal void Read(BinaryReader br)
		{
			Cb = br.ReadUInt32();
			MajorRuntimeVersion = br.ReadUInt16();
			MinorRuntimeVersion = br.ReadUInt16();
			MetaData.Read(br);
			Flags = br.ReadUInt32();
			EntryPointToken = br.ReadUInt32();
			Resources.Read(br);
			StrongNameSignature.Read(br);
			CodeManagerTable.Read(br);
			VTableFixups.Read(br);
			ExportAddressTableJumps.Read(br);
			ManagedNativeHeader.Read(br);
		}

		internal void Write(IKVM.Reflection.Writer.MetadataWriter mw)
		{
			mw.Write(Cb);
			mw.Write(MajorRuntimeVersion);
			mw.Write(MinorRuntimeVersion);
			MetaData.Write(mw);
			mw.Write(Flags);
			mw.Write(EntryPointToken);
			Resources.Write(mw);
			StrongNameSignature.Write(mw);
			CodeManagerTable.Write(mw);
			VTableFixups.Write(mw);
			ExportAddressTableJumps.Write(mw);
			ManagedNativeHeader.Write(mw);
		}
	}
}
