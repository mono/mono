//
// System.Data.Common.DbParameterCollection.cs
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

#if NET_2_0 || TARGET_JVM

using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Data.Common {
	public abstract class DbParameterCollection : MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Constructors

		protected DbParameterCollection ()
		{
		}

		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public abstract int Count {
			get;
		}

		object IDataParameterCollection.this [string parameterName] {
			get { return this [parameterName]; }
			set { this [parameterName] = (DbParameter) value; }
		}

		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (DbParameter) value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public abstract bool IsFixedSize {
			get;
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public abstract bool IsReadOnly {
			get;
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public abstract bool IsSynchronized {
			get;
		}

		public DbParameter this [string parameterName] {
			get {
				int index = IndexOf (parameterName);
				return this [index];
			}
			set {
				int index = IndexOf (parameterName);
				this [index] = value;
			}
		}

		public DbParameter this [int index] {
			get { return GetParameter (index); }
			set { SetParameter (index, value); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public abstract object SyncRoot {
			get;
		}

		#endregion // Properties

		#region Methods

		public abstract int Add (object value);

#if NET_2_0
		public abstract void AddRange (Array values);
		protected abstract DbParameter GetParameter (String parameterName);
		protected abstract void SetParameter (String parameterName, 
						      DbParameter value);
#endif

		public abstract void Clear ();
		public abstract bool Contains (object value);
		public abstract bool Contains (string value);
		public abstract void CopyTo (Array array, int index);

		[EditorBrowsable (EditorBrowsableState.Never)]
		public abstract IEnumerator GetEnumerator ();

		protected abstract DbParameter GetParameter (int index);
		public abstract int IndexOf (object value);
		public abstract int IndexOf (string parameterName);
		public abstract void Insert (int index, object value);
		public abstract void Remove (object value);
		public abstract void RemoveAt (int index);
		public abstract void RemoveAt (string parameterName);
		protected abstract void SetParameter (int index, DbParameter value);

		#endregion // Methods
	}
}

#endif
