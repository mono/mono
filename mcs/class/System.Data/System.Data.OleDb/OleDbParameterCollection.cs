//
// System.Data.OleDb.OleDbParameterCollection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Umadevi S	 (sumadevi@novell.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Novell Inc.
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

using System.Collections;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.OleDb
{
	[ListBindable (false)]
	[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBParametersEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing)]
	public sealed class OleDbParameterCollection :
		DbParameterCollection, IList, ICollection, IDataParameterCollection
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors

		internal OleDbParameterCollection ()
		{
		}

		#endregion // Constructors
	
		#region Properties

		public
		override
		int Count {
			get { return list.Count; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new OleDbParameter this[int index] {
			get { return (OleDbParameter) list [index]; }
			set { list[index] = (OleDbParameter) value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new OleDbParameter this[string parameterName] {
			get {
				foreach (OleDbParameter p in list)
					if (p.ParameterName.Equals (parameterName))
						return p; 
				throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
			}
			set {
				if (!Contains (parameterName))
					throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
				this [IndexOf (parameterName)] = (OleDbParameter) value;
			}
		}

		public override bool IsFixedSize {
			get {
				return list.IsFixedSize;
			}
		}

		public override bool IsReadOnly {
			get {
				return list.IsReadOnly;
			}
		}

		public override bool IsSynchronized {
			get {
				return list.IsSynchronized;
			}
		}

		public override object SyncRoot {
			get {
				return list.SyncRoot;
			}
		}

		internal IntPtr GdaParameterList {
			[MonoTODO]
			get {
				IntPtr param_list;

				param_list = libgda.gda_parameter_list_new ();
				// FIXME: add parameters to list
				
				return param_list;
			}
		}

		#endregion // Properties

		#region Methods

		[EditorBrowsable(EditorBrowsableState.Never)]
		public
		override
		int Add (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			Add ((OleDbParameter) value);
			return IndexOf (value);
		}

		public OleDbParameter Add (OleDbParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The OleDbParameter specified in the value parameter is already added to this or another OleDbParameterCollection.");
			value.Container = this;
			list.Add (value);
			return value;
		}

		[Obsolete("OleDbParameterCollection.Add(string, value) is now obsolete. Use OleDbParameterCollection.AddWithValue(string, object) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public OleDbParameter Add (string parameterName, object value)
		{
			return Add (new OleDbParameter (parameterName, value));
		}

		public OleDbParameter AddWithValue (string parameterName, object value)
		{
			return Add (new OleDbParameter (parameterName, value));
		}

		public OleDbParameter Add (string parameterName, OleDbType oleDbType)
		{
			return Add (new OleDbParameter (parameterName, oleDbType));
		}

		public OleDbParameter Add (string parameterName, OleDbType oleDbType, int size)
		{
			return Add (new OleDbParameter (parameterName, oleDbType, size));
		}

		public OleDbParameter Add (string parameterName, OleDbType oleDbType, int size, string sourceColumn)
		{
			return Add (new OleDbParameter (parameterName, oleDbType, size, sourceColumn));
		}

		public override void AddRange(Array values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			foreach (object value in values)
				Add (value);
		}

		public void AddRange(OleDbParameter[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			foreach (OleDbParameter value in values)
				Add (value);
		}

		public
		override
		void Clear()
		{
			foreach (OleDbParameter p in list)
				p.Container = null;
			list.Clear ();
		}

		public
		override
		bool Contains (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			return Contains (((OleDbParameter) value).ParameterName);
		}

		public
		override
		bool Contains (string value)
		{
			foreach (OleDbParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

		public bool Contains (OleDbParameter value)
		{
			return IndexOf (value) != -1;
		}

		public
		override
		void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public void CopyTo(OleDbParameter[] array, int index)
		{
			CopyTo (array, index);
		}

		public
		override
		IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}

		[MonoTODO]
		protected override DbParameter GetParameter (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DbParameter GetParameter (string parameterName)
		{
			throw new NotImplementedException ();
		}

		public
		override
		int IndexOf (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			return IndexOf (((OleDbParameter) value).ParameterName);
		}

		public int IndexOf(OleDbParameter value)
		{
			return IndexOf (value);
		}

		public
		override
		int IndexOf (string parameterName)
		{
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.Equals (parameterName))
					return i;
			return -1;
		}

		public
		override
		void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Insert (int index, OleDbParameter value)
		{
			Insert (index, value);
		}

		public
		override
		void Remove (object value)
		{
			((OleDbParameter) value).Container = null;
			list.Remove (value);
		}

		public void Remove (OleDbParameter value)
		{
			Remove (value);
		}

		public
		override
		void RemoveAt (int index)
		{
			this [index].Container = null;
			list.RemoveAt (index);
		}

		public
		override
		void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

		[MonoTODO]
		protected override void SetParameter (int index, DbParameter value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetParameter (string parameterName, DbParameter value)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
