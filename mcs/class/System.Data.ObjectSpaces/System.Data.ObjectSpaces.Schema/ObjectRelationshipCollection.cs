//
// System.Data.ObjectSpaces.Schema.ObjectRelationshipCollection.cs
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
	public class ObjectRelationshipCollection : CollectionBase
	{
		#region Properties

		[MonoTODO]
		public ObjectRelationship this [string name] {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public ObjectRelationship this [int index] {
			get { return (ObjectRelationship) List [index]; }
		}

		#endregion // Properties

		#region Methods

		public void Add (ObjectRelationship relationship)
		{
			Insert (Count, relationship);
		}

		public bool Contains (ObjectRelationship relationship)
		{
			return List.Contains (relationship);
		}

		[MonoTODO]
		public ObjectRelationship[] GetChildRelationships (Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ObjectRelationship[] GetParentRelationships (Type type)
		{
			throw new NotImplementedException ();
		}

		public int IndexOf (ObjectRelationship relationship)
		{
			return List.IndexOf (relationship);
		}

		public void Insert (int index, ObjectRelationship relationship)
		{
			List.Insert (index, relationship);
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

		public void Remove (ObjectRelationship relation)
		{
			List.Remove (relation);
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
