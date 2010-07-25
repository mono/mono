//
// System.Data.Common.AbstractDbParameterCollection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Data.Common;
using System.Collections;

namespace System.Data.ProviderBase
{
	public abstract class AbstractDbParameterCollection : DbParameterCollection
	{
		#region Fields

		readonly AbstractDbCommand _parent;
		readonly ArrayList _list = new ArrayList();

		#endregion // Fields

		#region Constructors

		public AbstractDbParameterCollection(DbCommand parent)
        {
			_parent = (AbstractDbCommand)parent;
        }

		#endregion // Constructors

		#region Properties

		public override int Count {
			get { return _list.Count; }
		}

		public override bool IsFixedSize {
			get { return _list.IsFixedSize; }
		}

		public override bool IsReadOnly {
			get { return _list.IsReadOnly; }
		}

		public override bool IsSynchronized {
			get { return _list.IsSynchronized; }
		}

		protected abstract Type ItemType { get; }

		public override object SyncRoot {
			get { return _list.SyncRoot; }
		}

		#endregion // Properties

		#region Methods

		public override int Add (object value) {
			Validate (-1, value);
			OnSchemaChanging();
			((AbstractDbParameter)value).Parent = this;
			return _list.Add (value);
		}

#if NET_2_0
		public override void AddRange (Array values)
		{
			foreach (object value in values)
				Add (value);
		}

#endif

		public override void Clear () {
			OnSchemaChanging();
			if (_list != null && Count != 0) {
				for (int i = 0; i < _list.Count; i++) {
					((AbstractDbParameter)_list [i]).Parent = null;
				}
				_list.Clear ();
			}
		}

		public override bool Contains (object value) {
			if (IndexOf (value) != -1)
				return true;
			else
				return false;
		}

		public override bool Contains (string value) {
			if (IndexOf (value) != -1)
				return true;
			else
				return false;
		}

		public override void CopyTo (Array array, int index) {
			_list.CopyTo (array, index);
		}

		public override IEnumerator GetEnumerator () {
			return _list.GetEnumerator ();
		}

		protected override DbParameter GetParameter (int index) {
			return (DbParameter) _list [index];
		}

#if NET_2_0
		protected override DbParameter GetParameter (string parameterName) {
			return GetParameter (IndexOf (parameterName));
		}

		protected override void SetParameter (string parameterName, DbParameter value) {
			SetParameter (IndexOf (parameterName), value);
		}
#endif

		public override int IndexOf (object value) {
			ValidateType (value);
			return _list.IndexOf (value);
		}

		public override int IndexOf (string parameterName) {
			if (_list == null)
				return -1;

			for (int i = 0; i < _list.Count; i++) {
				string name = ((DbParameter)_list [i]).ParameterName;
				if (String.Compare (name, parameterName, StringComparison.OrdinalIgnoreCase) == 0) {
					return i;
				}
			}
			return -1;
		}

		public override void Insert (int index, object value) {
			Validate(-1, (DbParameter)value);
			OnSchemaChanging();
			((AbstractDbParameter)value).Parent = this;
			_list.Insert (index, value);
		}

		public override void Remove (object value) {
			ValidateType (value);
			int index = IndexOf (value);
			RemoveIndex (index);
		}

		public override void RemoveAt (int index) {
			RemoveIndex (index);
		}

		public override void RemoveAt (string parameterName) {
			int index = IndexOf (parameterName);
			RemoveIndex (index);
		}

		protected override void SetParameter (int index, DbParameter value) {
			Replace (index, value);
		}

		void Validate (int index, object value) {
			ValidateType (value);
			AbstractDbParameter parameter = (AbstractDbParameter) value;

			if (parameter.Parent != null) {
				if (parameter.Parent.Equals (this)) {
					if (IndexOf (parameter) != index)
						throw ExceptionHelper.CollectionAlreadyContains (ItemType,"ParameterName",parameter.ParameterName,this);                    
				}
				else {
					// FIXME :  The OleDbParameter with ParameterName 'MyParam2' is already contained by another OleDbParameterCollection.
					throw new ArgumentException ("");
				}
			}

			if (parameter.ParameterName == null  || parameter.ParameterName == String.Empty) {
				int newIndex = 1;
				string parameterName;
				
				do {
					parameterName = "Parameter" + newIndex;
					newIndex++;
				}
				while(IndexOf (parameterName) != -1);

				parameter.ParameterName = parameterName;
			}
		}		

		void ValidateType (object value) {
			if (value == null)
				throw ExceptionHelper.CollectionNoNullsAllowed (this,ItemType);

			Type objectType = value.GetType ();
			Type itemType = ItemType;

			if (itemType.IsInstanceOfType(objectType)) {
				Type thisType = this.GetType ();
				string err = String.Format ("The {0} only accepts non-null {1} type objects, not {2} objects.", thisType.Name, itemType.Name, objectType.Name);
				throw new InvalidCastException (err);
			}
		}

		private void RemoveIndex (int index) {
			OnSchemaChanging();
			AbstractDbParameter oldItem = (AbstractDbParameter)_list [index];
			oldItem.Parent = null;
			_list.RemoveAt (index);
		}		

		private void Replace (int index, DbParameter value) {
			Validate (index, value);
			AbstractDbParameter oldItem = (AbstractDbParameter)this [index];
			oldItem.Parent = null;

			((AbstractDbParameter)value).Parent = this;
			_list [index] = value;
		}

		protected internal void OnSchemaChanging()
        {
            if (_parent != null) {
                _parent.OnSchemaChanging();
            }
        }

		#endregion // Methods
	}
}
