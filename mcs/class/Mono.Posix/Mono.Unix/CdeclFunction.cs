//
// Mono.Unix/CdeclFunction.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Unix {

	// This class is intended to be thread-safe
	public sealed class CdeclFunction
	{
		// The readonly fields (1) shouldn't be modified, and (2) should only be
		// used when `overloads' is locked.
		private readonly string library;
		private readonly string method;
		private readonly Type returnType;
		private readonly AssemblyName assemblyName;
		private readonly AssemblyBuilder assemblyBuilder;
		private readonly ModuleBuilder moduleBuilder;

		private Hashtable overloads;

		public CdeclFunction (string library, string method)
			: this (library, method, typeof(void))
		{
		}

		public CdeclFunction (string library, string method, Type returnType)
		{
			Console.WriteLine ("** construction CdeclFunction for lib [{0}], export {1}",
					library, method);
			this.library = library;
			this.method = method;
			this.returnType = returnType;
			this.overloads = new Hashtable ();
			this.assemblyName = new AssemblyName ();
			this.assemblyName.Name = "Mono.Posix.Imports." + library;
			this.assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.Run);
			this.moduleBuilder = assemblyBuilder.DefineDynamicModule (assemblyName.Name);
		}

		public object Invoke (object[] parameters)
		{
			Type[] parameterTypes = GetParameterTypes (parameters);
			MethodInfo m = CreateMethod (parameterTypes);
			return m.Invoke (null, parameters);
		}

		private MethodInfo CreateMethod (Type[] parameterTypes)
		{
			string typeName = GetTypeName (parameterTypes);

			lock (overloads) {
				MethodInfo mi = (MethodInfo) overloads [typeName];

				if (mi != null) {
					Console.WriteLine ("** using cached MethodInfo");
					return mi;
				}
				Console.WriteLine ("** creating a new P/Invoke import");

				// TypeBuilder tb = ModuleBuilder.DefineType (typeName, TypeAttributes.Public);
				TypeBuilder tb = CreateType (typeName);
				MethodBuilder mb = tb.DefinePInvokeMethod (
						method, 
						library, 
						MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public,
						CallingConventions.Standard, 
						returnType, 
						parameterTypes, 
						CallingConvention.Cdecl,
						CharSet.Ansi);
				mi = tb.CreateType ().GetMethod (method);
				overloads.Add (typeName, mi);
				Console.WriteLine ("** # overloads: " + overloads.Count);
				return mi;
			}
		}

		private TypeBuilder CreateType (string typeName)
		{
			return moduleBuilder.DefineType (typeName, TypeAttributes.Public);
		}

		private static Type GetMarshalType (Type t)
		{
			switch (Type.GetTypeCode (t)) {
				// types < sizeof(int) are marshaled as ints
				case TypeCode.Boolean: case TypeCode.Char: case TypeCode.SByte: 
				case TypeCode.Int16: case TypeCode.Int32: 
					return typeof(int);
				case TypeCode.Byte: case TypeCode.UInt16: case TypeCode.UInt32:
					return typeof(uint);
				case TypeCode.Int64:
					return typeof(long);
				case TypeCode.UInt64:
					return typeof(ulong);
				case TypeCode.Single: case TypeCode.Double:
					return typeof(double);
				default:
					return t;
			}
		}

		private string GetTypeName (Type[] parameterTypes)
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append ("[").Append (library).Append ("] ").Append (method);
			sb.Append ("(");

			if (parameterTypes.Length > 0)
				sb.Append (parameterTypes [0]);
			for (int i = 1; i < parameterTypes.Length; ++i)
				sb.Append (",").Append (parameterTypes [i]);

			sb.Append (") : ").Append (returnType.FullName);

			string r = sb.ToString ();
			Console.WriteLine ("** type name: " + r + "; HashCode=" + r.GetHashCode());
			return r;
		}

		private static Type[] GetParameterTypes (object[] parameters)
		{
			Type[] parameterTypes = new Type [parameters.Length];
			for (int i = 0; i < parameters.Length; ++i)
				parameterTypes [i] = GetMarshalType (parameters [i].GetType ());
			return parameterTypes;
		}
	}
}

