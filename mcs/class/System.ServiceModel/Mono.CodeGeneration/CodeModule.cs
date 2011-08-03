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

#if !MONOTOUCH
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CodeGeneration
{
	public class CodeModule
	{
		ModuleBuilder module;
		static CodeModule sharedModule;
		
		public CodeModule (string name)
		{
			AppDomain myDomain = System.Threading.Thread.GetDomain();
			AssemblyName myAsmName = new AssemblyName();
			myAsmName.Name = name;
#if NET_2_1
			AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly (myAsmName, AssemblyBuilderAccess.Run);
#else
			AssemblyBuilder myAsmBuilder = myDomain.DefineDynamicAssembly (myAsmName, AssemblyBuilderAccess.RunAndSave);
#endif
			module = myAsmBuilder.DefineDynamicModule (name);
		}
		
		public static CodeModule Shared
		{
			get {
				if (sharedModule == null)
					sharedModule = new CodeModule ("SharedModule");
				return sharedModule;
			}
		}
		
		public CodeClass CreateClass (string name)
		{
			return CreateClass (name, TypeAttributes.Public, typeof(object));
		}
		
		public CodeClass CreateClass (string name, Type baseType, params Type[] interfaces)
		{
			return CreateClass (name, TypeAttributes.Public, baseType, interfaces);
		}
		
		public CodeClass CreateClass (string name, TypeAttributes attr, Type baseType, params Type[] interfaces)
		{
			return new CodeClass (module, name, attr, baseType, interfaces);
		}
		
		public ModuleBuilder ModuleBuilder
		{
			get { return module; }
		}
	}
}

#endif
