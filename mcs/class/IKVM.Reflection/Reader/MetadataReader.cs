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
using System.Collections.Generic;
using System.Text;
using System.IO;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Reader
{
	sealed class MetadataReader : MetadataRW
	{
		private readonly BinaryReader br;

		internal MetadataReader(ModuleReader module, BinaryReader br, byte heapSizes)
			: base(module, (heapSizes & 0x01) != 0, (heapSizes & 0x02) != 0, (heapSizes & 0x04) != 0)
		{
			this.br = br;
		}

		internal short ReadInt16()
		{
			return br.ReadInt16();
		}

		internal ushort ReadUInt16()
		{
			return br.ReadUInt16();
		}

		internal int ReadInt32()
		{
			return br.ReadInt32();
		}

		internal int ReadStringIndex()
		{
			if (bigStrings)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadGuidIndex()
		{
			if (bigGuids)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadBlobIndex()
		{
			if (bigBlobs)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadResolutionScope()
		{
			int codedIndex;
			if (bigResolutionScope)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigTypeDefOrRef)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigMemberRefParent)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigHasCustomAttribute)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigCustomAttributeType)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigMethodDefOrRef)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigHasConstant)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigHasSemantics)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigHasFieldMarshal)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigHasDeclSecurity)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigTypeOrMethodDef)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigMemberForwarded)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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
			int codedIndex;
			if (bigImplementation)
			{
				codedIndex = br.ReadInt32();
			}
			else
			{
				codedIndex = br.ReadUInt16();
			}
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

		private int ReadToken(int table, bool big)
		{
			int rid;
			if (big)
			{
				rid = br.ReadInt32();
			}
			else
			{
				rid = br.ReadUInt16();
			}
			return rid | (table << 24);
		}

		internal int ReadField()
		{
			if (bigField)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadMethodDef()
		{
			if (bigMethodDef)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadParam()
		{
			if (bigParam)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadProperty()
		{
			if (bigProperty)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadEvent()
		{
			if (bigEvent)
			{
				return br.ReadInt32();
			}
			else
			{
				return br.ReadUInt16();
			}
		}

		internal int ReadTypeDef()
		{
			return ReadToken(TypeDefTable.Index, bigTypeDef);
		}

		internal int ReadGenericParam()
		{
			return ReadToken(GenericParamTable.Index, bigGenericParam);
		}

		internal int ReadModuleRef()
		{
			return ReadToken(ModuleRefTable.Index, bigModuleRef);
		}
	}
}
