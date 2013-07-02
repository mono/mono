//
// FileRefHandler.cs : Handles a ResXFileRef that was either stored in 
// a resx file or has been added to a ResXDataNode that was freshly 
// instantiated by a user 
// 
// Author:
//	Gary Barnett (gary.barnett.mono@gmail.com)
// 
// Copyright (C) Gary Barnett (2012)
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

using System;
using System.Reflection;
using System.ComponentModel.Design;
using System.ComponentModel;
using System.IO;

namespace System.Resources {
	internal class FileRefHandler : ResXDataNodeHandler {
		ResXFileRef resXFileRef; // same as that referenced in ResXDataNode

		public FileRefHandler (ResXFileRef fileRef)
		{
			resXFileRef = fileRef;
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			return GetValue ();		
		}

		public override object GetValue (AssemblyName [] assemblyNames)
		{
			return GetValue ();
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			// although params ignored by GetValue. .NET resolves the type for GetValueTypeName
			Type type = ResolveType (resXFileRef.TypeName, typeResolver);

			if (type == null)
				return resXFileRef.TypeName;
			else
				return type.AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName [] assemblyNames)
		{
			Type type = ResolveType (resXFileRef.TypeName, assemblyNames);

			if (type == null)
				return resXFileRef.TypeName;
			else
				return type.AssemblyQualifiedName;
		}
		#endregion

		private object GetValue ()
		{
			TypeConverter c = TypeDescriptor.GetConverter (typeof (ResXFileRef));

			try {
				return c.ConvertFromInvariantString (resXFileRef.ToString ());
			} catch (ArgumentNullException ex) {
				if (ex.ParamName == "type")
					throw new TypeLoadException ("Could not find type", ex); //FIXME: error message?
				else 
					throw ex;
			}
		}

	}
}

