// 
// System.Xml.Serialization.SchemaImporterExtensionCollection.cs 
//
// Author:
//   Lluis Sanchez Gual (lluis@novell.com)
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

#if NET_2_0

using System;
using System.Collections;

namespace System.Xml.Serialization 
{
	public class SchemaImporterExtensionCollection : CollectionBase
	{
		public SchemaImporterExtensionCollection ()
		{
		}

		[MonoTODO]
		public int Add (SchemaImporterExtension extension)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int Add (string name, Type type)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public new void Clear()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public bool Contains (SchemaImporterExtension extension)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void CopyTo (SchemaImporterExtension[] array, int index)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int IndexOf (SchemaImporterExtension extension)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Insert (int index, SchemaImporterExtension extension)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Remove (SchemaImporterExtension extension)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Remove (string name)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public SchemaImporterExtension this [int index] 
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}

#endif
