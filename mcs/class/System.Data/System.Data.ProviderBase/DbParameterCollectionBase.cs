//
// System.Data.ProviderBase.DbParameterCollectionBase
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Data.Common;

namespace System.Data.ProviderBase {
	public abstract class DbParameterBaseCollection : DbParameterCollection
	{
		#region Fields

		ArrayList list;

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]
		protected DbParameterBaseCollection ()
		{
			list = new ArrayList ();
		}

		#endregion // Constructors

		#region Properties

		public override int Count {
			get { return list.Count; }
		}

		public override bool IsFixedSize {
			get { return list.IsFixedSize; }
		}

		public override bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public override bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		protected abstract Type ItemType { get; }

		[MonoTODO]
		protected virtual string ParameterNamePrefix {
			get { throw new NotImplementedException (); }
		}

		public override object SyncRoot {
			get { return list.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override int Add (object value)
		{
			ValidateType (value);
			throw new NotImplementedException ();
		}

		public override void AddRange (Array values)
		{
			foreach (object value in values)
				Add (value);
		}

		[MonoTODO]
		protected override int CheckName (string parameterName)
		{
			throw new NotImplementedException ();
		}

		public override void Clear ()
		{
			list.Clear ();
		}

		public override bool Contains (object value)
		{
			return list.Contains (value);
		}

		[MonoTODO]
		public override bool Contains (string value)
		{
			throw new NotImplementedException ();
		}

		public override void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		protected override DbParameter GetParameter (int index)
		{
			return (DbParameter) list [index];
		}

		public override int IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		[MonoTODO]
		public override int IndexOf (string parameterName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal static int IndexOf (IEnumerable items, string parameterName)
		{
			throw new NotImplementedException ();
		}

		public override void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		[MonoTODO]
		protected virtual void OnChange ()
		{
			throw new NotImplementedException ();
		}

		public override void Remove (object value)
		{
			list.Remove (value);
		}

		public override void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		[MonoTODO]
		public override void RemoveAt (string parameterName)
		{
			throw new NotImplementedException ();
		}

		protected override void SetParameter (int index, DbParameter value)
		{
			list [index] = value;
		}

		[MonoTODO]
		protected virtual void Validate (int index, object value)
		{
			throw new NotImplementedException ();
		}

		protected virtual void ValidateType (object value)
		{
			Type objectType = value.GetType ();
			Type itemType = ItemType;

			if (objectType != itemType)
			{
				Type thisType = this.GetType ();
				string err = String.Format ("The {0} only accepts non-null {1} type objects, not {2} objects.", thisType.Name, itemType.Name, objectType.Name);
				throw new InvalidCastException (err);
			}
		}

		#endregion // Methods
	}
}

#endif
