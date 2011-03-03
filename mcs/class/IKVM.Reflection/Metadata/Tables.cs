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
using IKVM.Reflection.Emit;
using IKVM.Reflection.Writer;
using IKVM.Reflection.Reader;

namespace IKVM.Reflection.Metadata
{
	internal abstract class Table
	{
		internal bool IsBig
		{
			get { return RowCount > 65535; }
		}

		internal abstract int RowCount { get; set; }

		internal abstract void Write(MetadataWriter mw);
		internal abstract void Read(MetadataReader mr);

		internal int GetLength(MetadataWriter md)
		{
			return RowCount * GetRowSize(new RowSizeCalc(md));
		}

		protected abstract int GetRowSize(RowSizeCalc rsc);

		protected sealed class RowSizeCalc
		{
			private readonly MetadataWriter mw;
			private int size;

			internal RowSizeCalc(MetadataWriter mw)
			{
				this.mw = mw;
			}

			internal RowSizeCalc AddFixed(int size)
			{
				this.size += size;
				return this;
			}

			internal RowSizeCalc WriteStringIndex()
			{
				if (mw.bigStrings)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteGuidIndex()
			{
				if (mw.bigGuids)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteBlobIndex()
			{
				if (mw.bigBlobs)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteTypeDefOrRef()
			{
				if (mw.bigTypeDefOrRef)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteField()
			{
				if (mw.bigField)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMethodDef()
			{
				if (mw.bigMethodDef)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteParam()
			{
				if (mw.bigParam)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteResolutionScope()
			{
				if (mw.bigResolutionScope)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMemberRefParent()
			{
				if (mw.bigMemberRefParent)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasCustomAttribute()
			{
				if (mw.bigHasCustomAttribute)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteCustomAttributeType()
			{
				if (mw.bigCustomAttributeType)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasConstant()
			{
				if (mw.bigHasConstant)
				{
					size += 4;
				}
				else
				{
					size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteTypeDef()
			{
				if (mw.bigTypeDef)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMethodDefOrRef()
			{
				if (mw.bigMethodDefOrRef)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteEvent()
			{
				if (mw.bigEvent)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteProperty()
			{
				if (mw.bigProperty)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasSemantics()
			{
				if (mw.bigHasSemantics)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteImplementation()
			{
				if (mw.bigImplementation)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteTypeOrMethodDef()
			{
				if (mw.bigTypeOrMethodDef)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteGenericParam()
			{
				if (mw.bigGenericParam)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasDeclSecurity()
			{
				if (mw.bigHasDeclSecurity)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteMemberForwarded()
			{
				if (mw.bigMemberForwarded)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteModuleRef()
			{
				if (mw.bigModuleRef)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal RowSizeCalc WriteHasFieldMarshal()
			{
				if (mw.bigHasFieldMarshal)
				{
					this.size += 4;
				}
				else
				{
					this.size += 2;
				}
				return this;
			}

			internal int Value
			{
				get { return size; }
			}
		}
	}

	abstract class Table<T> : Table
	{
		internal T[] records = new T[1];
		protected int rowCount;

		internal sealed override int RowCount
		{
			get { return rowCount; }
			set { rowCount = value; records = new T[value]; }
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			throw new InvalidOperationException();
		}

		internal int AddRecord(T newRecord)
		{
			if (rowCount == records.Length)
			{
				T[] newarr = new T[records.Length * 2];
				Array.Copy(records, newarr, records.Length);
				records = newarr;
			}
			records[rowCount++] = newRecord;
			return rowCount;
		}

		internal int AddVirtualRecord()
		{
			return ++rowCount;
		}

		internal override void Write(MetadataWriter mw)
		{
			throw new InvalidOperationException();
		}
	}

	sealed class ModuleTable : Table<ModuleTable.Record>
	{
		internal const int Index = 0x00;

		internal struct Record
		{
			internal short Generation;
			internal int Name; // -> StringHeap
			internal int Mvid; // -> GuidHeap
			internal int EncId; // -> GuidHeap
			internal int EncBaseId; // -> GuidHeap
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Generation = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Mvid = mr.ReadGuidIndex();
				records[i].EncId = mr.ReadGuidIndex();
				records[i].EncBaseId = mr.ReadGuidIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Generation);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteGuidIndex(records[i].Mvid);
				mw.WriteGuidIndex(records[i].EncId);
				mw.WriteGuidIndex(records[i].EncBaseId);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteStringIndex()
				.WriteGuidIndex()
				.WriteGuidIndex()
				.WriteGuidIndex()
				.Value;
		}

		internal void Add(short generation, int name, int mvid, int encid, int encbaseid)
		{
			Record record = new Record();
			record.Generation = generation;
			record.Name = name;
			record.Mvid = mvid;
			record.EncId = encid;
			record.EncBaseId = encbaseid;
			AddRecord(record);
		}
	}

	sealed class TypeRefTable : Table<TypeRefTable.Record>
	{
		internal const int Index = 0x01;

		internal struct Record
		{
			internal int ResolutionScope;
			internal int TypeName;
			internal int TypeNameSpace;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].ResolutionScope = mr.ReadResolutionScope();
				records[i].TypeName = mr.ReadStringIndex();
				records[i].TypeNameSpace = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteResolutionScope(records[i].ResolutionScope);
				mw.WriteStringIndex(records[i].TypeName);
				mw.WriteStringIndex(records[i].TypeNameSpace);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteResolutionScope()
				.WriteStringIndex()
				.WriteStringIndex()
				.Value;
		}
	}

	sealed class TypeDefTable : Table<TypeDefTable.Record>
	{
		internal const int Index = 0x02;

		internal struct Record
		{
			internal int Flags;
			internal int TypeName;
			internal int TypeNamespace;
			internal int Extends;
			internal int FieldList;
			internal int MethodList;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt32();
				records[i].TypeName = mr.ReadStringIndex();
				records[i].TypeNamespace = mr.ReadStringIndex();
				records[i].Extends = mr.ReadTypeDefOrRef();
				records[i].FieldList = mr.ReadField();
				records[i].MethodList = mr.ReadMethodDef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteTypeDefTable(mw);
		}

		internal int AllocToken()
		{
			return 0x02000000 + AddVirtualRecord();
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(4)
				.WriteStringIndex()
				.WriteStringIndex()
				.WriteTypeDefOrRef()
				.WriteField()
				.WriteMethodDef()
				.Value;
		}
	}

	sealed class FieldPtrTable : Table<int>
	{
		internal const int Index = 0x03;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadField();
			}
		}
	}

	sealed class FieldTable : Table<FieldTable.Record>
	{
		internal const int Index = 0x04;

		internal struct Record
		{
			internal short Flags;
			internal int Name;
			internal int Signature;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Signature = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteFieldTable(mw);
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteStringIndex()
				.WriteBlobIndex()
				.Value;
		}
	}

	sealed class MethodPtrTable : Table<int>
	{
		internal const int Index = 0x05;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadMethodDef();
			}
		}
	}

	sealed class MethodDefTable : Table<MethodDefTable.Record>
	{
		internal const int Index = 0x06;
		private int baseRVA;

		internal struct Record
		{
			internal int RVA;
			internal short ImplFlags;
			internal short Flags;
			internal int Name;
			internal int Signature;
			internal int ParamList;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].RVA = mr.ReadInt32();
				records[i].ImplFlags = mr.ReadInt16();
				records[i].Flags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Signature = mr.ReadBlobIndex();
				records[i].ParamList = mr.ReadParam();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteMethodDefTable(baseRVA, mw);
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(8)
				.WriteStringIndex()
				.WriteBlobIndex()
				.WriteParam()
				.Value;
		}

		internal void Fixup(TextSection code)
		{
			baseRVA = (int)code.MethodBodiesRVA;
		}
	}

	sealed class ParamPtrTable : Table<int>
	{
		internal const int Index = 0x07;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadParam();
			}
		}
	}

	sealed class ParamTable : Table<ParamTable.Record>
	{
		internal const int Index = 0x08;

		internal struct Record
		{
			internal short Flags;
			internal short Sequence;
			internal int Name;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt16();
				records[i].Sequence = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			mw.ModuleBuilder.WriteParamTable(mw);
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(4)
				.WriteStringIndex()
				.Value;
		}
	}

	sealed class InterfaceImplTable : Table<InterfaceImplTable.Record>, IComparer<InterfaceImplTable.Record>
	{
		internal const int Index = 0x09;

		internal struct Record
		{
			internal int Class;
			internal int Interface;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Class = mr.ReadTypeDef();
				records[i].Interface = mr.ReadTypeDefOrRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].Class);
				mw.WriteEncodedTypeDefOrRef(records[i].Interface);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteTypeDef()
				.WriteTypeDefOrRef()
				.Value;
		}

		internal void Fixup()
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = records[i].Interface;
				switch (token >> 24)
				{
					case 0:
						break;
					case TypeDefTable.Index:
						token = (token & 0xFFFFFF) << 2 | 0;
						break;
					case TypeRefTable.Index:
						token = (token & 0xFFFFFF) << 2 | 1;
						break;
					case TypeSpecTable.Index:
						token = (token & 0xFFFFFF) << 2 | 2;
						break;
					default:
						throw new InvalidOperationException();
				}
				records[i].Interface = token;
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			if (x.Class == y.Class)
			{
				return x.Interface == y.Interface ? 0 : (x.Interface > y.Interface ? 1 : -1);
			}
			return x.Class > y.Class ? 1 : -1;
		}
	}

	sealed class MemberRefTable : Table<MemberRefTable.Record>
	{
		internal const int Index = 0x0A;

		internal struct Record
		{
			internal int Class;
			internal int Name;
			internal int Signature;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Class = mr.ReadMemberRefParent();
				records[i].Name = mr.ReadStringIndex();
				records[i].Signature = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteMemberRefParent(records[i].Class);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteBlobIndex(records[i].Signature);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteMemberRefParent()
				.WriteStringIndex()
				.WriteBlobIndex()
				.Value;
		}

		internal int FindOrAddRecord(Record record)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Class == record.Class
					&& records[i].Name == record.Name
					&& records[i].Signature == record.Signature)
				{
					return i + 1;
				}
			}
			return AddRecord(record);
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (moduleBuilder.IsPseudoToken(records[i].Class))
				{
					records[i].Class = moduleBuilder.ResolvePseudoToken(records[i].Class);
				}
			}
		}
	}

	sealed class ConstantTable : Table<ConstantTable.Record>, IComparer<ConstantTable.Record>
	{
		internal const int Index = 0x0B;

		internal struct Record
		{
			internal short Type;
			internal int Parent;
			internal int Value;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Type = mr.ReadInt16();
				records[i].Parent = mr.ReadHasConstant();
				records[i].Value = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Type);
				mw.WriteHasConstant(records[i].Parent);
				mw.WriteBlobIndex(records[i].Value);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteHasConstant()
				.WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = records[i].Parent;
				if (moduleBuilder.IsPseudoToken(token))
				{
					token = moduleBuilder.ResolvePseudoToken(token);
				}
				// do the HasConstant encoding, so that we can sort the table
				switch (token >> 24)
				{
					case FieldTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 2 | 0;
						break;
					case ParamTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 2 | 1;
						break;
					case PropertyTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 2 | 2;
						break;
					default:
						throw new InvalidOperationException();
				}
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Parent == y.Parent ? 0 : (x.Parent > y.Parent ? 1 : -1);
		}

		internal object GetRawConstantValue(Module module, int parent)
		{
			// TODO use binary search (if sorted)
			for (int i = 0; i < module.Constant.records.Length; i++)
			{
				if (module.Constant.records[i].Parent == parent)
				{
					ByteReader br = module.GetBlob(module.Constant.records[i].Value);
					switch (module.Constant.records[i].Type)
					{
						// see ModuleBuilder.AddConstant for the encodings
						case Signature.ELEMENT_TYPE_BOOLEAN:
							return br.ReadByte() != 0;
						case Signature.ELEMENT_TYPE_I1:
							return br.ReadSByte();
						case Signature.ELEMENT_TYPE_I2:
							return br.ReadInt16();
						case Signature.ELEMENT_TYPE_I4:
							return br.ReadInt32();
						case Signature.ELEMENT_TYPE_I8:
							return br.ReadInt64();
						case Signature.ELEMENT_TYPE_U1:
							return br.ReadByte();
						case Signature.ELEMENT_TYPE_U2:
							return br.ReadUInt16();
						case Signature.ELEMENT_TYPE_U4:
							return br.ReadUInt32();
						case Signature.ELEMENT_TYPE_U8:
							return br.ReadUInt64();
						case Signature.ELEMENT_TYPE_R4:
							return br.ReadSingle();
						case Signature.ELEMENT_TYPE_R8:
							return br.ReadDouble();
						case Signature.ELEMENT_TYPE_CHAR:
							return br.ReadChar();
						case Signature.ELEMENT_TYPE_STRING:
							{
								char[] chars = new char[br.Length / 2];
								for (int j = 0; j < chars.Length; j++)
								{
									chars[j] = br.ReadChar();
								}
								return new String(chars);
							}
						case Signature.ELEMENT_TYPE_CLASS:
							if (br.ReadInt32() != 0)
							{
								throw new BadImageFormatException();
							}
							return null;
						default:
							throw new BadImageFormatException();
					}
				}
			}
			throw new InvalidOperationException();
		}
	}

	sealed class CustomAttributeTable : Table<CustomAttributeTable.Record>, IComparer<CustomAttributeTable.Record>
	{
		internal const int Index = 0x0C;

		internal struct Record
		{
			internal int Parent;
			internal int Type;
			internal int Value;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadHasCustomAttribute();
				records[i].Type = mr.ReadCustomAttributeType();
				records[i].Value = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteHasCustomAttribute(records[i].Parent);
				mw.WriteCustomAttributeType(records[i].Type);
				mw.WriteBlobIndex(records[i].Value);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteHasCustomAttribute()
				.WriteCustomAttributeType()
				.WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			int[] genericParamFixup = moduleBuilder.GenericParam.GetIndexFixup();
			for (int i = 0; i < rowCount; i++)
			{
				if (moduleBuilder.IsPseudoToken(records[i].Type))
				{
					records[i].Type = moduleBuilder.ResolvePseudoToken(records[i].Type);
				}
				int token = records[i].Parent;
				if (moduleBuilder.IsPseudoToken(token))
				{
					token = moduleBuilder.ResolvePseudoToken(token);
				}
				// do the HasCustomAttribute encoding, so that we can sort the table
				switch (token >> 24)
				{
					case MethodDefTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 0;
						break;
					case FieldTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 1;
						break;
					case TypeRefTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 2;
						break;
					case TypeDefTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 3;
						break;
					case ParamTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 4;
						break;
					case InterfaceImplTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 5;
						break;
					case MemberRefTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 6;
						break;
					case ModuleTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 7;
						break;
					// Permission (8) table doesn't exist in the spec
					case PropertyTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 9;
						break;
					case EventTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 10;
						break;
					case StandAloneSigTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 11;
						break;
					case ModuleRefTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 12;
						break;
					case TypeSpecTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 13;
						break;
					case AssemblyTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 14;
						break;
					case AssemblyRefTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 15;
						break;
					case FileTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 16;
						break;
					case ExportedTypeTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 17;
						break;
					case ManifestResourceTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 5 | 18;
						break;
					case GenericParamTable.Index:
						records[i].Parent = (genericParamFixup[(token & 0xFFFFFF) - 1] + 1) << 5 | 19;
						break;
					default:
						throw new InvalidOperationException();
				}
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Parent == y.Parent ? 0 : (x.Parent > y.Parent ? 1 : -1);
		}
	}

	sealed class FieldMarshalTable : Table<FieldMarshalTable.Record>, IComparer<FieldMarshalTable.Record>
	{
		internal const int Index = 0x0D;

		internal struct Record
		{
			internal int Parent;
			internal int NativeType;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadHasFieldMarshal();
				records[i].NativeType = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteHasFieldMarshal(records[i].Parent);
				mw.WriteBlobIndex(records[i].NativeType);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteHasFieldMarshal()
				.WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = moduleBuilder.ResolvePseudoToken(records[i].Parent);
				// do the HasFieldMarshal encoding, so that we can sort the table
				switch (token >> 24)
				{
					case FieldTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 1 | 0;
						break;
					case ParamTable.Index:
						records[i].Parent = (token & 0xFFFFFF) << 1 | 1;
						break;
					default:
						throw new InvalidOperationException();
				}
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Parent == y.Parent ? 0 : (x.Parent > y.Parent ? 1 : -1);
		}
	}

	sealed class DeclSecurityTable : Table<DeclSecurityTable.Record>, IComparer<DeclSecurityTable.Record>
	{
		internal const int Index = 0x0E;

		internal struct Record
		{
			internal short Action;
			internal int Parent;
			internal int PermissionSet;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Action = mr.ReadInt16();
				records[i].Parent = mr.ReadHasDeclSecurity();
				records[i].PermissionSet = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Action);
				mw.WriteHasDeclSecurity(records[i].Parent);
				mw.WriteBlobIndex(records[i].PermissionSet);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteHasDeclSecurity()
				.WriteBlobIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = records[i].Parent;
				if (moduleBuilder.IsPseudoToken(token))
				{
					token = moduleBuilder.ResolvePseudoToken(token);
				}
				// do the HasDeclSecurity encoding, so that we can sort the table
				switch (token >> 24)
				{
					case TypeDefTable.Index:
						token = (token & 0xFFFFFF) << 2 | 0;
						break;
					case MethodDefTable.Index:
						token = (token & 0xFFFFFF) << 2 | 1;
						break;
					case AssemblyTable.Index:
						token = (token & 0xFFFFFF) << 2 | 2;
						break;
					default:
						throw new InvalidOperationException();
				}
				records[i].Parent = token;
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Parent == y.Parent ? 0 : (x.Parent > y.Parent ? 1 : -1);
		}
	}

	sealed class ClassLayoutTable : Table<ClassLayoutTable.Record>, IComparer<ClassLayoutTable.Record>
	{
		internal const int Index = 0x0f;

		internal struct Record
		{
			internal short PackingSize;
			internal int ClassSize;
			internal int Parent;
		}

		internal void AddOrReplaceRecord(Record rec)
		{
			for (int i = 0; i < records.Length; i++)
			{
				if (records[i].Parent == rec.Parent)
				{
					records[i] = rec;
					return;
				}
			}
			AddRecord(rec);
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].PackingSize = mr.ReadInt16();
				records[i].ClassSize = mr.ReadInt32();
				records[i].Parent = mr.ReadTypeDef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			Array.Sort(records, 0, rowCount, this);
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].PackingSize);
				mw.Write(records[i].ClassSize);
				mw.WriteTypeDef(records[i].Parent);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(6)
				.WriteTypeDef()
				.Value;
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Parent == y.Parent ? 0 : (x.Parent > y.Parent ? 1 : -1);
		}

		internal void GetLayout(int token, ref int pack, ref int size)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Parent == token)
				{
					pack = records[i].PackingSize;
					size = records[i].ClassSize;
					break;
				}
			}
		}
	}

