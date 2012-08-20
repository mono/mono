//
// ByteArrayFromResXHandler.cs : Handles a byte [] object that was stored
// in a resx file.
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

namespace System.Resources {
	internal class ByteArrayFromResXHandler : ResXDataNodeHandler, IWritableHandler {

		string dataString;

		public ByteArrayFromResXHandler (string data)
		{
			dataString = data;
		}

		#region implemented abstract members of System.Resources.ResXDataNodeHandler
		public override object GetValue (ITypeResolutionService typeResolver)
		{
			return Convert.FromBase64String (dataString);
		}

		public override object GetValue (AssemblyName [] assemblyNames)
		{
			return Convert.FromBase64String (dataString);
		}

		public override string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			Type type = ResolveType (typeof (byte []).AssemblyQualifiedName, typeResolver);
			return type.AssemblyQualifiedName;
		}

		public override string GetValueTypeName (AssemblyName [] assemblyNames)
		{
			return typeof (byte []).AssemblyQualifiedName;
		}
		#endregion		

		#region IWritableHandler implementation
		public string DataString {
			get {
				return dataString;
			}
		}
		#endregion
	}
}

