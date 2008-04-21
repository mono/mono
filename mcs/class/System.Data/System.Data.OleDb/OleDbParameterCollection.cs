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
#if NET_2_0
		DbParameterCollection, IList, ICollection, IDataParameterCollection
#else
		MarshalByRefObject, IDataParameterCollection, IList, ICollection, IEnumerable
#endif
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

#if ONLY_1_1
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#endif
		public
#if NET_2_0
		override
#endif
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

#if NET_2_0
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
#else
		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		bool ICollection.IsSynchronized {
			get { return list.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return list.SyncRoot; }
		}

		object IList.this[int index] {
			get { return list[index]; }
			set { list[index] = value; }
		}

		object IDataParameterCollection.this [string index] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}
#endif

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

#if NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
#endif
		public
#if NET_2_0
		override
#endif
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

#if NET_2_0
		[Obsolete("OleDbParameterCollection.Add(string, value) is now obsolete. Use OleDbParameterCollection.AddWithValue(string, object) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
#endif
		public OleDbParameter Add (string parameterName, object value)
		{
			return Add (new OleDbParameter (parameterName, value));
		}

#if NET_2_0
		public OleDbParameter AddWithValue (string parameterName, object value)
		{
			return Add (new OleDbParameter (parameterName, value));
		}
#endif

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

#if NET_2_0
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
#endif

		public
#if NET_2_0
		override
#endif
		void Clear()
		{
			foreach (OleDbParameter p in list)
				p.Container = null;
			list.Clear ();
		}

		public
#if NET_2_0
		override
#endif
		bool Contains (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			return Contains (((OleDbParameter) value).ParameterName);
		}

		public
#if NET_2_0
		override
#endif
		bool Contains (string value)
		{
			foreach (OleDbParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

#if NET_2_0
		public bool Contains (OleDbParameter value)
		{
			return IndexOf (value) != -1;
		}
#endif

		public
#if NET_2_0
		override
#endif
		void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

#if NET_2_0
		public void CopyTo(OleDbParameter[] array, int index)
		{
			CopyTo (array, index);
		}
#endif

		public
#if NET_2_0
		override
#endif
		IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}

#if NET_2_0
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
#endif

		public
#if NET_2_0
		override
#endif
		int IndexOf (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			return IndexOf (((OleDbParameter) value).ParameterName);
		}

#if NET_2_0
		public int IndexOf(OleDbParameter value)
		{
			return IndexOf (value);
		}
#endif

		public
#if NET_2_0
		override
#endif
		int IndexOf (string parameterName)
		{
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.Equals (parameterName))
					return i;
			return -1;
		}

		public
#if NET_2_0
		override
#endif
		void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

#if NET_2_0
		public void Insert (int index, OleDbParameter value)
		{
			Insert (index, value);
		}
#endif

		public
#if NET_2_0
		override
#endif
		void Remove (object value)
		{
			((OleDbParameter) value).Container = null;
			list.Remove (value);
		}

#if NET_2_0
		public void Remove (OleDbParameter value)
		{
			Remove (value);
		}
#endif

		public
#if NET_2_0
		override
#endif
		void RemoveAt (int index)
		{
			this [index].Container = null;
			list.RemoveAt (index);
		}

		public
#if NET_2_0
		override
#endif
		void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

#if NET_2_0
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
#endif

		#endregion // Methods
	}
}
