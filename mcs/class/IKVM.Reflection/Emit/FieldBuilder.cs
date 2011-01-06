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
using System.Runtime.CompilerServices;
using IKVM.Reflection.Metadata;
using IKVM.Reflection.Writer;

namespace IKVM.Reflection.Emit
{
	public sealed class FieldBuilder : FieldInfo
	{
		private readonly TypeBuilder typeBuilder;
		private readonly string name;
		private readonly int pseudoToken;
		private FieldAttributes attribs;
		private readonly int nameIndex;
		private readonly int signature;
		private readonly FieldSignature fieldSig;

		internal FieldBuilder(TypeBuilder type, string name, Type fieldType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers, FieldAttributes attribs)
		{
			this.typeBuilder = type;
			this.name = name;
			this.pseudoToken = type.ModuleBuilder.AllocPseudoToken();
			this.nameIndex = type.ModuleBuilder.Strings.Add(name);
			this.fieldSig = FieldSignature.Create(fieldType, optionalCustomModifiers, requiredCustomModifiers);
			ByteBuffer sig = new ByteBuffer(5);
			fieldSig.WriteSig(this.typeBuilder.ModuleBuilder, sig);
			this.signature = this.typeBuilder.ModuleBuilder.Blobs.Add(sig);
			this.attribs = attribs;
			this.typeBuilder.ModuleBuilder.Field.AddVirtualRecord();
		}

		public void SetConstant(object defaultValue)
		{
			attribs |= FieldAttributes.HasDefault;
			typeBuilder.ModuleBuilder.AddConstant(pseudoToken, defaultValue);
		}

		public override object GetRawConstantValue()
		{
			return typeBuilder.Module.Constant.GetRawConstantValue(typeBuilder.Module, this.MetadataToken);
		}

		public void __SetDataAndRVA(byte[] data)
		{
			attribs |= FieldAttributes.HasFieldRVA;
			FieldRVATable.Record rec = new FieldRVATable.Record();
			rec.RVA = typeBuilder.ModuleBuilder.initializedData.Position;
			rec.Field = pseudoToken;
			typeBuilder.ModuleBuilder.FieldRVA.AddRecord(rec);
			typeBuilder.ModuleBuilder.initializedData.Write(data);
		}

		public override void __GetDataFromRVA(byte[] data, int offset, int length)
		{
			throw new NotImplementedException();
		}

		public void SetCustomAttribute(ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute(new CustomAttributeBuilder(con, binaryAttribute));
		}

		public void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			Universe u = this.Module.universe;
			if (customBuilder.Constructor.DeclaringType == u.System_Runtime_InteropServices_FieldOffsetAttribute)
			{
				customBuilder = customBuilder.DecodeBlob(this.Module.Assembly);
				SetOffset((int)customBuilder.GetConstructorArgument(0));
			}
			else if (customBuilder.Constructor.DeclaringType == u.System_Runtime_InteropServices_MarshalAsAttribute)
			{
				MarshalSpec.SetMarshalAsAttribute(typeBuilder.ModuleBuilder, pseudoToken, customBuilder);
				attribs |= FieldAttributes.HasFieldMarshal;
			}
			else if (customBuilder.Constructor.DeclaringType == u.System_NonSerializedAttribute)
			{
				attribs |= FieldAttributes.NotSerialized;
			}
			else if (customBuilder.Constructor.DeclaringType == u.System_Runtime_CompilerServices_SpecialNameAttribute)
			{
				attribs |= FieldAttributes.SpecialName;
			}
			else
			{
				typeBuilder.ModuleBuilder.SetCustomAttribute(pseudoToken, customBuilder);
			}
		}

		public void SetOffset(int iOffset)
		{
			FieldLayoutTable.Record rec = new FieldLayoutTable.Record();
			rec.Offset = iOffset;
			rec.Field = pseudoToken;
			typeBuilder.ModuleBuilder.FieldLayout.AddRecord(rec);
		}

		public override FieldAttributes Attributes
		{
			get { return attribs; }
		}

		public override Type DeclaringType
		{
			get { return typeBuilder.IsModulePseudoType ? null : typeBuilder; }
		}

		public override string Name
		{
			get { return name; }
		}

		public override int MetadataToken
		{
			get { return pseudoToken; }
		}

		public override Module Module
		{
			get { return typeBuilder.Module; }
		}

		public FieldToken GetToken()
		{
			return new FieldToken(pseudoToken);
		}

		internal void WriteFieldRecords(MetadataWriter mw)
		{
			mw.Write((short)attribs);
			mw.WriteStringIndex(nameIndex);
			mw.WriteBlobIndex(signature);
		}

		internal void FixupToken(int token)
		{
			typeBuilder.ModuleBuilder.RegisterTokenFixup(this.pseudoToken, token);
		}

		internal override FieldSignature FieldSignature
		{
			get { return fieldSig; }
		}

		internal override int ImportTo(ModuleBuilder other)
		{
			if (typeBuilder.IsGenericTypeDefinition)
			{
				return other.ImportMember(TypeBuilder.GetField(typeBuilder, this));
			}
			else if (other == typeBuilder.ModuleBuilder)
			{
				return pseudoToken;
			}
			else
			{
				return other.ImportMethodOrField(typeBuilder, name, fieldSig);
			}
		}
	}
}
