//
// aot.cs: AOT System.Reflection.Emit extensions to simplify mcs compilation
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2014 Xamarin Inc (http://www.xamarin.com)
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

#if FULL_AOT_RUNTIME

namespace System.Reflection.Emit
{
	static class AssemblyBuilderExtensions
	{
		public static void AddResourceFile (this AssemblyBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static ModuleBuilder DefineDynamicModule (this AssemblyBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static void DefineVersionInfoResource (this AssemblyBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static void DefineUnmanagedResource (this AssemblyBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static void Save (this AssemblyBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static void SetEntryPoint (this AssemblyBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	static class ConstructorBuilderExtensions
	{
		public static void AddDeclarativeSecurity (this ConstructorBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

	}

	static class MethodBuilderExtensions
	{
		public static void AddDeclarativeSecurity (this MethodBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

	}

	static class ModuleBuilderExtensions
	{
		public static void DefineManifestResource (this ModuleBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}
	}

	static class TypeBuilderExtensions
	{
		public static void AddDeclarativeSecurity (this TypeBuilder builder, params object[] args)
		{
			throw new NotSupportedException ();
		}

		public static Type CreateType (this TypeBuilder builder)
		{
			throw new NotSupportedException ();
		}
	}
}
#endif
