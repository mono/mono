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
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.IO;

namespace System.Resources {
	internal class FileRefHandler : ResXDataNodeHandler {

		ResXFileRef resXFileRef; // may not be the same as that referenced in ResXDataNode
		ResXFileRef originalResXFileRef; // will be same

		public FileRefHandler (ResXFileRef fileRef, string basePath)
		{

			originalResXFileRef = fileRef;
			// recreate object to apply basePath if present
			if (basePath == null) {
				resXFileRef = fileRef;
			} else {
				string pathToUse = Path.Combine (basePath, fileRef.FileName);
				resXFileRef = new ResXFileRef (pathToUse,fileRef.TypeName);
			}
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			TypeConverter c = TypeDescriptor.GetConverter (typeof (ResXFileRef));
			return c.ConvertFromInvariantString (resXFileRef.ToString ());				
		}

		public override object GetValue (AssemblyName[] assemblyNames)
		{
			TypeConverter c = TypeDescriptor.GetConverter (typeof (ResXFileRef));
			return c.ConvertFromInvariantString (resXFileRef.ToString ());	
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			// although params ignored by GetValue. .NET resolves the type for GetValueTypeName
			return ResolveType (resXFileRef.TypeName, typeResolver).AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName[] assemblyNames)
		{
			// although params ignored by GetValue. .NET resolves the type for GetValueTypeName
			return ResolveType (resXFileRef.TypeName, assemblyNames).AssemblyQualifiedName;
		}
		#endregion

		public override object GetValueForResX ()
		{
			return originalResXFileRef;
		}
	}
}

