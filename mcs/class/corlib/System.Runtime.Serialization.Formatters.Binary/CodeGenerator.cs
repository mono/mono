// CodeGenerator.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2004 Novell, Inc

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !FULL_AOT_RUNTIME
using System;
using System.IO;
using System.Collections;
using System.Runtime.Serialization;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

namespace System.Runtime.Serialization.Formatters.Binary
{
	internal class CodeGenerator
	{
		// Code generation

		static object monitor = new object ();
		
		static ModuleBuilder _module;
		
		static CodeGenerator ()
		{
			AppDomain myDomain = System.Threading.Thread.GetDomain();
			AssemblyName myAsmName = new AssemblyName();
			myAsmName.Name = "__MetadataTypes";
		   
			AssemblyBuilder myAsmBuilder = myDomain.DefineInternalDynamicAssembly (myAsmName, AssemblyBuilderAccess.Run);
			_module = myAsmBuilder.DefineDynamicModule("__MetadataTypesModule", false);
		}
		
		static public Type GenerateMetadataType (Type type, StreamingContext context) {
			/* SRE is not thread safe */
			lock (monitor) {
				return GenerateMetadataTypeInternal (type, context);
			}
		}

		static public Type GenerateMetadataTypeInternal (Type type, StreamingContext context)
		{		
			string name = type.Name + "__TypeMetadata";
			string sufix = "";
			int n = 0;
			while (_module.GetType (name + sufix) != null)
				sufix = (++n).ToString();
				
			name += sufix;
				
			MemberInfo[] members = FormatterServices.GetSerializableMembers (type, context);
			
			TypeBuilder typeBuilder = _module.DefineType (name, TypeAttributes.Public, typeof(ClrTypeMetadata));

			Type[] parameters;
			MethodBuilder method;
			ILGenerator gen;
			
			// *********************
			// 	METHOD public constructor (Type t): base (t);
			
			parameters = Type.EmptyTypes;

			ConstructorBuilder ctor = typeBuilder.DefineConstructor (MethodAttributes.Public, CallingConventions.Standard, parameters);
			ConstructorInfo baseCtor = typeof(ClrTypeMetadata).GetConstructor (new Type[] { typeof(Type) });
			gen = ctor.GetILGenerator();

			gen.Emit (OpCodes.Ldarg_0);
			gen.Emit (OpCodes.Ldtoken, type);
			gen.EmitCall (OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
			gen.Emit (OpCodes.Call, baseCtor);
			gen.Emit (OpCodes.Ret);

			// *********************
			// 	METHOD public override void WriteAssemblies (ObjectWriter ow, BinaryWriter writer);

			parameters = new Type[] { typeof(ObjectWriter), typeof(BinaryWriter) };
			method = typeBuilder.DefineMethod ("WriteAssemblies", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), parameters);			
			gen = method.GetILGenerator();
			
			foreach (FieldInfo field in members)
			{
				Type memberType = field.FieldType;
				while (memberType.IsArray) 
					memberType = memberType.GetElementType();

				if (memberType.Assembly != ObjectWriter.CorlibAssembly)
				{
					// EMIT ow.WriteAssembly (writer, memberType.Assembly);
					gen.Emit (OpCodes.Ldarg_1);
					gen.Emit (OpCodes.Ldarg_2);
					EmitLoadTypeAssembly (gen, memberType, field.Name);
					gen.EmitCall (OpCodes.Callvirt, typeof(ObjectWriter).GetMethod("WriteAssembly"), null);
					gen.Emit (OpCodes.Pop);
				}
			}
			gen.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride (method, typeof(TypeMetadata).GetMethod ("WriteAssemblies"));
			
			// *********************
			// METHOD public override void WriteTypeData (ObjectWriter ow, BinaryWriter writer, bool writeTypes);
			
			parameters = new Type[] { typeof(ObjectWriter), typeof(BinaryWriter), typeof(bool) };
			method = typeBuilder.DefineMethod ("WriteTypeData", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), parameters);
			gen = method.GetILGenerator();
			
			// EMIT writer.Write (members.Length);
			gen.Emit (OpCodes.Ldarg_2);
			gen.Emit (OpCodes.Ldc_I4, members.Length);
			EmitWrite (gen, typeof(int));
			
			// Names of fields
			foreach (FieldInfo field in members)
			{
				// EMIT writer.Write (name);
				gen.Emit (OpCodes.Ldarg_2);
				gen.Emit (OpCodes.Ldstr, field.Name);
				EmitWrite (gen, typeof(string));
			}

