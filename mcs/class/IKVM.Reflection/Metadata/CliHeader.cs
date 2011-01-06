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
		internal uint MetaDataRVA;
		internal uint MetaDataSize;
		internal uint Flags;
		internal uint EntryPointToken;
		internal uint ResourcesRVA;
		internal uint ResourcesSize;
		internal uint StrongNameSignatureRVA;
		internal uint StrongNameSignatureSize;
		internal ulong CodeManagerTable;
		internal uint VTableFixupsRVA;
		internal uint VTableFixupsSize;
		internal ulong ExportAddressTableJumps;
		internal ulong ManagedNativeHeader;

		internal void Read(BinaryReader br)
		{
			Cb = br.ReadUInt32();
			MajorRuntimeVersion = br.ReadUInt16();
			MinorRuntimeVersion = br.ReadUInt16();
			MetaDataRVA = br.ReadUInt32();
			MetaDataSize = br.ReadUInt32();
			Flags = br.ReadUInt32();
			EntryPointToken = br.ReadUInt32();
			ResourcesRVA = br.ReadUInt32();
			ResourcesSize = br.ReadUInt32();
			StrongNameSignatureRVA = br.ReadUInt32();
			StrongNameSignatureSize = br.ReadUInt32();
			CodeManagerTable = br.ReadUInt32();
			VTableFixupsRVA = br.ReadUInt32();
			VTableFixupsSize = br.ReadUInt32();
			ExportAddressTableJumps = br.ReadUInt32();
			ManagedNativeHeader = br.ReadUInt32();
		}

		internal void Write(IKVM.Reflection.Writer.MetadataWriter mw)
		{
			mw.Write(Cb);
			mw.Write(MajorRuntimeVersion);
			mw.Write(MinorRuntimeVersion);
			mw.Write(MetaDataRVA);
			mw.Write(MetaDataSize);
			mw.Write(Flags);
			mw.Write(EntryPointToken);
			mw.Write(ResourcesRVA);
			mw.Write(ResourcesSize);
			mw.Write(StrongNameSignatureRVA);
			mw.Write(StrongNameSignatureSize);
			mw.Write(CodeManagerTable);
			mw.Write(VTableFixupsRVA);
			mw.Write(VTableFixupsSize);
			mw.Write(ExportAddressTableJumps);
			mw.Write(ManagedNativeHeader);
		}
	}
}
