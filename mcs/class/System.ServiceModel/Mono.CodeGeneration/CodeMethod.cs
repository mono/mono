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
	public class CodeMethod
	{
		MethodBase methodBase;
		CodeBuilder builder;
		string name;
		MethodAttributes attributes;
		Type returnType;
		TypeBuilder typeBuilder;
		Type[] parameterTypes;
		ArrayList customAttributes = new ArrayList ();
		CodeClass cls;
		
		internal static CodeMethod DefineMethod (CodeClass cls, string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes)
		{
			return new CodeMethod (cls, name, attributes, returnType, parameterTypes);
		}
		
		public static CodeMethod DefineConstructor (CodeClass cls, MethodAttributes attributes, Type[] parameterTypes)
		{
			return new CodeMethod (cls, attributes, parameterTypes);
		}
		
		internal CodeMethod (CodeClass cls, string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes) 
		{
			this.cls = cls;
			this.typeBuilder = cls.TypeBuilder;
			this.name = name;
			this.attributes = attributes;
			this.returnType = returnType;
			this.parameterTypes = parameterTypes;
		
			methodBase = typeBuilder.DefineMethod (name, attributes, returnType, parameterTypes);
			builder = new CodeBuilder (cls);
		}
		
		CodeMethod (CodeClass cls, MethodAttributes attributes, Type[] parameterTypes) 
		{
			this.cls = cls;
			this.typeBuilder = cls.TypeBuilder;
			this.attributes = attributes;
			this.parameterTypes = parameterTypes;
			this.name = typeBuilder.Name;
		
			methodBase = typeBuilder.DefineConstructor (attributes, CallingConventions.Standard, parameterTypes);
			builder = new CodeBuilder (cls);
		}
		
		public TypeBuilder DeclaringType
		{
			get { return typeBuilder; }
		}
		
		public MethodInfo MethodInfo
		{
			get { return methodBase as MethodInfo; }
		}
		
		public MethodBase MethodBase
		{
			get { return methodBase; }
		}
		
		public string Name
		{
			get { return name; }
		}
		
		public MethodAttributes Attributes
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
		
		public CodeBuilder CodeBuilder
		{
			get { return builder; }
		}
		
		public bool IsStatic
		{
			get { return (attributes & MethodAttributes.Static) != 0; }
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
			if (methodBase is MethodBuilder)
				((MethodBuilder) methodBase).SetCustomAttribute (cca.Builder);
			else if (methodBase is ConstructorBuilder)
				((ConstructorBuilder) methodBase).SetCustomAttribute (cca.Builder);
			customAttributes.Add (cca);
		}

		public string PrintCode ()
		{
			StringWriter sw = new StringWriter ();
			CodeWriter cw = new CodeWriter (sw);
			PrintCode (cw);
			return sw.ToString ();
		}
		
		public virtual void PrintCode (CodeWriter cp)
		{
			cp.BeginLine ();
			foreach (CodeCustomAttribute a in customAttributes)
				a.PrintCode (cp);
			if ((methodBase.Attributes & MethodAttributes.Static) != 0)
				cp.Write ("static ");
			if ((methodBase.Attributes & MethodAttributes.Public) != 0)
				cp.Write ("public ");
			if (returnType != null) cp.Write (returnType + " ");
			cp.Write (name + " (");
			for (int n=0; n<parameterTypes.Length; n++) {
				if (n > 0) cp.Write (", ");
				cp.Write (parameterTypes[n] + " arg" + n);
			}
			cp.Write (")");
			cp.EndLine ();
			cp.WriteLineInd ("{");
			
			builder.PrintCode (cp);
			
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
			ILGenerator gen = methodBase is MethodInfo ? ((MethodBuilder)methodBase).GetILGenerator() : ((ConstructorBuilder)methodBase).GetILGenerator();
			Label returnLabel = gen.DefineLabel ();
			builder.ReturnLabel = returnLabel;
			builder.Generate (gen);
			gen.MarkLabel(returnLabel);
			gen.Emit(OpCodes.Ret);
		}
		
		public void UpdateMethodBase (Type type)
		{
			if (methodBase is MethodInfo)
				methodBase = type.GetMethod (methodBase.Name, parameterTypes);
			else
				methodBase = type.GetConstructor (parameterTypes);
		}
	}
}
#endif
