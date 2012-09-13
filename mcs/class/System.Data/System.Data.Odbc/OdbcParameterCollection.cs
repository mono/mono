//
// System.Data.Odbc.OdbcParameterCollection
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//   Umadevi S (sumadevi@novell.com)
//   Amit Biswas (amit@amitbiswas.com)
//
// Copyright (C) Brian Ritchie, 2002
// Copyright (C) Novell,Inc 
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	[ListBindable (false)]
	[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBParametersEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
#if NET_2_0
	public sealed class OdbcParameterCollection : DbParameterCollection
#else
	public sealed class OdbcParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
#endif // NET_2_0
	{
		#region Fields

		readonly ArrayList list = new ArrayList ();
		int nullParamCount = 1;

		#endregion // Fields
	
		#region Constructors

		internal OdbcParameterCollection ()
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
		public new OdbcParameter this [int index] {
			get { return (OdbcParameter) list [index]; }
			set { list [index] = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new OdbcParameter this [string parameterName] {
			get {
				foreach (OdbcParameter p in list)
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

#if ONLY_1_1
		bool IList.IsFixedSize {
#else
		public override bool IsFixedSize {
#endif
			get { return false; }
		}

#if ONLY_1_1
		bool IList.IsReadOnly {
#else
		public override bool IsReadOnly {
#endif
			get { return false; }
		}

#if ONLY_1_1
		bool ICollection.IsSynchronized {
#else
		public override bool IsSynchronized {
#endif
			get { return list.IsSynchronized; }
		}

#if ONLY_1_1
		object ICollection.SyncRoot {
#else
		public override object SyncRoot {
#endif
			get { return list.SyncRoot; }
		}
		
#if ONLY_1_1
		object IList.this [int index] {
			get { return list [index]; }
			set { list [index] = value; }
		}

		object IDataParameterCollection.this [string index]
		{
			get { return this [index]; }
			set {
				if (!(value is OdbcParameter))
					throw new InvalidCastException ("Only OdbcParameter objects can be used.");
				this [index] = (OdbcParameter) value;
			}
		}
#endif // ONLY_1_1

		#endregion // Properties

		#region Methods

#if NET_2_0
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
#endif
		public
#if NET_2_0
		override
#endif
		int Add (object value)
		{
			if (!(value is OdbcParameter))
				throw new InvalidCastException ("The parameter was not an OdbcParameter.");
			Add ((OdbcParameter) value);
			return IndexOf (value);
		}

		public OdbcParameter Add (OdbcParameter value)
		{
			if (value.Container != null)
				throw new ArgumentException ("The OdbcParameter specified in " +
							     "the value parameter is already " +
							     "added to this or another OdbcParameterCollection.");
			if (value.ParameterName == null || value.ParameterName.Length == 0) {
				value.ParameterName = "Parameter" + nullParamCount;
				nullParamCount++;
			}
			value.Container = this;
			list.Add (value);
			return value;
		}

#if NET_2_0
		[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		[Obsolete ("Add(String parameterName, Object value) has been deprecated.  Use AddWithValue(String parameterName, Object value).")]
#endif
		public OdbcParameter Add (string parameterName, object value)
		{
			return Add (new OdbcParameter (parameterName, value));
		}

		public OdbcParameter Add (string parameterName, OdbcType odbcType)
		{
			return Add (new OdbcParameter (parameterName, odbcType));
		}

		public OdbcParameter Add (string parameterName, OdbcType odbcType, int size)
		{
			return Add (new OdbcParameter (parameterName, odbcType, size));
		}

		public OdbcParameter Add (string parameterName, OdbcType odbcType,
					   int size, string sourceColumn)
		{
			return Add (new OdbcParameter (parameterName, odbcType,
				size, sourceColumn));
		}

		public
#if NET_2_0
		override
#endif
		void Clear()
		{
			foreach (OdbcParameter p in list)
				p.Container = null;
			list.Clear ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool Contains (object value)
		{
			if (value == null)
				//should not throw ArgumentNullException
				return false;
			if (!(value is OdbcParameter))
				throw new InvalidCastException ("The parameter was not an OdbcParameter.");
			return Contains (((OdbcParameter) value).ParameterName);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		bool Contains (string value)
		{
			if (value == null || value.Length == 0)
				//should not throw ArgumentNullException
				return false;
			string value_upper = value.ToUpper ();
			foreach (OdbcParameter p in this)
				if (p.ParameterName.ToUpper ().Equals (value_upper))
					return true;
			return false;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		IEnumerator GetEnumerator()
		{
			return list.GetEnumerator ();
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int IndexOf (object value)
		{
			if (value == null)
				return -1;
			if (!(value is OdbcParameter))
				throw new InvalidCastException ("The parameter was not an OdbcParameter.");
			return list.IndexOf (value);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		int IndexOf (string parameterName)
		{
			if (parameterName == null || parameterName.Length == 0)
				return -1;
			string parameterName_upper = parameterName.ToUpper ();
			for (int i = 0; i < Count; i += 1)
				if (this [i].ParameterName.ToUpper ().Equals (parameterName_upper))
					return i;
			return -1;
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Insert (int index, object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (!(value is OdbcParameter))
				throw new InvalidCastException ("The parameter was not an OdbcParameter.");
			Insert (index, (OdbcParameter) value);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void Remove (object value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (!(value is OdbcParameter))
				throw new InvalidCastException ("The parameter was not an OdbcParameter.");
			Remove ((OdbcParameter) value);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void RemoveAt (int index)
		{
			if (index >= list.Count || index < 0)
				throw new IndexOutOfRangeException (String.Format ("Invalid index {0} for this OdbcParameterCollection with count = {1}", index, list.Count));
			this [index].Container = null;
			list.RemoveAt (index);
		}

		public
#if NET_2_0
		override
#endif // NET_2_0
		void RemoveAt (string parameterName)
		{
			RemoveAt (IndexOf (parameterName));
		}

#if NET_2_0
		protected override DbParameter GetParameter (string parameterName)
		{
			return this [parameterName];
		}

		protected override DbParameter GetParameter (int index)
		{
			return this [index];
		}

		protected override void SetParameter (string parameterName, DbParameter value)
		{
			this [parameterName] = (OdbcParameter) value;
		}

		protected override void SetParameter (int index, DbParameter value)
		{
			this [index] = (OdbcParameter) value;
		}


		public override void AddRange (Array values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");
			foreach (OdbcParameter p in values)
				if (p == null)
					throw new ArgumentNullException ("values", "The OdbcParameterCollection only accepts non-null OdbcParameter type objects");
			// no need to check if parameter is already contained
			foreach (OdbcParameter p in values)
				Add (p);
		}

		public void AddRange (OdbcParameter [] values)
		{
			AddRange ((Array) values);
		}

		public void Insert (int index, OdbcParameter value)
		{
			if (index > list.Count || index < 0)
				throw new ArgumentOutOfRangeException ("index", "The index must be non-negative and less than or equal to size of the collection");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Container != null)
				throw new ArgumentException ("The OdbcParameter is already contained by another collection");
			if (String.IsNullOrEmpty (value.ParameterName)) {
				value.ParameterName = "Parameter" + nullParamCount;
				nullParamCount++;
			}
			value.Container = this;
			list.Insert (index, value);
		}

		public OdbcParameter AddWithValue (string parameterName, Object value)
		{
			if (value == null)
				return Add (new OdbcParameter (parameterName, OdbcType.NVarChar));
			return Add (new OdbcParameter (parameterName, value));
		}

		public void Remove (OdbcParameter value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Container != this)
				throw new ArgumentException ("values", "Attempted to remove an OdbcParameter that is not contained in this OdbcParameterCollection");
			value.Container = null;
			list.Remove (value);
		}

		public bool Contains (OdbcParameter value)
		{
			if (value == null)
				//should not throw ArgumentNullException
				return false;
			if (value.Container != this)
				return false;
			return Contains (value.ParameterName);
		}

		public int IndexOf (OdbcParameter value)
		{
			if (value == null)
				//should not throw ArgumentNullException
				return -1;
			return IndexOf ((Object) value);
		}

		public void CopyTo (OdbcParameter [] array, int index)
		{
			list.CopyTo (array, index);
		}
#endif

		#endregion // Methods
	}
}