			Label falseLabel = gen.DefineLabel ();
			gen.Emit (OpCodes.Ldarg_3);
			gen.Emit (OpCodes.Brfalse, falseLabel);
					
			// Types of fields
			foreach (FieldInfo field in members)
			{
				// EMIT writer.Write ((byte) ObjectWriter.GetTypeTag (type));
				gen.Emit (OpCodes.Ldarg_2);
				gen.Emit (OpCodes.Ldc_I4_S, (byte) ObjectWriter.GetTypeTag (field.FieldType));
				EmitWrite (gen, typeof(byte));
			}

			// Type specs of fields
			foreach (FieldInfo field in members)
			{
				// EMIT ow.WriteTypeSpec (writer, field.FieldType);
				EmitWriteTypeSpec (gen, field.FieldType, field.Name);
			}
			gen.MarkLabel(falseLabel);
			
			gen.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride (method, typeof(TypeMetadata).GetMethod ("WriteTypeData"));
			
			// *********************
			// METHOD public override void WriteObjectData (ObjectWriter ow, BinaryWriter writer, object data)
			
			parameters = new Type[] { typeof(ObjectWriter), typeof(BinaryWriter), typeof(object) };
			method = typeBuilder.DefineMethod ("WriteObjectData", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), parameters);			
			gen = method.GetILGenerator();
			
			LocalBuilder localBuilder = gen.DeclareLocal (type);
			OpCode lload = OpCodes.Ldloc;
			
			gen.Emit (OpCodes.Ldarg_3);
			if (type.IsValueType)
			{
				gen.Emit (OpCodes.Unbox, type);
				LoadFromPtr (gen, type);
				lload = OpCodes.Ldloca_S;
			}
			else
				gen.Emit (OpCodes.Castclass, type);
				
			gen.Emit (OpCodes.Stloc, localBuilder);
			
			foreach (FieldInfo field in members)
			{
				// EMIT ow.WriteValue (writer, ((FieldInfo)members[n]).FieldType, values[n]);
				Type ftype = field.FieldType;
				if (BinaryCommon.IsPrimitive (ftype))
				{
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (lload, localBuilder);
					if (ftype == typeof(DateTime) || ftype == typeof(TimeSpan) || ftype == typeof(decimal))
						gen.Emit (OpCodes.Ldflda, field);
					else
						gen.Emit (OpCodes.Ldfld, field);
					EmitWritePrimitiveValue (gen, ftype);
				}
				else
				{
					gen.Emit (OpCodes.Ldarg_1);
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (OpCodes.Ldtoken, ftype);
					gen.EmitCall (OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
					gen.Emit (lload, localBuilder);
					gen.Emit (OpCodes.Ldfld, field);
					if (ftype.IsValueType)
						gen.Emit (OpCodes.Box, ftype);
					gen.EmitCall (OpCodes.Call, typeof(ObjectWriter).GetMethod("WriteValue"), null);
				}
			}
			
			gen.Emit(OpCodes.Ret);
			typeBuilder.DefineMethodOverride (method, typeof(TypeMetadata).GetMethod ("WriteObjectData"));
			
        	return typeBuilder.CreateType();
		}
		
