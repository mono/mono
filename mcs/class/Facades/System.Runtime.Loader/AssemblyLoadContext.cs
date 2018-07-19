//
// AssemblyLoadContext.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Runtime.Loader
{
	//
	// System.Runtime.Loader netstandard typeforwarders dependency
	//
	public abstract class AssemblyLoadContext
	{
		protected AssemblyLoadContext ()
		{
		}
		
		public static System.Runtime.Loader.AssemblyLoadContext Default { 
			get { 
				throw new NotImplementedException ();
			}
		}
		
		public static System.Reflection.AssemblyName GetAssemblyName (string assemblyPath)
		{
			throw new NotImplementedException ();
		}
		
		public static AssemblyLoadContext GetLoadContext (System.Reflection.Assembly assembly) 
		{
			throw new NotImplementedException ();
		}
		
		protected abstract System.Reflection.Assembly Load (System.Reflection.AssemblyName assemblyName);
		
		public System.Reflection.Assembly LoadFromAssemblyName(System.Reflection.AssemblyName assemblyName) 
		{
			throw new NotImplementedException ();
		}
		
		public System.Reflection.Assembly LoadFromAssemblyPath (string assemblyPath)
		{
			throw new NotImplementedException ();
		}
		
		public System.Reflection.Assembly LoadFromNativeImagePath (string nativeImagePath, string assemblyPath)
		{
			throw new NotImplementedException ();
		}
		
		public System.Reflection.Assembly LoadFromStream (System.IO.Stream assembly)
		{
			throw new NotImplementedException ();
		}

		public System.Reflection.Assembly LoadFromStream (System.IO.Stream assembly, System.IO.Stream assemblySymbols) 
		{
			throw new NotImplementedException ();
		}
		
		protected IntPtr LoadUnmanagedDllFromPath (string unmanagedDllPath)
		{
			throw new NotImplementedException ();
		}
		
		protected virtual IntPtr LoadUnmanagedDll (string unmanagedDllName)
		{
			throw new NotImplementedException ();
		}

		public void SetProfileOptimizationRoot (string directoryPath)
		{
		}

		public void StartProfileOptimization (string profile)
		{        	
		}

#pragma warning disable 67
		public event Func<AssemblyLoadContext, System.Reflection.AssemblyName, System.Reflection.Assembly> Resolving;
		public event Action<AssemblyLoadContext> Unloading;
#pragma warning restore
	}
}
