//
// System.Data.ObjectSpaces.Schema.SchemaClassCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

using System.Collections;

namespace System.Data.ObjectSpaces.Schema {
	public class SchemaClassCollection : CollectionBase
	{
		#region Properties

		public SchemaClass this [int index] {
			get { return (SchemaClass) List [index]; }
		}

		[MonoTODO]
		public SchemaClass this [string typeName] {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		public void Add (SchemaClass schemaClass)
		{
			Insert (Count, schemaClass);
		}

		public bool Contains (SchemaClass schemaClass)
		{
			return List.Contains (schemaClass);
		}

		public void CopyTo (SchemaClass[] array, int index)
		{
			List.CopyTo (array, index);
		}

		public int IndexOf (SchemaClass schemaClass)
		{
			return List.IndexOf (schemaClass);
		}

		public void Insert (int index, SchemaClass schemaClass)
		{
			List.Insert (index, schemaClass);
		}

		[MonoTODO]
		protected override void OnInsert (int index, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value)
		{
			throw new NotImplementedException ();
		}

		public void Remove (SchemaClass schemaClass)
		{
			List.Remove (schemaClass);
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
