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
	public sealed class OleDbParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
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
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Count {
			get { return list.Count; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public OleDbParameter this[int index] {
			get { return (OleDbParameter) list[index]; }
			set { list[index] = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public OleDbParameter this[string parameterName] {
			get {
				foreach (OleDbParameter p in list)
					if (p.ParameterName.Equals (parameterName))
						return p; 
				throw new IndexOutOfRangeException ("The specified name does not exist: " + parameterName);
			}
			set {
				if (!Contains (parameterName))
					throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
				this [IndexOf (parameterName)] = value;
			}
		}

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

		object IDataParameterCollection.this[string name] {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
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
		
		public int Add (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			Add ((OleDbParameter) value);
			return IndexOf (value);
		}

		public OleDbParameter Add (OleDbParameter parameter)
		{
			if (parameter.Container != null)
				throw new ArgumentException ("The OleDbParameter specified in the value parameter is already added to this or another OleDbParameterCollection.");
			parameter.Container = this;
			list.Add (parameter);
			return parameter;
		}

#if NET_2_0
		[Obsolete("OleDbParameterCollection.Add(string, value) is now obsolete. Use OleDbParameterCollection.AddWithValue(string, object) instead.")]
#endif
		public OleDbParameter Add (string name, object value)
		{
			return Add (new OleDbParameter (name, value));
		}

#if NET_2_0
		public OleDbParameter AddWithValue (string parameterName, object value)
		{
			return Add (new OleDbParameter (parameterName, value));
		}
#endif // NET_2_0

		public OleDbParameter Add (string name, OleDbType type)
		{
			return Add (new OleDbParameter (name, type));
		}

		public OleDbParameter Add (string name, OleDbType type, int width)
		{
			return Add (new OleDbParameter (name, type, width));
		}

		public OleDbParameter Add (string name, OleDbType type, int width, string src_col)
		{
			return Add (new OleDbParameter (name, type, width, src_col));
		}

		public void Clear()
		{
			foreach (OleDbParameter p in list)
				p.Container = null;
			list.Clear ();
		}

		public bool Contains (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			return Contains (((OleDbParameter) value).ParameterName);
		}

		public bool Contains (string value)
		{
			foreach (OleDbParameter p in list)
				if (p.ParameterName.Equals (value))
					return true;
			return false;
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}

		public int IndexOf (object value)
		{
			if (!(value is OleDbParameter))
				throw new InvalidCastException ("The parameter was not an OleDbParameter.");
			return IndexOf (((OleDbParameter) value).ParameterName);
		}

		public int IndexOf (string parameterName)
		{
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.Equals (parameterName))
					return i;
			return -1;
		}

		public void Insert (int index, object value)
		{
			list.Insert (index, value);
		}

		public void Remove (object value)
		{
			((OleDbParameter) value).Container = null;
			list.Remove (value);
		}

		public void RemoveAt (int index) {
			this [index].Container = null;
			list.RemoveAt (index);
		}

		public void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

		#endregion // Methods
	}
}
