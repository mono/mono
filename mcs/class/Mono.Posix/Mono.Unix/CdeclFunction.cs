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

	// This class represents a single unmanaged function with "cdecl" calling
	// convention -- that is, it can accept a variable number of arguments which
	// are passed on the runtime stack.
	//
	// To use, create an instance:
	//
	//    CdeclFunction printf = new CdeclFunction ("the library", 
	//        "the function name", /* optional */ typeof (ReturnType));
	//
	// Then call the Invoke method with the appropriate number of arguments:
	//
	//    printf.Invoke (new object[]{"hello, %s\n", "world!"});
	//
	// In the background a P/Invoke definition for the method with the
	// requested argument types will be generated and invoked, invoking the
	// unmanaged function.  The generated methods are cached, so that subsequent
	// calls with the same argument list do not generate new code, speeding up
	// the call sequence.
	//
	// Invoking Cdecl functions is not guaranteed to be portable across all 
	// platforms.  For example, AMD64 requires that the caller set EAX to the 
	// number of floating point arguments passed in the SSE registers.  This 
	// is only required for variable argument/cdecl functions; consequently, 
	// the overload technique used by this class wouldn't normally work.  
	// Mono's AMD64 JIT works around this by always setting EAX on P/Invoke
	// invocations, allowing CdeclFunction to work properly, but it will not
	// necessarily always work.  See also: 
	//
	//     http://lwn.net/Articles/5201/?format=printable
	//
	// Due to potential portability issues, cdecl functions should be avoided 
	// on most platforms.
	//
	// This class is intended to be thread-safe.
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
					return mi;
				}

				TypeBuilder tb = CreateType (typeName);
				/* MethodBuilder mb = */ tb.DefinePInvokeMethod (
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

			return sb.ToString ();
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