		public static void LoadFromPtr (ILGenerator ig, Type t)
		{
			if (t == typeof(int))
				ig.Emit (OpCodes.Ldind_I4);
			else if (t == typeof(uint))
				ig.Emit (OpCodes.Ldind_U4);
			else if (t == typeof(short))
				ig.Emit (OpCodes.Ldind_I2);
			else if (t == typeof(ushort))
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == typeof(char))
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == typeof(byte))
				ig.Emit (OpCodes.Ldind_U1);
			else if (t == typeof(sbyte))
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == typeof(ulong))
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == typeof(long))
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == typeof(float))
				ig.Emit (OpCodes.Ldind_R4);
			else if (t == typeof(double))
				ig.Emit (OpCodes.Ldind_R8);
			else if (t == typeof(bool))
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == typeof(IntPtr))
				ig.Emit (OpCodes.Ldind_I);
			else if (t.IsEnum) {
				if (t == typeof(Enum))
					ig.Emit (OpCodes.Ldind_Ref);
				else
					LoadFromPtr (ig, EnumToUnderlying (t));
			} else if (t.IsValueType)
				ig.Emit (OpCodes.Ldobj, t);
			else
				ig.Emit (OpCodes.Ldind_Ref);
		}

		static void EmitWriteTypeSpec (ILGenerator gen, Type type, string member)
		{
			// WARNING Keep in sync with WriteTypeSpec
			
			switch (ObjectWriter.GetTypeTag (type))
			{
				case TypeTag.PrimitiveType:
					// EMIT writer.Write (BinaryCommon.GetTypeCode (type));
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (OpCodes.Ldc_I4_S, (byte) BinaryCommon.GetTypeCode (type));
					EmitWrite (gen, typeof(byte));
					break;

				case TypeTag.RuntimeType:
					// EMIT writer.Write (type.FullName);
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (OpCodes.Ldstr, type.FullName);
					EmitWrite (gen, typeof(string));
					break;

				case TypeTag.GenericType:
					// EMIT writer.Write (type.FullName);
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (OpCodes.Ldstr, type.FullName);
					EmitWrite (gen, typeof(string));
					
					// EMIT writer.Write ((int)ow.GetAssemblyId (type.Assembly));
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (OpCodes.Ldarg_1);
					EmitLoadTypeAssembly (gen, type, member);
					gen.EmitCall (OpCodes.Callvirt, typeof(ObjectWriter).GetMethod("GetAssemblyId"), null);
					gen.Emit (OpCodes.Conv_I4);
					EmitWrite (gen, typeof(int));
					break;

				case TypeTag.ArrayOfPrimitiveType:
					// EMIT writer.Write (BinaryCommon.GetTypeCode (type.GetElementType()));
					gen.Emit (OpCodes.Ldarg_2);
					gen.Emit (OpCodes.Ldc_I4_S, (byte) BinaryCommon.GetTypeCode (type.GetElementType()));
					EmitWrite (gen, typeof(byte));
					break;

				default:
					// Type spec not needed
					break;
			}
		}
		
		static void EmitLoadTypeAssembly (ILGenerator gen, Type type, string member)
		{
			gen.Emit (OpCodes.Ldtoken, type);
			gen.EmitCall (OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);
			gen.EmitCall (OpCodes.Callvirt, typeof(Type).GetProperty("Assembly").GetGetMethod(), null);
		}
		
		static void EmitWrite (ILGenerator gen, Type type)
		{
			gen.EmitCall (OpCodes.Callvirt, typeof(BinaryWriter).GetMethod("Write", new Type[] { type }), null);
		}
		
		public static void EmitWritePrimitiveValue (ILGenerator gen, Type type)
		{
			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean:
				case TypeCode.Byte:
				case TypeCode.Char:
				case TypeCode.Double:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.SByte:
				case TypeCode.Single:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.String:
					EmitWrite (gen, type);
					break;

				case TypeCode.Decimal:
					// writer.Write (val.ToString (CultureInfo.InvariantCulture));
					gen.EmitCall (OpCodes.Call, typeof(CultureInfo).GetProperty("InvariantCulture").GetGetMethod(), null);
					gen.EmitCall (OpCodes.Call, typeof(Decimal).GetMethod("ToString", new Type[] {typeof(IFormatProvider)}), null);
					EmitWrite (gen, typeof(string));
					break;
					
				case TypeCode.DateTime: 
					gen.EmitCall (OpCodes.Call, typeof(DateTime).GetMethod("ToBinary", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance), null);
					EmitWrite (gen, typeof(long));
					break;
					
				default:
					if (type == typeof (TimeSpan)) {
						gen.EmitCall (OpCodes.Call, typeof(TimeSpan).GetProperty("Ticks").GetGetMethod(), null);
						EmitWrite (gen, typeof(long));
					}
					else
						throw new NotSupportedException ("Unsupported primitive type: " + type.FullName);
					break;
			}
		}
		
		//
		// This is needed, because enumerations from assemblies
		// do not report their underlyingtype, but they report
		// themselves
		//
		public static Type EnumToUnderlying (Type t)
		{
			TypeCode tc = Type.GetTypeCode (t);
	
			switch (tc){
			case TypeCode.Boolean:
				return typeof (bool);
			case TypeCode.Byte:
				return typeof (byte);
			case TypeCode.SByte:
				return typeof (sbyte);
			case TypeCode.Char:
				return typeof (char);
			case TypeCode.Int16:
				return typeof (short);
			case TypeCode.UInt16:
				return typeof (ushort);
			case TypeCode.Int32:
				return typeof (int);
			case TypeCode.UInt32:
				return typeof (uint);
			case TypeCode.Int64:
				return typeof (long);
			case TypeCode.UInt64:
				return typeof (ulong);
			}
			throw new Exception ("Unhandled typecode in enum " + tc + " from " + t.AssemblyQualifiedName);
		}
	}
 }
 
#endif
