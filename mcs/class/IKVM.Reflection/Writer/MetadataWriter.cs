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
using System.Collections.Generic;
using System.Text;
using IKVM.Reflection.Emit;
using IKVM.Reflection.Metadata;

namespace IKVM.Reflection.Writer
{
	sealed class MetadataWriter : MetadataRW
	{
		private readonly ModuleBuilder moduleBuilder;
		private readonly Stream stream;
		private readonly byte[] buffer = new byte[8];

		internal MetadataWriter(ModuleBuilder module, Stream stream)
			: base(module, module.Strings.IsBig, module.Guids.IsBig, module.Blobs.IsBig)
		{
			this.moduleBuilder = module;
			this.stream = stream;
		}

		internal ModuleBuilder ModuleBuilder
		{
			get { return moduleBuilder; }
		}

		internal int Position
		{
			get { return (int)stream.Position; }
		}

		internal void Write(ByteBuffer bb)
		{
			bb.WriteTo(stream);
		}

		internal void Write(byte[] value)
		{
			stream.Write(value, 0, value.Length);
		}

		internal void Write(byte value)
		{
			stream.WriteByte(value);
		}

		internal void Write(ushort value)
		{
			Write((short)value);
		}

		internal void Write(short value)
		{
			stream.WriteByte((byte)value);
			stream.WriteByte((byte)(value >> 8));
		}

		internal void Write(uint value)
		{
			Write((int)value);
		}

		internal void Write(int value)
		{
			buffer[0] = (byte)value;
			buffer[1] = (byte)(value >> 8);
			buffer[2] = (byte)(value >> 16);
			buffer[3] = (byte)(value >> 24);
			stream.Write(buffer, 0, 4);
		}

		internal void Write(ulong value)
		{
			Write((long)value);
		}

		internal void Write(long value)
		{
			buffer[0] = (byte)value;
			buffer[1] = (byte)(value >> 8);
			buffer[2] = (byte)(value >> 16);
			buffer[3] = (byte)(value >> 24);
			buffer[4] = (byte)(value >> 32);
			buffer[5] = (byte)(value >> 40);
			buffer[6] = (byte)(value >> 48);
			buffer[7] = (byte)(value >> 56);
			stream.Write(buffer, 0, 8);
		}

		internal void WriteCompressedInt(int value)
		{
			if (value <= 0x7F)
			{
				Write((byte)value);
			}
			else if (value <= 0x3FFF)
			{
				Write((byte)(0x80 | (value >> 8)));
				Write((byte)value);
			}
			else
			{
				Write((byte)(0xC0 | (value >> 24)));
				Write((byte)(value >> 16));
				Write((byte)(value >> 8));
				Write((byte)value);
			}
		}

		internal static int GetCompressedIntLength(int value)
		{
			if (value <= 0x7F)
			{
				return 1;
			}
			else if (value <= 0x3FFF)
			{
				return 2;
			}
			else
			{
				return 4;
			}
		}

