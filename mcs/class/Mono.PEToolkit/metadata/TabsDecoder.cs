// Auto-generated file - DO NOT EDIT!
// Please edit md-schema.xml or tabs-decoder.xsl if you want to make changes.

using System;

namespace Mono.PEToolkit.Metadata {


	/// <summary>
	/// </summary>
	/// <remarks>
	/// </remarks>
	public sealed class TabsDecoder {

		private TabsDecoder()
		{
		}


		/// <summary>
		/// </summary>
		/// <remarks>
		/// </remarks>
		public static MDToken DecodeToken(CodedTokenId id, int data)
		{
			MDToken res = new MDToken();
			int tag;
			int rid;
			TokenType tok;

			switch (id) {

				case CodedTokenId.TypeDefOrRef :
					tag = data & 0x03;
					rid = (int) ((uint) data >> 2);
					switch (tag) {

						case 0 :
							tok = TokenType.TypeDef;
							break;

						case 1 :
							tok = TokenType.TypeRef;
							break;

						case 2 :
							tok = TokenType.TypeSpec;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for TypeDefOrRef, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.HasConstant :
					tag = data & 0x03;
					rid = (int) ((uint) data >> 2);
					switch (tag) {

						case 0 :
							tok = TokenType.FieldDef;
							break;

						case 1 :
							tok = TokenType.ParamDef;
							break;

						case 2 :
							tok = TokenType.Property;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for HasConstant, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.HasCustomAttribute :
					tag = data & 0x1F;
					rid = (int) ((uint) data >> 5);
					switch (tag) {

						case 0 :
							tok = TokenType.MethodDef;
							break;

						case 1 :
							tok = TokenType.FieldDef;
							break;

						case 2 :
							tok = TokenType.TypeRef;
							break;

						case 3 :
							tok = TokenType.TypeDef;
							break;

						case 4 :
							tok = TokenType.ParamDef;
							break;

						case 5 :
							tok = TokenType.InterfaceImpl;
							break;

						case 6 :
							tok = TokenType.MemberRef;
							break;

						case 7 :
							tok = TokenType.Module;
							break;

						case 8 :
							tok = TokenType.Permission;
							break;

						case 9 :
							tok = TokenType.Property;
							break;

						case 10 :
							tok = TokenType.Event;
							break;

						case 11 :
							tok = TokenType.Signature;
							break;

						case 12 :
							tok = TokenType.ModuleRef;
							break;

						case 13 :
							tok = TokenType.TypeSpec;
							break;

						case 14 :
							tok = TokenType.Assembly;
							break;

						case 15 :
							tok = TokenType.AssemblyRef;
							break;

						case 16 :
							tok = TokenType.File;
							break;

						case 17 :
							tok = TokenType.ExportedType;
							break;

						case 18 :
							tok = TokenType.ManifestResource;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for HasCustomAttribute, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.HasFieldMarshal :
					tag = data & 0x01;
					rid = (int) ((uint) data >> 1);
					switch (tag) {

						case 0 :
							tok = TokenType.FieldDef;
							break;

						case 1 :
							tok = TokenType.ParamDef;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for HasFieldMarshal, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.HasDeclSecurity :
					tag = data & 0x03;
					rid = (int) ((uint) data >> 2);
					switch (tag) {

						case 0 :
							tok = TokenType.TypeDef;
							break;

						case 1 :
							tok = TokenType.MethodDef;
							break;

						case 2 :
							tok = TokenType.Assembly;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for HasDeclSecurity, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.MemberRefParent :
					tag = data & 0x07;
					rid = (int) ((uint) data >> 3);
					switch (tag) {

						case 0 :
							tok = TokenType.TypeDef;
							break;

						case 1 :
							tok = TokenType.TypeRef;
							break;

						case 2 :
							tok = TokenType.ModuleRef;
							break;

						case 3 :
							tok = TokenType.MethodDef;
							break;

						case 4 :
							tok = TokenType.TypeSpec;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for MemberRefParent, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.HasSemantics :
					tag = data & 0x01;
					rid = (int) ((uint) data >> 1);
					switch (tag) {

						case 0 :
							tok = TokenType.Event;
							break;

						case 1 :
							tok = TokenType.Property;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for HasSemantics, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.MethodDefOrRef :
					tag = data & 0x01;
					rid = (int) ((uint) data >> 1);
					switch (tag) {

						case 0 :
							tok = TokenType.MethodDef;
							break;

						case 1 :
							tok = TokenType.MemberRef;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for MethodDefOrRef, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.MemberForwarded :
					tag = data & 0x01;
					rid = (int) ((uint) data >> 1);
					switch (tag) {

						case 0 :
							tok = TokenType.FieldDef;
							break;

						case 1 :
							tok = TokenType.MethodDef;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for MemberForwarded, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.Implementation :
					tag = data & 0x03;
					rid = (int) ((uint) data >> 2);
					switch (tag) {

						case 0 :
							tok = TokenType.File;
							break;

						case 1 :
							tok = TokenType.AssemblyRef;
							break;

						case 2 :
							tok = TokenType.ExportedType;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for Implementation, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.CustomAttributeType :
					tag = data & 0x07;
					rid = (int) ((uint) data >> 3);
					switch (tag) {

						case 0 :
							tok = TokenType.TypeRef;
							break;

						case 1 :
							tok = TokenType.TypeDef;
							break;

						case 2 :
							tok = TokenType.MethodDef;
							break;

						case 3 :
							tok = TokenType.MemberRef;
							break;

						case 4 :
							tok = TokenType.String;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for CustomAttributeType, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				case CodedTokenId.ResolutionScope :
					tag = data & 0x03;
					rid = (int) ((uint) data >> 2);
					switch (tag) {

						case 0 :
							tok = TokenType.Module;
							break;

						case 1 :
							tok = TokenType.ModuleRef;
							break;

						case 2 :
							tok = TokenType.AssemblyRef;
							break;

						case 3 :
							tok = TokenType.TypeRef;
							break;

						default :
							throw new BadMetaDataException("Invalid coded token for ResolutionScope, unknown table tag - " + tag);
					}
					res = new MDToken(tok, rid);
					break;

				default:
					break;
			}
			return res;
		}


		private static int GetCodedIndexSize(TablesHeap heap, CodedTokenId id, int [] rows)
		{
			int res = 0;

			switch (id) {

				case CodedTokenId.TypeDefOrRef :
					res = MDUtils.Max(rows [(int) TableId.TypeDef], rows [(int) TableId.TypeRef], rows [(int) TableId.TypeSpec]);
					res = res < (1 << (16 - 2)) ? 2 : 4;
					break;

				case CodedTokenId.HasConstant :
					res = MDUtils.Max(rows [(int) TableId.Field], rows [(int) TableId.Param], rows [(int) TableId.Property]);
					res = res < (1 << (16 - 2)) ? 2 : 4;
					break;

				case CodedTokenId.HasCustomAttribute :
					res = MDUtils.Max(rows [(int) TableId.Method], rows [(int) TableId.Field], rows [(int) TableId.TypeRef], rows [(int) TableId.TypeDef], rows [(int) TableId.Param], rows [(int) TableId.InterfaceImpl], rows [(int) TableId.MemberRef], rows [(int) TableId.Module], rows [(int) TableId.DeclSecurity], rows [(int) TableId.Property], rows [(int) TableId.Event], rows [(int) TableId.StandAloneSig], rows [(int) TableId.ModuleRef], rows [(int) TableId.TypeSpec], rows [(int) TableId.Assembly], rows [(int) TableId.AssemblyRef], rows [(int) TableId.File], rows [(int) TableId.ExportedType], rows [(int) TableId.ManifestResource]);
					res = res < (1 << (16 - 5)) ? 2 : 4;
					break;

				case CodedTokenId.HasFieldMarshal :
					res = MDUtils.Max(rows [(int) TableId.Field], rows [(int) TableId.Param]);
					res = res < (1 << (16 - 1)) ? 2 : 4;
					break;

				case CodedTokenId.HasDeclSecurity :
					res = MDUtils.Max(rows [(int) TableId.TypeDef], rows [(int) TableId.Method], rows [(int) TableId.Assembly]);
					res = res < (1 << (16 - 2)) ? 2 : 4;
					break;

				case CodedTokenId.MemberRefParent :
					res = MDUtils.Max(rows [(int) TableId.TypeDef], rows [(int) TableId.TypeRef], rows [(int) TableId.ModuleRef], rows [(int) TableId.Method], rows [(int) TableId.TypeSpec]);
					res = res < (1 << (16 - 3)) ? 2 : 4;
					break;

				case CodedTokenId.HasSemantics :
					res = MDUtils.Max(rows [(int) TableId.Event], rows [(int) TableId.Property]);
					res = res < (1 << (16 - 1)) ? 2 : 4;
					break;

				case CodedTokenId.MethodDefOrRef :
					res = MDUtils.Max(rows [(int) TableId.Method], rows [(int) TableId.MemberRef]);
					res = res < (1 << (16 - 1)) ? 2 : 4;
					break;

				case CodedTokenId.MemberForwarded :
					res = MDUtils.Max(rows [(int) TableId.Field], rows [(int) TableId.Method]);
					res = res < (1 << (16 - 1)) ? 2 : 4;
					break;

				case CodedTokenId.Implementation :
					res = MDUtils.Max(rows [(int) TableId.File], rows [(int) TableId.AssemblyRef], rows [(int) TableId.ExportedType]);
					res = res < (1 << (16 - 2)) ? 2 : 4;
					break;

				case CodedTokenId.CustomAttributeType :
					res = MDUtils.Max(rows [(int) TableId.TypeRef], rows [(int) TableId.TypeDef], rows [(int) TableId.Method], rows [(int) TableId.MemberRef], (heap.StringsIndexSize > 2 ? 1 << 17 : 1));
					res = res < (1 << (16 - 3)) ? 2 : 4;
					break;

				case CodedTokenId.ResolutionScope :
					res = MDUtils.Max(rows [(int) TableId.Module], rows [(int) TableId.ModuleRef], rows [(int) TableId.AssemblyRef], rows [(int) TableId.TypeRef]);
					res = res < (1 << (16 - 2)) ? 2 : 4;
					break;

				default:
					break;
			}

			return res;
		}


		private static int GetIndexSize(TableId tab, int [] rows)
		{
			// Index is 2 bytes wide if table has less than 2^16 rows
			// otherwise it's 4 bytes wide.
			return ((uint) rows [(int) tab]) < (1 << 16) ? 2 : 4;
		}


		private static void AllocBuff(ref byte [] buff, int size)
		{
			if (buff == null || buff.Length < size) {
				buff = new byte [(size + 4) & ~3];
			}
			Array.Clear(buff, 0, size);
		}


		/// <summary>
		/// </summary>
		unsafe public static int DecodePhysicalTables(TablesHeap heap, byte [] data, int offs, int [] rows)
		{
			int rowSize; // expanded row size (all indices are dwords)
			int fldSize; // physical field size
			int dest;
			int nRows;
			byte [] buff = null;
			int si = heap.StringsIndexSize;
			int gi = heap.GUIDIndexSize;
			int bi = heap.BlobIndexSize;

			if (heap.HasModule) {
				rowSize = sizeof (ushort) + 4 + 4 + 4 + 4;
				nRows = rows [(int) TableId.Module];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ModuleTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Generation, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Mvid, index(#GUID)
					fldSize = gi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// EncId, index(#GUID)
					fldSize = gi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// EncBaseId, index(#GUID)
					fldSize = gi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasTypeRef) {
				rowSize = 4 + 4 + 4;
				nRows = rows [(int) TableId.TypeRef];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new TypeRefTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// ResolutionScope, coded-index(ResolutionScope)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.ResolutionScope, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Namespace, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasTypeDef) {
				rowSize = sizeof (uint) + 4 + 4 + 4 + 4 + 4;
				nRows = rows [(int) TableId.TypeDef];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new TypeDefTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Flags, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Namespace, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Extends, coded-index(TypeDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.TypeDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// FieldList, index(Field)
					fldSize = GetIndexSize(TableId.Field, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// MethodList, index(Method)
					fldSize = GetIndexSize(TableId.Method, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasFieldPtr) {
				rowSize = 4;
				nRows = rows [(int) TableId.FieldPtr];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new FieldPtrTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Field, index(Field)
					fldSize = GetIndexSize(TableId.Field, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasField) {
				rowSize = sizeof (ushort) + 4 + 4;
				nRows = rows [(int) TableId.Field];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new FieldTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Flags, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Signature, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasMethodPtr) {
				rowSize = 4;
				nRows = rows [(int) TableId.MethodPtr];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new MethodPtrTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Method, index(Method)
					fldSize = GetIndexSize(TableId.Method, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasMethod) {
				rowSize = RVA.Size + sizeof (ushort) + sizeof (ushort) + 4 + 4 + 4;
				nRows = rows [(int) TableId.Method];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new MethodTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// RVA, RVA
					fldSize = RVA.Size;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += RVA.Size;
	
					// ImplFlags, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Flags, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Signature, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// ParamList, index(Param)
					fldSize = GetIndexSize(TableId.Param, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasParamPtr) {
				rowSize = 4;
				nRows = rows [(int) TableId.ParamPtr];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ParamPtrTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Param, index(Param)
					fldSize = GetIndexSize(TableId.Param, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasParam) {
				rowSize = sizeof (ushort) + sizeof (ushort) + 4;
				nRows = rows [(int) TableId.Param];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ParamTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Flags, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Sequence, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasInterfaceImpl) {
				rowSize = 4 + 4;
				nRows = rows [(int) TableId.InterfaceImpl];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new InterfaceImplTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Class, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Interface, coded-index(TypeDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.TypeDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasMemberRef) {
				rowSize = 4 + 4 + 4;
				nRows = rows [(int) TableId.MemberRef];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new MemberRefTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Class, coded-index(MemberRefParent)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.MemberRefParent, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Signature, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasConstant) {
				rowSize = sizeof (short) + 4 + 4;
				nRows = rows [(int) TableId.Constant];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ConstantTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Type, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// Parent, coded-index(HasConstant)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.HasConstant, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Value, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasCustomAttribute) {
				rowSize = 4 + 4 + 4;
				nRows = rows [(int) TableId.CustomAttribute];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new CustomAttributeTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Parent, coded-index(HasCustomAttribute)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.HasCustomAttribute, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Type, coded-index(CustomAttributeType)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.CustomAttributeType, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Value, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasFieldMarshal) {
				rowSize = 4 + 4;
				nRows = rows [(int) TableId.FieldMarshal];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new FieldMarshalTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Parent, coded-index(HasFieldMarshal)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.HasFieldMarshal, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// NativeType, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasDeclSecurity) {
				rowSize = sizeof (short) + 4 + 4;
				nRows = rows [(int) TableId.DeclSecurity];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new DeclSecurityTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Action, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// Parent, coded-index(HasDeclSecurity)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.HasDeclSecurity, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// PermissionSet, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasClassLayout) {
				rowSize = sizeof (short) + sizeof (int) + 4;
				nRows = rows [(int) TableId.ClassLayout];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ClassLayoutTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// PackingSize, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// ClassSize, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// Parent, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasFieldLayout) {
				rowSize = sizeof (int) + 4;
				nRows = rows [(int) TableId.FieldLayout];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new FieldLayoutTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Offset, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// Field, index(Field)
					fldSize = GetIndexSize(TableId.Field, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasStandAloneSig) {
				rowSize = 4;
				nRows = rows [(int) TableId.StandAloneSig];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new StandAloneSigTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Signature, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasEventMap) {
				rowSize = 4 + 4;
				nRows = rows [(int) TableId.EventMap];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new EventMapTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Parent, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// EventList, index(Event)
					fldSize = GetIndexSize(TableId.Event, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasEventPtr) {
				rowSize = 4;
				nRows = rows [(int) TableId.EventPtr];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new EventPtrTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Event, index(Event)
					fldSize = GetIndexSize(TableId.Event, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasEvent) {
				rowSize = sizeof (short) + 4 + 4;
				nRows = rows [(int) TableId.Event];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new EventTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// EventFlags, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// EventType, coded-index(TypeDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.TypeDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasPropertyMap) {
				rowSize = 4 + 4;
				nRows = rows [(int) TableId.PropertyMap];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new PropertyMapTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Parent, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// PropertyList, index(Property)
					fldSize = GetIndexSize(TableId.Property, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasPropertyPtr) {
				rowSize = 4;
				nRows = rows [(int) TableId.PropertyPtr];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new PropertyPtrTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Property, index(Property)
					fldSize = GetIndexSize(TableId.Property, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasProperty) {
				rowSize = sizeof (ushort) + 4 + 4;
				nRows = rows [(int) TableId.Property];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new PropertyTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Flags, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Type, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasMethodSemantics) {
				rowSize = sizeof (ushort) + 4 + 4;
				nRows = rows [(int) TableId.MethodSemantics];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new MethodSemanticsTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Semantics, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Method, index(Method)
					fldSize = GetIndexSize(TableId.Method, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Association, coded-index(HasSemantics)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.HasSemantics, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasMethodImpl) {
				rowSize = 4 + 4 + 4;
				nRows = rows [(int) TableId.MethodImpl];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new MethodImplTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Class, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// MethodBody, coded-index(MethodDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.MethodDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// MethodDeclaration, coded-index(MethodDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.MethodDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasModuleRef) {
				rowSize = 4;
				nRows = rows [(int) TableId.ModuleRef];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ModuleRefTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasTypeSpec) {
				rowSize = 4;
				nRows = rows [(int) TableId.TypeSpec];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new TypeSpecTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Signature, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasImplMap) {
				rowSize = sizeof (ushort) + 4 + 4 + 4;
				nRows = rows [(int) TableId.ImplMap];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ImplMapTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// MappingFlags, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// MemberForwarded, coded-index(MemberForwarded)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.MemberForwarded, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// ImportName, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// ImportScope, index(ModuleRef)
					fldSize = GetIndexSize(TableId.ModuleRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasFieldRVA) {
				rowSize = RVA.Size + 4;
				nRows = rows [(int) TableId.FieldRVA];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new FieldRVATable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// RVA, RVA
					fldSize = RVA.Size;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += RVA.Size;
	
					// Field, index(Field)
					fldSize = GetIndexSize(TableId.Field, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasENCLog) {
				rowSize = sizeof (uint) + sizeof (uint);
				nRows = rows [(int) TableId.ENCLog];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ENCLogTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Token, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// FuncCode, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasENCMap) {
				rowSize = sizeof (uint);
				nRows = rows [(int) TableId.ENCMap];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ENCMapTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Token, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasAssembly) {
				rowSize = sizeof (int) + sizeof (short) + sizeof (short) + sizeof (short) + sizeof (short) + sizeof (uint) + 4 + 4 + 4;
				nRows = rows [(int) TableId.Assembly];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new AssemblyTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// HashAlgId, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// MajorVersion, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// MinorVersion, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// BuildNumber, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// RevisionNumber, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// Flags, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// PublicKey, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Culture, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasAssemblyProcessor) {
				rowSize = sizeof (int);
				nRows = rows [(int) TableId.AssemblyProcessor];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new AssemblyProcessorTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Processor, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasAssemblyOS) {
				rowSize = sizeof (int) + sizeof (int) + sizeof (int);
				nRows = rows [(int) TableId.AssemblyOS];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new AssemblyOSTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// OSPlatformID, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// OSMajorVersion, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// OSMinorVersion, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasAssemblyRef) {
				rowSize = sizeof (short) + sizeof (short) + sizeof (short) + sizeof (short) + sizeof (uint) + 4 + 4 + 4 + 4;
				nRows = rows [(int) TableId.AssemblyRef];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new AssemblyRefTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// MajorVersion, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// MinorVersion, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// BuildNumber, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// RevisionNumber, short
					fldSize = sizeof (short);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (short);
	
					// Flags, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// PublicKeyOrToken, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Culture, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// HashValue, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasAssemblyRefProcessor) {
				rowSize = sizeof (int) + 4;
				nRows = rows [(int) TableId.AssemblyRefProcessor];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new AssemblyRefProcessorTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Processor, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// AssemblyRef, index(AssemblyRef)
					fldSize = GetIndexSize(TableId.AssemblyRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasAssemblyRefOS) {
				rowSize = sizeof (int) + sizeof (int) + sizeof (int) + 4;
				nRows = rows [(int) TableId.AssemblyRefOS];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new AssemblyRefOSTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// OSPlatformID, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// OSMajorVersion, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// OSMinorVersion, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// AssemblyRef, index(AssemblyRef)
					fldSize = GetIndexSize(TableId.AssemblyRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasFile) {
				rowSize = sizeof (uint) + 4 + 4;
				nRows = rows [(int) TableId.File];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new FileTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Flags, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// HashValue, index(#Blob)
					fldSize = bi;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasExportedType) {
				rowSize = sizeof (uint) + 4 + 4 + 4 + 4;
				nRows = rows [(int) TableId.ExportedType];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ExportedTypeTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Flags, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// TypeDefId, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// TypeName, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// TypeNamespace, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Implementation, coded-index(Implementation)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.Implementation, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasManifestResource) {
				rowSize = sizeof (int) + sizeof (uint) + 4 + 4;
				nRows = rows [(int) TableId.ManifestResource];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new ManifestResourceTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Offset, int
					fldSize = sizeof (int);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (int);
	
					// Flags, uint
					fldSize = sizeof (uint);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (uint);
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Implementation, coded-index(Implementation)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.Implementation, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasNestedClass) {
				rowSize = 4 + 4;
				nRows = rows [(int) TableId.NestedClass];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new NestedClassTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// NestedClass, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// EnclosingClass, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasTypeTyPar) {
				rowSize = sizeof (ushort) + 4 + 4 + 4;
				nRows = rows [(int) TableId.TypeTyPar];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new TypeTyParTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Number, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Class, index(TypeDef)
					fldSize = GetIndexSize(TableId.TypeDef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Bound, coded-index(TypeDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.TypeDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			if (heap.HasMethodTyPar) {
				rowSize = sizeof (ushort) + 4 + 4 + 4;
				nRows = rows [(int) TableId.MethodTyPar];
				AllocBuff(ref buff, rowSize * nRows);
				dest = 0;

				MDTable tab = new MethodTyParTable(heap);

				for (int i = nRows; --i >= 0;) {
	
					// Number, ushort
					fldSize = sizeof (ushort);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += sizeof (ushort);
	
					// Method, index(Method)
					fldSize = GetIndexSize(TableId.Method, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Bound, coded-index(TypeDefOrRef)
					fldSize = GetCodedIndexSize(heap, CodedTokenId.TypeDefOrRef, rows);
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
					// Name, index(#Strings)
					fldSize = si;
					Array.Copy(data, offs, buff, dest, fldSize);
					offs += fldSize;
					dest += 4;
	
				}

				tab.FromRawData(buff, 0, nRows);
			}

			return offs;
		}

	} // end class
} // end namespace

