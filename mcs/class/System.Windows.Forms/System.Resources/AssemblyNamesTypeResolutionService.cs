//
// AssemblyNamesTypeResolutionService.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2015 Xamarin Inc (http://www.xamarin.com)
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

using System.Reflection;
using System.ComponentModel.Design;

namespace System.Resources
{
	sealed class AssemblyNamesTypeResolutionService : ITypeResolutionService
	{
		public AssemblyNamesTypeResolutionService (AssemblyName[] names)
		{

		}

		#region ITypeResolutionService implementation

		public Assembly GetAssembly (AssemblyName name)
		{
			return GetAssembly (name, true);
		}

		public Assembly GetAssembly (AssemblyName name, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public Type GetType (string name)
		{
			return GetType (name, true);
		}

		public Type GetType (string name, bool throwOnError)
		{
			return GetType (name, throwOnError, false);
		}

		public Type GetType (string name, bool throwOnError, bool ignoreCase)
		{
			var type = Type.GetType (name, false, ignoreCase);
			if (type == null && throwOnError)
				throw new ArgumentException (string.Format ("Could not find a type for a name. The type name was `{0}'", name));

			return type;
		}

		public void ReferenceAssembly (AssemblyName name)
		{
			throw new NotImplementedException ();
		}

		public string GetPathOfAssembly (AssemblyName name)
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}