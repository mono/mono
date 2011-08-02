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
using System.Collections.Generic;
using System.Text;
using System.IO;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	sealed class MetadataReader : MetadataRW
	{
		private readonly Stream stream;
		private const int bufferLength = 2048;
		private readonly byte[] buffer = new byte[bufferLength];
		private int pos = bufferLength;

		internal MetadataReader(ModuleReader module, Stream stream, byte heapSizes)
			: base(module, (heapSizes & 0x01) != 0, (heapSizes & 0x02) != 0, (heapSizes & 0x04) != 0)
		{
			this.stream = stream;
		}

		private void FillBuffer(int needed)
		{
			int count = bufferLength - pos;
			if (count != 0)
			{
				// move remaining bytes to the front of the buffer
				Buffer.BlockCopy(buffer, pos, buffer, 0, count);
			}
			pos = 0;

			while (count < needed)
			{
				int len = stream.Read(buffer, count, bufferLength - count);
				if (len == 0)
				{
					throw new BadImageFormatException();
				}
				count += len;
			}

			if (count != bufferLength)
			{
				// we didn't fill the buffer completely, so have to restore the invariant
				// that all data from pos up until the end of the buffer is valid
				Buffer.BlockCopy(buffer, 0, buffer, bufferLength - count, count);
				pos = bufferLength - count;
			}
		}

		internal ushort ReadUInt16()
		{
			return (ushort)ReadInt16();
		}

		internal short ReadInt16()
		{
			if (pos > bufferLength - 2)
			{
				FillBuffer(2);
			}
			byte b1 = buffer[pos++];
			byte b2 = buffer[pos++];
			return (short)(b1 | (b2 << 8));
		}

		internal int ReadInt32()
		{
			if (pos > bufferLength - 4)
			{
				FillBuffer(4);
			}
			byte b1 = buffer[pos++];
			byte b2 = buffer[pos++];
			byte b3 = buffer[pos++];
			byte b4 = buffer[pos++];
			return b1 | (b2 << 8) | (b3 << 16) | (b4 << 24);
		}

		private int ReadIndex(bool big)
		{
			if (big)
			{
				return ReadInt32();
			}
			else
			{
				return ReadUInt16();
			}
		}

		internal int ReadStringIndex()
		{
			return ReadIndex(bigStrings);
		}

		internal int ReadGuidIndex()
		{
			return ReadIndex(bigGuids);
		}

		internal int ReadBlobIndex()
		{
			return ReadIndex(bigBlobs);
		}

		internal int ReadResolutionScope()
		{
			int codedIndex = ReadIndex(bigResolutionScope);
			switch (codedIndex & 3)
			{
				case 0:
					return (ModuleTable.Index << 24) + (codedIndex >> 2);
				case 1:
					return (ModuleRefTable.Index << 24) + (codedIndex >> 2);
				case 2:
					return (AssemblyRefTable.Index << 24) + (codedIndex >> 2);
				case 3:
					return (TypeRefTable.Index << 24) + (codedIndex >> 2);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadTypeDefOrRef()
		{
			int codedIndex = ReadIndex(bigTypeDefOrRef);
			switch (codedIndex & 3)
			{
				case 0:
					return (TypeDefTable.Index << 24) + (codedIndex >> 2);
				case 1:
					return (TypeRefTable.Index << 24) + (codedIndex >> 2);
				case 2:
					return (TypeSpecTable.Index << 24) + (codedIndex >> 2);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadMemberRefParent()
		{
			int codedIndex = ReadIndex(bigMemberRefParent);
			switch (codedIndex & 7)
			{
				case 0:
					return (TypeDefTable.Index << 24) + (codedIndex >> 3);
				case 1:
					return (TypeRefTable.Index << 24) + (codedIndex >> 3);
				case 2:
					return (ModuleRefTable.Index << 24) + (codedIndex >> 3);
				case 3:
					return (MethodDefTable.Index << 24) + (codedIndex >> 3);
				case 4:
					return (TypeSpecTable.Index << 24) + (codedIndex >> 3);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadHasCustomAttribute()
		{
			int codedIndex = ReadIndex(bigHasCustomAttribute);
			switch (codedIndex & 31)
			{
				case 0:
					return (MethodDefTable.Index << 24) + (codedIndex >> 5);
				case 1:
					return (FieldTable.Index << 24) + (codedIndex >> 5);
				case 2:
					return (TypeRefTable.Index << 24) + (codedIndex >> 5);
				case 3:
					return (TypeDefTable.Index << 24) + (codedIndex >> 5);
				case 4:
					return (ParamTable.Index << 24) + (codedIndex >> 5);
				case 5:
					return (InterfaceImplTable.Index << 24) + (codedIndex >> 5);
				case 6:
					return (MemberRefTable.Index << 24) + (codedIndex >> 5);
				case 7:
					return (ModuleTable.Index << 24) + (codedIndex >> 5);
				case 8:
					throw new BadImageFormatException();
				case 9:
					return (PropertyTable.Index << 24) + (codedIndex >> 5);
				case 10:
					return (EventTable.Index << 24) + (codedIndex >> 5);
				case 11:
					return (StandAloneSigTable.Index << 24) + (codedIndex >> 5);
				case 12:
					return (ModuleRefTable.Index << 24) + (codedIndex >> 5);
				case 13:
					return (TypeSpecTable.Index << 24) + (codedIndex >> 5);
				case 14:
					return (AssemblyTable.Index << 24) + (codedIndex >> 5);
				case 15:
					return (AssemblyRefTable.Index << 24) + (codedIndex >> 5);
				case 16:
					return (FileTable.Index << 24) + (codedIndex >> 5);
				case 17:
					return (ExportedTypeTable.Index << 24) + (codedIndex >> 5);
				case 18:
					return (ManifestResourceTable.Index << 24) + (codedIndex >> 5);
				case 19:
					return (GenericParamTable.Index << 24) + (codedIndex >> 5);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadCustomAttributeType()
		{
			int codedIndex = ReadIndex(bigCustomAttributeType);
			switch (codedIndex & 7)
			{
				case 2:
					return (MethodDefTable.Index << 24) + (codedIndex >> 3);
				case 3:
					return (MemberRefTable.Index << 24) + (codedIndex >> 3);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadMethodDefOrRef()
		{
			int codedIndex = ReadIndex(bigMethodDefOrRef);
			switch (codedIndex & 1)
			{
				case 0:
					return (MethodDefTable.Index << 24) + (codedIndex >> 1);
				case 1:
					return (MemberRefTable.Index << 24) + (codedIndex >> 1);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadHasConstant()
		{
			int codedIndex = ReadIndex(bigHasConstant);
			switch (codedIndex & 3)
			{
				case 0:
					return (FieldTable.Index << 24) + (codedIndex >> 2);
				case 1:
					return (ParamTable.Index << 24) + (codedIndex >> 2);
				case 2:
					return (PropertyTable.Index << 24) + (codedIndex >> 2);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadHasSemantics()
		{
			int codedIndex = ReadIndex(bigHasSemantics);
			switch (codedIndex & 1)
			{
				case 0:
					return (EventTable.Index << 24) + (codedIndex >> 1);
				case 1:
					return (PropertyTable.Index << 24) + (codedIndex >> 1);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadHasFieldMarshal()
		{
			int codedIndex = ReadIndex(bigHasFieldMarshal);
			switch (codedIndex & 1)
			{
				case 0:
					return (FieldTable.Index << 24) + (codedIndex >> 1);
				case 1:
					return (ParamTable.Index << 24) + (codedIndex >> 1);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadHasDeclSecurity()
		{
			int codedIndex = ReadIndex(bigHasDeclSecurity);
			switch (codedIndex & 3)
			{
				case 0:
					return (TypeDefTable.Index << 24) + (codedIndex >> 2);
				case 1:
					return (MethodDefTable.Index << 24) + (codedIndex >> 2);
				case 2:
					return (AssemblyTable.Index << 24) + (codedIndex >> 2);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadTypeOrMethodDef()
		{
			int codedIndex = ReadIndex(bigTypeOrMethodDef);
			switch (codedIndex & 1)
			{
				case 0:
					return (TypeDefTable.Index << 24) + (codedIndex >> 1);
				case 1:
					return (MethodDefTable.Index << 24) + (codedIndex >> 1);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadMemberForwarded()
		{
			int codedIndex = ReadIndex(bigMemberForwarded);
			switch (codedIndex & 1)
			{
				case 0:
					return (FieldTable.Index << 24) + (codedIndex >> 1);
				case 1:
					return (MethodDefTable.Index << 24) + (codedIndex >> 1);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadImplementation()
		{
			int codedIndex = ReadIndex(bigImplementation);
			switch (codedIndex & 3)
			{
				case 0:
					return (FileTable.Index << 24) + (codedIndex >> 2);
				case 1:
					return (AssemblyRefTable.Index << 24) + (codedIndex >> 2);
				case 2:
					return (ExportedTypeTable.Index << 24) + (codedIndex >> 2);
				default:
					throw new BadImageFormatException();
			}
		}

		internal int ReadField()
		{
			return ReadIndex(bigField);
		}

		internal int ReadMethodDef()
		{
			return ReadIndex(bigMethodDef);
		}

		internal int ReadParam()
		{
			return ReadIndex(bigParam);
		}

		internal int ReadProperty()
		{
			return ReadIndex(bigProperty);
		}

		internal int ReadEvent()
		{
			return ReadIndex(bigEvent);
		}

		internal int ReadTypeDef()
		{
			return ReadIndex(bigTypeDef) | (TypeDefTable.Index << 24);
		}

		internal int ReadGenericParam()
		{
			return ReadIndex(bigGenericParam) | (GenericParamTable.Index << 24);
		}

		internal int ReadModuleRef()
		{
			return ReadIndex(bigModuleRef) | (ModuleRefTable.Index << 24);
		}
	}
}