	sealed class FieldLayoutTable : Table<FieldLayoutTable.Record>, IComparer<FieldLayoutTable.Record>
	{
		internal const int Index = 0x10;

		internal struct Record
		{
			internal int Offset;
			internal int Field;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Offset = mr.ReadInt32();
				records[i].Field = mr.ReadField();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Offset);
				mw.WriteField(records[i].Field);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(4)
				.WriteField()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				records[i].Field = moduleBuilder.ResolvePseudoToken(records[i].Field);
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Field == y.Field ? 0 : (x.Field > y.Field ? 1 : -1);
		}
	}

	sealed class StandAloneSigTable : Table<int>
	{
		internal const int Index = 0x11;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteBlobIndex(records[i]);
			}
		}

		protected override int GetRowSize(Table.RowSizeCalc rsc)
		{
			return rsc.WriteBlobIndex().Value;
		}

		internal int FindOrAddRecord(int blob)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i] == blob)
				{
					return i + 1;
				}
			}
			return AddRecord(blob);
		}
	}

	sealed class EventMapTable : Table<EventMapTable.Record>
	{
		internal const int Index = 0x12;

		internal struct Record
		{
			internal int Parent;
			internal int EventList;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadTypeDef();
				records[i].EventList = mr.ReadEvent();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].Parent);
				mw.WriteEvent(records[i].EventList);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteTypeDef()
				.WriteEvent()
				.Value;
		}
	}

	sealed class EventPtrTable : Table<int>
	{
		internal const int Index = 0x13;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadEvent();
			}
		}
	}

	sealed class EventTable : Table<EventTable.Record>
	{
		internal const int Index = 0x14;

		internal struct Record
		{
			internal short EventFlags;
			internal int Name;
			internal int EventType;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].EventFlags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].EventType = mr.ReadTypeDefOrRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].EventFlags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteTypeDefOrRef(records[i].EventType);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteStringIndex()
				.WriteTypeDefOrRef()
				.Value;
		}
	}

	sealed class PropertyMapTable : Table<PropertyMapTable.Record>
	{
		internal const int Index = 0x15;

		internal struct Record
		{
			internal int Parent;
			internal int PropertyList;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Parent = mr.ReadTypeDef();
				records[i].PropertyList = mr.ReadProperty();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].Parent);
				mw.WriteProperty(records[i].PropertyList);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteTypeDef()
				.WriteProperty()
				.Value;
		}
	}

	sealed class PropertyPtrTable : Table<int>
	{
		internal const int Index = 0x16;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadProperty();
			}
		}
	}

	sealed class PropertyTable : Table<PropertyTable.Record>
	{
		internal const int Index = 0x17;

		internal struct Record
		{
			internal short Flags;
			internal int Name;
			internal int Type;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt16();
				records[i].Name = mr.ReadStringIndex();
				records[i].Type = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Flags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteBlobIndex(records[i].Type);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteStringIndex()
				.WriteBlobIndex()
				.Value;
		}
	}

	sealed class MethodSemanticsTable : Table<MethodSemanticsTable.Record>, IComparer<MethodSemanticsTable.Record>
	{
		internal const int Index = 0x18;

		// semantics
		internal const short Setter = 0x0001;
		internal const short Getter = 0x0002;
		internal const short Other = 0x0004;
		internal const short AddOn = 0x0008;
		internal const short RemoveOn = 0x0010;
		internal const short Fire = 0x0020;

		internal struct Record
		{
			internal short Semantics;
			internal int Method;
			internal int Association;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Semantics = mr.ReadInt16();
				records[i].Method = mr.ReadMethodDef();
				records[i].Association = mr.ReadHasSemantics();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Semantics);
				mw.WriteMethodDef(records[i].Method);
				mw.WriteHasSemantics(records[i].Association);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteMethodDef()
				.WriteHasSemantics()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (moduleBuilder.IsPseudoToken(records[i].Method))
				{
					records[i].Method = moduleBuilder.ResolvePseudoToken(records[i].Method);
				}
				int token = records[i].Association;
				// do the HasSemantics encoding, so that we can sort the table
				switch (token >> 24)
				{
					case EventTable.Index:
						token = (token & 0xFFFFFF) << 1 | 0;
						break;
					case PropertyTable.Index:
						token = (token & 0xFFFFFF) << 1 | 1;
						break;
					default:
						throw new InvalidOperationException();
				}
				records[i].Association = token;
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Association == y.Association ? 0 : (x.Association > y.Association ? 1 : -1);
		}

		internal MethodInfo GetMethod(Module module, int token, bool nonPublic, short semantics)
		{
			int i = 0;
			return GetNextMethod(module, token, nonPublic, semantics, ref i);
		}

		internal MethodInfo[] GetMethods(Module module, int token, bool nonPublic, short semantics)
		{
			List<MethodInfo> methods = new List<MethodInfo>();
			MethodInfo method;
			for (int i = 0; (method = GetNextMethod(module, token, nonPublic, semantics, ref i)) != null; )
			{
				methods.Add(method);
			}
			return methods.ToArray();
		}

		private MethodInfo GetNextMethod(Module module, int token, bool nonPublic, short semantics, ref int i)
		{
			// TODO use binary search?
			for (; i < records.Length; i++)
			{
				if (records[i].Association == token)
				{
					if ((records[i].Semantics & semantics) != 0)
					{
						MethodInfo method = (MethodInfo)module.ResolveMethod((MethodDefTable.Index << 24) + records[i].Method);
						if (nonPublic || method.IsPublic)
						{
							i++;
							return method;
						}
					}
				}
			}
			return null;
		}

		internal void ComputeFlags(Module module, int token, out bool isPublic, out bool isStatic)
		{
			isPublic = false;
			isStatic = false;
			MethodInfo method;
			for (int i = 0; (method = GetNextMethod(module, token, true, -1, ref i)) != null; )
			{
				if (method.IsPublic)
				{
					isPublic = true;
				}
				if (method.IsStatic)
				{
					isStatic = true;
				}
			}
		}
	}

	sealed class MethodImplTable : Table<MethodImplTable.Record>, IComparer<MethodImplTable.Record>
	{
		internal const int Index = 0x19;

		internal struct Record
		{
			internal int Class;
			internal int MethodBody;
			internal int MethodDeclaration;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Class = mr.ReadTypeDef();
				records[i].MethodBody = mr.ReadMethodDefOrRef();
				records[i].MethodDeclaration = mr.ReadMethodDefOrRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].Class);
				mw.WriteMethodDefOrRef(records[i].MethodBody);
				mw.WriteMethodDefOrRef(records[i].MethodDeclaration);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteTypeDef()
				.WriteMethodDefOrRef()
				.WriteMethodDefOrRef()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (moduleBuilder.IsPseudoToken(records[i].MethodBody))
				{
					records[i].MethodBody = moduleBuilder.ResolvePseudoToken(records[i].MethodBody);
				}
				if (moduleBuilder.IsPseudoToken(records[i].MethodDeclaration))
				{
					records[i].MethodDeclaration = moduleBuilder.ResolvePseudoToken(records[i].MethodDeclaration);
				}
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Class == y.Class ? 0 : (x.Class > y.Class ? 1 : -1);
		}
	}

	sealed class ModuleRefTable : Table<int>
	{
		internal const int Index = 0x1A;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteStringIndex(records[i]);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteStringIndex()
				.Value;
		}

		internal int FindOrAddRecord(int str)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i] == str)
				{
					return i + 1;
				}
			}
			return AddRecord(str);
		}
	}

	sealed class TypeSpecTable : Table<int>
	{
		internal const int Index = 0x1B;

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i] = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteBlobIndex(records[i]);
			}
		}

		protected override int GetRowSize(Table.RowSizeCalc rsc)
		{
			return rsc.WriteBlobIndex().Value;
		}
	}

	sealed class ImplMapTable : Table<ImplMapTable.Record>, IComparer<ImplMapTable.Record>
	{
		internal const int Index = 0x1C;

		internal struct Record
		{
			internal short MappingFlags;
			internal int MemberForwarded;
			internal int ImportName;
			internal int ImportScope;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].MappingFlags = mr.ReadInt16();
				records[i].MemberForwarded = mr.ReadMemberForwarded();
				records[i].ImportName = mr.ReadStringIndex();
				records[i].ImportScope = mr.ReadModuleRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].MappingFlags);
				mw.WriteMemberForwarded(records[i].MemberForwarded);
				mw.WriteStringIndex(records[i].ImportName);
				mw.WriteModuleRef(records[i].ImportScope);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(2)
				.WriteMemberForwarded()
				.WriteStringIndex()
				.WriteModuleRef()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (moduleBuilder.IsPseudoToken(records[i].MemberForwarded))
				{
					records[i].MemberForwarded = moduleBuilder.ResolvePseudoToken(records[i].MemberForwarded);
				}
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.MemberForwarded == y.MemberForwarded ? 0 : (x.MemberForwarded > y.MemberForwarded ? 1 : -1);
		}
	}

	sealed class FieldRVATable : Table<FieldRVATable.Record>, IComparer<FieldRVATable.Record>
	{
		internal const int Index = 0x1D;

		internal struct Record
		{
			internal int RVA;
			internal int Field;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].RVA = mr.ReadInt32();
				records[i].Field = mr.ReadField();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].RVA);
				mw.WriteField(records[i].Field);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(4)
				.WriteField()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder, int sdataRVA)
		{
			for (int i = 0; i < rowCount; i++)
			{
				records[i].RVA += sdataRVA;
				if (moduleBuilder.IsPseudoToken(records[i].Field))
				{
					records[i].Field = moduleBuilder.ResolvePseudoToken(records[i].Field);
				}
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Field == y.Field ? 0 : (x.Field > y.Field ? 1 : -1);
		}
	}

	sealed class AssemblyTable : Table<AssemblyTable.Record>
	{
		internal const int Index = 0x20;

		internal struct Record
		{
			internal int HashAlgId;
			internal ushort MajorVersion;
			internal ushort MinorVersion;
			internal ushort BuildNumber;
			internal ushort RevisionNumber;
			internal int Flags;
			internal int PublicKey;
			internal int Name;
			internal int Culture;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].HashAlgId = mr.ReadInt32();
				records[i].MajorVersion = mr.ReadUInt16();
				records[i].MinorVersion = mr.ReadUInt16();
				records[i].BuildNumber = mr.ReadUInt16();
				records[i].RevisionNumber = mr.ReadUInt16();
				records[i].Flags = mr.ReadInt32();
				records[i].PublicKey = mr.ReadBlobIndex();
				records[i].Name = mr.ReadStringIndex();
				records[i].Culture = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].HashAlgId);
				mw.Write(records[i].MajorVersion);
				mw.Write(records[i].MinorVersion);
				mw.Write(records[i].BuildNumber);
				mw.Write(records[i].RevisionNumber);
				mw.Write(records[i].Flags);
				mw.WriteBlobIndex(records[i].PublicKey);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteStringIndex(records[i].Culture);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(16)
				.WriteBlobIndex()
				.WriteStringIndex()
				.WriteStringIndex()
				.Value;
		}
	}

	sealed class AssemblyRefTable : Table<AssemblyRefTable.Record>
	{
		internal const int Index = 0x23;

		internal struct Record
		{
			internal ushort MajorVersion;
			internal ushort MinorVersion;
			internal ushort BuildNumber;
			internal ushort RevisionNumber;
			internal int Flags;
			internal int PublicKeyOrToken;
			internal int Name;
			internal int Culture;
			internal int HashValue;
		}

		internal int FindOrAddRecord(Record rec)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Name == rec.Name
					&& records[i].MajorVersion == rec.MajorVersion
					&& records[i].MinorVersion == rec.MinorVersion
					&& records[i].BuildNumber == rec.BuildNumber
					&& records[i].RevisionNumber == rec.RevisionNumber
					&& records[i].Flags == rec.Flags
					&& records[i].PublicKeyOrToken == rec.PublicKeyOrToken
					&& records[i].Culture == rec.Culture
					&& records[i].HashValue == rec.HashValue
					)
				{
					return i + 1;
				}
			}
			return AddRecord(rec);
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].MajorVersion = mr.ReadUInt16();
				records[i].MinorVersion = mr.ReadUInt16();
				records[i].BuildNumber = mr.ReadUInt16();
				records[i].RevisionNumber = mr.ReadUInt16();
				records[i].Flags = mr.ReadInt32();
				records[i].PublicKeyOrToken = mr.ReadBlobIndex();
				records[i].Name = mr.ReadStringIndex();
				records[i].Culture = mr.ReadStringIndex();
				records[i].HashValue = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].MajorVersion);
				mw.Write(records[i].MinorVersion);
				mw.Write(records[i].BuildNumber);
				mw.Write(records[i].RevisionNumber);
				mw.Write(records[i].Flags);
				mw.WriteBlobIndex(records[i].PublicKeyOrToken);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteStringIndex(records[i].Culture);
				mw.WriteBlobIndex(records[i].HashValue);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(12)
				.WriteBlobIndex()
				.WriteStringIndex()
				.WriteStringIndex()
				.WriteBlobIndex()
				.Value;
		}
	}

	sealed class FileTable : Table<FileTable.Record>
	{
		internal const int Index = 0x26;

		internal struct Record
		{
			internal int Flags;
			internal int Name;
			internal int HashValue;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt32();
				records[i].Name = mr.ReadStringIndex();
				records[i].HashValue = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Flags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteBlobIndex(records[i].HashValue);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(4)
				.WriteStringIndex()
				.WriteBlobIndex()
				.Value;
		}
	}

	sealed class ExportedTypeTable : Table<ExportedTypeTable.Record>
	{
		internal const int Index = 0x27;

		internal struct Record
		{
			internal int Flags;
			internal int TypeDefId;
			internal int TypeName;
			internal int TypeNamespace;
			internal int Implementation;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Flags = mr.ReadInt32();
				records[i].TypeDefId = mr.ReadInt32();
				records[i].TypeName = mr.ReadStringIndex();
				records[i].TypeNamespace = mr.ReadStringIndex();
				records[i].Implementation = mr.ReadImplementation();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Flags);
				mw.Write(records[i].TypeDefId);
				mw.WriteStringIndex(records[i].TypeName);
				mw.WriteStringIndex(records[i].TypeNamespace);
				mw.WriteImplementation(records[i].Implementation);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(8)
				.WriteStringIndex()
				.WriteStringIndex()
				.WriteImplementation()
				.Value;
		}

		internal int FindOrAddRecord(Record rec)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Implementation == rec.Implementation
					&& records[i].TypeName == rec.TypeName
					&& records[i].TypeNamespace == rec.TypeNamespace)
				{
					return i + 1;
				}
			}
			return AddRecord(rec);
		}
	}

	sealed class ManifestResourceTable : Table<ManifestResourceTable.Record>
	{
		internal const int Index = 0x28;

		internal struct Record
		{
			internal int Offset;
			internal int Flags;
			internal int Name;
			internal int Implementation;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Offset = mr.ReadInt32();
				records[i].Flags = mr.ReadInt32();
				records[i].Name = mr.ReadStringIndex();
				records[i].Implementation = mr.ReadImplementation();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Offset);
				mw.Write(records[i].Flags);
				mw.WriteStringIndex(records[i].Name);
				mw.WriteImplementation(records[i].Implementation);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(8)
				.WriteStringIndex()
				.WriteImplementation()
				.Value;
		}
	}

	sealed class NestedClassTable : Table<NestedClassTable.Record>
	{
		internal const int Index = 0x29;

		internal struct Record
		{
			internal int NestedClass;
			internal int EnclosingClass;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].NestedClass = mr.ReadTypeDef();
				records[i].EnclosingClass = mr.ReadTypeDef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteTypeDef(records[i].NestedClass);
				mw.WriteTypeDef(records[i].EnclosingClass);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteTypeDef()
				.WriteTypeDef()
				.Value;
		}

		internal List<int> GetNestedClasses(int enclosingClass)
		{
			List<int> nestedClasses = new List<int>();
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].EnclosingClass == enclosingClass)
				{
					nestedClasses.Add(records[i].NestedClass);
				}
			}
			return nestedClasses;
		}
	}

	sealed class GenericParamTable : Table<GenericParamTable.Record>, IComparer<GenericParamTable.Record>
	{
		internal const int Index = 0x2A;

		internal struct Record
		{
			internal short Number;
			internal short Flags;
			internal int Owner;
			internal int Name;
			// not part of the table, we use it to be able to fixup the GenericParamConstraint table
			internal int unsortedIndex;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Number = mr.ReadInt16();
				records[i].Flags = mr.ReadInt16();
				records[i].Owner = mr.ReadTypeOrMethodDef();
				records[i].Name = mr.ReadStringIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.Write(records[i].Number);
				mw.Write(records[i].Flags);
				mw.WriteTypeOrMethodDef(records[i].Owner);
				mw.WriteStringIndex(records[i].Name);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.AddFixed(4)
				.WriteTypeOrMethodDef()
				.WriteStringIndex()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				int token = records[i].Owner;
				if (moduleBuilder.IsPseudoToken(token))
				{
					token = moduleBuilder.ResolvePseudoToken(token);
				}
				// do the TypeOrMethodDef encoding, so that we can sort the table
				switch (token >> 24)
				{
					case TypeDefTable.Index:
						records[i].Owner = (token & 0xFFFFFF) << 1 | 0;
						break;
					case MethodDefTable.Index:
						records[i].Owner = (token & 0xFFFFFF) << 1 | 1;
						break;
					default:
						throw new InvalidOperationException();
				}
				records[i].unsortedIndex = i;
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			if (x.Owner == y.Owner)
			{
				return x.Number == y.Number ? 0 : (x.Number > y.Number ? 1 : -1);
			}
			return x.Owner > y.Owner ? 1 : -1;
		}

		internal GenericParameterAttributes GetAttributes(int token)
		{
			return (GenericParameterAttributes)records[(token & 0xFFFFFF) - 1].Flags;
		}

		internal void PatchAttribute(int token, GenericParameterAttributes genericParameterAttributes)
		{
			records[(token & 0xFFFFFF) - 1].Flags = (short)genericParameterAttributes;
		}

		internal int[] GetIndexFixup()
		{
			int[] array = new int[rowCount];
			for (int i = 0; i < rowCount; i++)
			{
				array[records[i].unsortedIndex] = i;
			}
			return array;
		}

		internal int FindFirstByOwner(int token)
		{
			// TODO use binary search (if sorted)
			for (int i = 0; i < records.Length; i++)
			{
				if (records[i].Owner == token)
				{
					return i;
				}
			}
			return -1;
		}
	}

	sealed class MethodSpecTable : Table<MethodSpecTable.Record>
	{
		internal const int Index = 0x2B;

		internal struct Record
		{
			internal int Method;
			internal int Instantiation;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Method = mr.ReadMethodDefOrRef();
				records[i].Instantiation = mr.ReadBlobIndex();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteMethodDefOrRef(records[i].Method);
				mw.WriteBlobIndex(records[i].Instantiation);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteMethodDefOrRef()
				.WriteBlobIndex()
				.Value;
		}

		internal int FindOrAddRecord(Record record)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (records[i].Method == record.Method
					&& records[i].Instantiation == record.Instantiation)
				{
					return i + 1;
				}
			}
			return AddRecord(record);
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			for (int i = 0; i < rowCount; i++)
			{
				if (moduleBuilder.IsPseudoToken(records[i].Method))
				{
					records[i].Method = moduleBuilder.ResolvePseudoToken(records[i].Method);
				}
			}
		}
	}

	sealed class GenericParamConstraintTable : Table<GenericParamConstraintTable.Record>, IComparer<GenericParamConstraintTable.Record>
	{
		internal const int Index = 0x2C;

		internal struct Record
		{
			internal int Owner;
			internal int Constraint;
		}

		internal override void Read(MetadataReader mr)
		{
			for (int i = 0; i < records.Length; i++)
			{
				records[i].Owner = mr.ReadGenericParam();
				records[i].Constraint = mr.ReadTypeDefOrRef();
			}
		}

		internal override void Write(MetadataWriter mw)
		{
			for (int i = 0; i < rowCount; i++)
			{
				mw.WriteGenericParam(records[i].Owner);
				mw.WriteTypeDefOrRef(records[i].Constraint);
			}
		}

		protected override int GetRowSize(RowSizeCalc rsc)
		{
			return rsc
				.WriteGenericParam()
				.WriteTypeDefOrRef()
				.Value;
		}

		internal void Fixup(ModuleBuilder moduleBuilder)
		{
			int[] fixups = moduleBuilder.GenericParam.GetIndexFixup();
			for (int i = 0; i < rowCount; i++)
			{
				records[i].Owner = fixups[records[i].Owner - 1] + 1;
			}
			Array.Sort(records, 0, rowCount, this);
		}

		int IComparer<Record>.Compare(Record x, Record y)
		{
			return x.Owner == y.Owner ? 0 : (x.Owner > y.Owner ? 1 : -1);
		}
	}
}