		internal void WriteStringIndex(int index)
		{
			if (bigStrings)
			{
				Write(index);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteGuidIndex(int index)
		{
			if (bigGuids)
			{
				Write(index);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteBlobIndex(int index)
		{
			if (bigBlobs)
			{
				Write(index);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteTypeDefOrRef(int token)
		{
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
			if (bigTypeDefOrRef)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteEncodedTypeDefOrRef(int encodedToken)
		{
			if (bigTypeDefOrRef)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteHasCustomAttribute(int encodedToken)
		{
			// NOTE because we've already had to do the encoding (to be able to sort the table)
			// here we simple write the value
			if (bigHasCustomAttribute)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteCustomAttributeType(int token)
		{
			switch (token >> 24)
			{
				case MethodDefTable.Index:
					token = (token & 0xFFFFFF) << 3 | 2;
					break;
				case MemberRefTable.Index:
					token = (token & 0xFFFFFF) << 3 | 3;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (bigCustomAttributeType)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteField(int index)
		{
			if (bigField)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteMethodDef(int index)
		{
			if (bigMethodDef)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteParam(int index)
		{
			if (bigParam)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteTypeDef(int index)
		{
			if (bigTypeDef)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteEvent(int index)
		{
			if (bigEvent)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteProperty(int index)
		{
			if (bigProperty)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteGenericParam(int index)
		{
			if (bigGenericParam)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteModuleRef(int index)
		{
			if (bigModuleRef)
			{
				Write(index & 0xFFFFFF);
			}
			else
			{
				Write((short)index);
			}
		}

		internal void WriteResolutionScope(int token)
		{
			switch (token >> 24)
			{
				case ModuleTable.Index:
					token = (token & 0xFFFFFF) << 2 | 0;
					break;
				case ModuleRefTable.Index:
					token = (token & 0xFFFFFF) << 2 | 1;
					break;
				case AssemblyRefTable.Index:
					token = (token & 0xFFFFFF) << 2 | 2;
					break;
				case TypeRefTable.Index:
					token = (token & 0xFFFFFF) << 2 | 3;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (bigResolutionScope)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteMemberRefParent(int token)
		{
			switch (token >> 24)
			{
				case TypeDefTable.Index:
					token = (token & 0xFFFFFF) << 3 | 0;
					break;
				case TypeRefTable.Index:
					token = (token & 0xFFFFFF) << 3 | 1;
					break;
				case ModuleRefTable.Index:
					token = (token & 0xFFFFFF) << 3 | 2;
					break;
				case MethodDefTable.Index:
					token = (token & 0xFFFFFF) << 3 | 3;
					break;
				case TypeSpecTable.Index:
					token = (token & 0xFFFFFF) << 3 | 4;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (bigMemberRefParent)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteMethodDefOrRef(int token)
		{
			switch (token >> 24)
			{
				case MethodDefTable.Index:
					token = (token & 0xFFFFFF) << 1 | 0;
					break;
				case MemberRefTable.Index:
					token = (token & 0xFFFFFF) << 1 | 1;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (bigMethodDefOrRef)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteHasConstant(int encodedToken)
		{
			// NOTE because we've already had to do the encoding (to be able to sort the table)
			// here we simple write the value
			if (bigHasConstant)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteHasSemantics(int encodedToken)
		{
			// NOTE because we've already had to do the encoding (to be able to sort the table)
			// here we simple write the value
			if (bigHasSemantics)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteImplementation(int token)
		{
			switch (token >> 24)
			{
				case 0:
					break;
				case FileTable.Index:
					token = (token & 0xFFFFFF) << 2 | 0;
					break;
				case AssemblyRefTable.Index:
					token = (token & 0xFFFFFF) << 2 | 1;
					break;
				case ExportedTypeTable.Index:
					token = (token & 0xFFFFFF) << 2 | 2;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (bigImplementation)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteTypeOrMethodDef(int encodedToken)
		{
			// NOTE because we've already had to do the encoding (to be able to sort the table)
			// here we simple write the value
			if (bigTypeOrMethodDef)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteHasDeclSecurity(int encodedToken)
		{
			// NOTE because we've already had to do the encoding (to be able to sort the table)
			// here we simple write the value
			if (bigHasDeclSecurity)
			{
				Write(encodedToken);
			}
			else
			{
				Write((short)encodedToken);
			}
		}

		internal void WriteMemberForwarded(int token)
		{
			switch (token >> 24)
			{
				case FieldTable.Index:
					token = (token & 0xFFFFFF) << 1 | 0;
				    break;
				case MethodDefTable.Index:
					token = (token & 0xFFFFFF) << 1 | 1;
					break;
				default:
					throw new InvalidOperationException();
			}
			if (bigMemberForwarded)
			{
				Write(token);
			}
			else
			{
				Write((short)token);
			}
		}

		internal void WriteHasFieldMarshal(int encodedToken)
		{
			// NOTE because we've already had to do the encoding (to be able to sort the table)
			// here we simple write the value
			if (bigHasFieldMarshal)
			{
				Write(encodedToken & 0xFFFFFF);
			}
			else
			{
				Write((short)encodedToken);
			}
		}
	}
}
