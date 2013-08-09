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
// Copyright (C) Lluis Sanchez Gual, 2004
//

#if !FULL_AOT_RUNTIME
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeProperty
	{
		PropertyInfo propertyInfo;
		CodeBuilder get_builder;
		CodeBuilder set_builder;
		string name;
		PropertyAttributes attributes;
		MethodAttributes methodAttributes;
		Type returnType;
		TypeBuilder typeBuilder;
		Type[] parameterTypes;
		ArrayList customAttributes = new ArrayList ();
		CodeClass cls;
		
		internal static CodeProperty DefineProperty (CodeClass cls, string name, PropertyAttributes attributes, MethodAttributes methodAttributes, Type returnType, Type[] parameterTypes)
		{
			return new CodeProperty (cls, name, attributes, methodAttributes, returnType, parameterTypes);
		}
		
		internal CodeProperty (CodeClass cls, string name, PropertyAttributes attributes, MethodAttributes methodAttributes, Type returnType, Type[] parameterTypes) 
		{
			this.cls = cls;
			this.typeBuilder = cls.TypeBuilder;
			this.name = name;
			this.attributes = attributes;
			this.methodAttributes = methodAttributes;
			this.returnType = returnType;
			this.parameterTypes = parameterTypes;
		
			PropertyBuilder pb = typeBuilder.DefineProperty (name, attributes, returnType, parameterTypes);
			pb.SetGetMethod (typeBuilder.DefineMethod ("get_" + name, methodAttributes, CallingConventions.Standard, returnType, Type.EmptyTypes));
			pb.SetSetMethod (typeBuilder.DefineMethod ("set_" + name, methodAttributes, CallingConventions.Standard, typeof (void), new Type [] {returnType}));
			get_builder = new CodeBuilder (cls);
			set_builder = new CodeBuilder (cls);
			propertyInfo = pb;
		}
		
		public TypeBuilder DeclaringType
		{
			get { return typeBuilder; }
		}
		
		public PropertyBuilder PropertyBuilder
		{
			get { return propertyInfo as PropertyBuilder; }
		}
		
		public string Name
		{
			get { return name; }
		}
		
		public PropertyAttributes Attributes
		{
			get { return attributes; }
		}
		
		public Type ReturnType
		{
			get { return returnType; }
		}
		
		public Type[] ParameterTypes
		{
			get { return parameterTypes; }
		}
		
		public CodeBuilder CodeBuilderGet
		{
			get { return get_builder; }
		}

		public CodeBuilder CodeBuilderSet
		{
			get { return set_builder; }
		}

		public bool IsStatic
		{
			get { return (methodAttributes & MethodAttributes.Static) != 0; }
		}

		public bool IsPublic
		{
			get { return (methodAttributes & MethodAttributes.Public) != 0; }
		}

		public CodeCustomAttribute CreateCustomAttribute (Type attributeType)
		{
			return CreateCustomAttribute (attributeType,
				Type.EmptyTypes, new object [0]);
		}

		public CodeCustomAttribute CreateCustomAttribute (Type attributeType, Type [] ctorArgTypes, object [] ctorArgs)
		{
			return CreateCustomAttribute (attributeType,
				ctorArgTypes, ctorArgs, new string [0], new object [0]);
		}

		public CodeCustomAttribute CreateCustomAttribute (Type attributeType, Type [] ctorArgTypes, object [] ctorArgs, string [] namedArgFieldNames, object [] namedArgValues)
		{
			CodeCustomAttribute cca = CodeCustomAttribute.Create (
				attributeType, ctorArgTypes, ctorArgs, namedArgFieldNames, namedArgValues);
			SetCustomAttribute (cca);
			return cca;
		}

		public CodeCustomAttribute CreateCustomAttribute (Type attributeType, Type [] ctorArgTypes, CodeLiteral [] ctorArgs, FieldInfo [] fields, CodeLiteral [] fieldValues)
		{
			CodeCustomAttribute cca = CodeCustomAttribute.Create (
				attributeType, ctorArgTypes, ctorArgs, fields, fieldValues);
			SetCustomAttribute (cca);
			return cca;
		}

		void SetCustomAttribute (CodeCustomAttribute cca)
		{
			PropertyBuilder.SetCustomAttribute (cca.Builder);
			customAttributes.Add (cca);
		}

		public string PrintCode ()
		{
			StringWriter sw = new StringWriter ();
			CodeWriter cw = new CodeWriter (sw);
			PrintCode (cw);
			return sw.ToString ();
		}
		
		public void PrintCode (CodeWriter cp)
		{
			cp.BeginLine ();
			foreach (CodeCustomAttribute a in customAttributes)
				a.PrintCode (cp);
			cp.BeginLine ();
			if (IsStatic)
				cp.Write ("static ");
			if (IsPublic)
				cp.Write ("public ");
			if (returnType != null) cp.Write (returnType + " ");
			cp.Write (name);
			if (parameterTypes.Length > 0) {
				cp.Write (name + " [");
				for (int n=0; n<parameterTypes.Length; n++) {
					if (n > 0) cp.Write (", ");
					cp.Write (parameterTypes[n] + " arg" + n);
				}
				cp.Write ("]");
			}
			cp.Write (" {");
			cp.EndLine ();
			cp.Indent ();
			cp.WriteLineInd ("get {");
			get_builder.PrintCode (cp);
			cp.WriteLineUnind ("}");
			cp.WriteLine ("set {");
			set_builder.PrintCode (cp);
			cp.WriteLine ("}");
			cp.WriteLineUnind ("}");
		}
		
		public CodeArgumentReference GetArg (int n)
		{
			if (n < 0 || n >= parameterTypes.Length)
				throw new InvalidOperationException ("Invalid argument number");

			int narg = IsStatic ? n : n + 1;
			return new CodeArgumentReference (parameterTypes[n], narg, "arg" + n);
		}
		
		public CodeArgumentReference GetThis ()
		{
			if (IsStatic)
				throw new InvalidOperationException ("'this' not available in static methods");
				
			return new CodeArgumentReference (DeclaringType, 0, "this");
		}
		
		public void Generate ()
		{
			ILGenerator gen;
			Label returnLabel;
			MethodBuilder mb;

			// getter
			mb = (MethodBuilder) propertyInfo.GetGetMethod ();
			if (mb != null) {
				gen = mb.GetILGenerator();
				returnLabel = gen.DefineLabel ();
				get_builder.ReturnLabel = returnLabel;
				get_builder.Generate (gen);
				gen.MarkLabel (returnLabel);
				gen.Emit (OpCodes.Ret);
			}

			// setter
			mb = (MethodBuilder) propertyInfo.GetSetMethod ();
			if (mb != null) {
				gen = mb.GetILGenerator();
				returnLabel = gen.DefineLabel ();
				set_builder.ReturnLabel = returnLabel;
				set_builder.Generate (gen);
				gen.MarkLabel (returnLabel);
				gen.Emit (OpCodes.Ret);
			}
		}
		
		public void UpdatePropertyInfo (Type type)
		{
			propertyInfo = type.GetProperty (propertyInfo.Name, parameterTypes);
		}
	}
}
#endif
