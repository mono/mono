//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xaml;

namespace System.Xaml.Permissions
{
	[SerializableAttribute]
	public class XamlAccessLevel
	{
		public static XamlAccessLevel AssemblyAccessTo (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			return new XamlAccessLevel (assembly.GetName ());
		}
		public static XamlAccessLevel AssemblyAccessTo (AssemblyName assemblyName)
		{
			if (assemblyName == null)
				throw new ArgumentNullException ("assemblyName");
			return new XamlAccessLevel (assemblyName);
		}

		public static XamlAccessLevel PrivateAccessTo (string assemblyQualifiedTypeName)
		{
			if (assemblyQualifiedTypeName == null)
				throw new ArgumentNullException ("assemblyQualifiedTypeName");
			return new XamlAccessLevel (assemblyQualifiedTypeName);
		}

		public static XamlAccessLevel PrivateAccessTo (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			return new XamlAccessLevel (type.AssemblyQualifiedName);
		}

		internal XamlAccessLevel (AssemblyName assemblyAccessToAssemblyName)
		{
			AssemblyAccessToAssemblyName = assemblyAccessToAssemblyName;
		}

		internal XamlAccessLevel (string privateAccessToTypeName)
		{
			PrivateAccessToTypeName = privateAccessToTypeName;
		}

		public AssemblyName AssemblyAccessToAssemblyName { get; private set; }
		public string PrivateAccessToTypeName { get; private set; }
	}
}
