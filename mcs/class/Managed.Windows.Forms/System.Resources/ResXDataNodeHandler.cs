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
// Copyright (c) 2012 Gary Barnett
//
// Authors:
//	Gary Barnett

using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace System.Resources {
	internal interface IWritableHandler {
		string DataString { get;}
	}

	internal abstract class ResXDataNodeHandler {
		protected ResXDataNodeHandler ()
		{
		}

		public abstract object GetValue (ITypeResolutionService typeResolver);
		
		public abstract object GetValue (AssemblyName[] assemblyNames);

		public abstract string GetValueTypeName (ITypeResolutionService typeResolver);

		public abstract string GetValueTypeName (AssemblyName[] assemblyNames);

		//override by any inheritor that doesnt want to send the default output of GetValue to be written to ResXFile
		public virtual object GetValueForResX ()
		{
			return GetValue ((AssemblyName[]) null);
		}

		protected Type ResolveType (string typeString) 
		{
			// FIXME: check the test that shows you cant load a type with just a fullname from current assembly is valid
			return Type.GetType (typeString);
		}

		protected Type ResolveType (string typeString, AssemblyName[] assemblyNames) 
		{
			Type result = null;

			//FIXME: should I unload the assemblies again if type not found?
			if (assemblyNames != null) {
				foreach (AssemblyName assem in assemblyNames) {
						Assembly myAssembly = Assembly.Load (assem);
						result = myAssembly.GetType (typeString, false);
						if (result != null)
							return result;
					}
			}
			if (result == null)
				result = ResolveType (typeString);

			return result;
		}

		protected Type ResolveType (string typeString, ITypeResolutionService typeResolver) 
		{
			Type result = null;

			if (typeResolver != null)
				result = typeResolver.GetType (typeString);

			if (result == null)
				result = ResolveType (typeString);

			return result;
		}
	}
}
