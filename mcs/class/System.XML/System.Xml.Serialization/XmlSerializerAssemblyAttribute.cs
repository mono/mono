// 
// System.Xml.Serialization.XmlSerializerAssemblyAttribute.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Novell, Inc., 2004
//

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

namespace System.Xml.Serialization 
{
	[AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct, AllowMultiple=false)]
	public sealed class XmlSerializerAssemblyAttribute : Attribute
	{	
		string _assemblyName;
		string _codeBase;
		
		public XmlSerializerAssemblyAttribute ()
		{
		}

		public XmlSerializerAssemblyAttribute (string assemblyName)
		{
			_assemblyName = assemblyName;
		}

		public XmlSerializerAssemblyAttribute (string assemblyName, string codeBase)
			: this (assemblyName)
		{
			_codeBase = codeBase;
		}
		
		public string AssemblyName 
		{
			get { return _assemblyName; }
			set { _assemblyName = value; }
		}

		public string CodeBase 
		{
			get { return _codeBase; }
			set { _codeBase = value; }
		}
	}
}

