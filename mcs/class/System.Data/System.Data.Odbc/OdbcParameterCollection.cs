//
// System.Data.Odbc.OdbcParameterCollection
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com) 
//   Umadevi S (sumadevi@novell.com)
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
using System.Data;
using System.ComponentModel;
using System.Data.Common;

#if NET_2_0
using System.Data.ProviderBase;
#endif // NET_2_0

namespace System.Data.Odbc
{
	[ListBindable (false)]
        [EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBParametersEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
#if NET_2_0
        public sealed class OdbcParameterCollection : DbParameterBaseCollection
#else
	public sealed class OdbcParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
#endif // NET_2_0
	{
		#region Fields

#if ONLY_1_1
		ArrayList list = new ArrayList ();
#endif // ONLY_1_1

		#endregion // Fields
	
		#region Constructors

		internal OdbcParameterCollection () {
		}

		#endregion // Constructors
	
		#region Properties
#if ONLY_1_1		
		[Browsable (false)]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Count {
			get { return list.Count; }
		}

                [Browsable (false)]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public OdbcParameter this[int index] {
			get { return (OdbcParameter) list[index]; }
			set { list[index] = value; }
		}

                [Browsable (false)]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public OdbcParameter this[string parameterName] {
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
#else
                [Browsable (false)]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new OdbcParameter this[int index] {
			get { return (OdbcParameter) base[index]; }
			set { base [index] = value; }
		}

                [Browsable (false)]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new OdbcParameter this[string parameterName] {
			get {
                                foreach (OdbcParameter p in this)
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

#endif // ONLY_1_1


#if ONLY_1_1
		int ICollection.Count {
			get { return list.Count; }
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

		object IDataParameterCollection.this[string name]
		{
			get { return this[name]; }
                        set {
                                if (!(value is OdbcParameter))
                                        throw new InvalidCastException ("Only OdbcParameter objects can be used.");
                                this [name] = (OdbcParameter) value;
                        }

		}
#endif // ONLY_1_1

#if NET_2_0
                protected override Type ItemType { get { return typeof (OdbcParameter); } }
#endif // NET_2_0

		#endregion // Properties

		#region Methods

#if ONLY_1_1
		public int Add (object value)
                {
                         if (!(value is OdbcParameter))
                                throw new InvalidCastException ("The parameter was not an OdbcParameter.");
                        Add ((OdbcParameter) value);
                        return IndexOf (value);
                }
#endif // ONLY_1_1

		
		public OdbcParameter Add (OdbcParameter parameter)
		{
			if (parameter.Container != null)
                                throw new ArgumentException ("The OdbcParameter specified in the value parameter is already added to this or another OdbcParameterCollection.");
                                                                                                    
                        parameter.Container = this;
#if ONLY_1_1
                        list.Add (parameter);
#else
                        base.Add ((DbParameter) parameter);
#endif // ONLY_1_1
	                return parameter;
		}

		public OdbcParameter Add (string name, object value)
		{
			return Add (new OdbcParameter (name, value));
		}

		public OdbcParameter Add (string name, OdbcType type)
	        {
			return Add (new OdbcParameter (name, type));
		}

		public OdbcParameter Add (string name, OdbcType type, int width)
		{
			return Add (new OdbcParameter (name, type, width));
		}

		public OdbcParameter Add (string name, OdbcType type,
					   int width, string src_col)
		{
			return Add (new OdbcParameter (name, type, width, src_col));
		}

		internal void Bind(IntPtr hstmt)
		{
			for (int i=0;i<Count;i++)
			{
				this[i].Bind(hstmt,i+1);
				
			}
		}
#if ONLY_1_1
		int IList.Add (object value)
		{
			if (!(value is IDataParameter))
				throw new InvalidCastException ();


			list.Add (value);
			return list.IndexOf (value);
		}

		void IList.Clear ()
		{
			list.Clear ();
		}

		bool IList.Contains (object value)
		{
			return list.Contains (value);
		}

		bool IDataParameterCollection.Contains (string value)
		{
			for (int i = 0; i < list.Count; i++) {
				IDataParameter parameter;

				parameter = (IDataParameter) list[i];
				if (parameter.ParameterName == value)
					return true;
			}

			return false;
		}

		void ICollection.CopyTo (Array array, int index)
		{
			((OdbcParameter[])(list.ToArray ())).CopyTo (array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return list.GetEnumerator ();
		}
		
		int IList.IndexOf (object value)
		{
			return list.IndexOf (value);
		}

		int IDataParameterCollection.IndexOf (string name)
		{
			return list.IndexOf (((IDataParameterCollection) this)[name]);
		}

		void IList.Insert (int index, object value)
	        {
			list.Insert (index, value);
		}

		void IList.Remove (object value)
		{
			list.Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			list.Remove ((object) list[index]);
		}

		void IDataParameterCollection.RemoveAt (string name)
		{
			list.Remove (((IDataParameterCollection) this)[name]);
		}
#endif // ONLY_1_1
		
#if ONLY_1_1
		public void Clear()
                {
                        foreach (OdbcParameter p in list)
                                p.Container = null;
                                                                                                    
                        list.Clear ();
                }
#else
                public override void Clear()
                {
                        foreach (OdbcParameter p in this)
                                p.Container = null;
                                                                                                    
                        base.Clear ();
                }

#endif // ONLY_1_1

		public
#if NET_2_0
                override
#endif // NET_2_0
                bool Contains (object value)
                {
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
                        foreach (OdbcParameter p in this)
                                if (p.ParameterName.Equals (value))
                                        return true;
                        return false;
                }

		public
#if NET_2_0
                override
#endif // NET_2_0
                void CopyTo (Array array, int index)
                {
#if ONLY_1_1
                        list.CopyTo (array, index);
#else
                        base.CopyTo (array, index);
#endif //ONLY_1_1
                }

		public
#if NET_2_0
                override
#endif // NET_2_0
                IEnumerator GetEnumerator()
                {
#if ONLY_1_1
                        return list.GetEnumerator ();
#else
                        return base.GetEnumerator ();
#endif // ONLY_1_1
                }

  		public
#if NET_2_0
                override
#endif // NET_2_0
                int IndexOf (object value)
                {
                        if (!(value is OdbcParameter))
                                throw new InvalidCastException ("The parameter was not an OdbcParameter.");
                        return IndexOf (((OdbcParameter) value).ParameterName);
                }
                                                                                                    
                public
#if NET_2_0
                override
#endif // NET_2_0
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
#endif // NET_2_0
                void Insert (int index, object value)
                {
#if ONLY_1_1
                        list.Insert (index, value);
#else
                        base.Insert (index, value);
#endif // ONLY_1_1
                }
                                                                                                    
                public
#if NET_2_0
                override
#endif // NET_2_0
                void Remove (object value)
                {
                        ((OdbcParameter) value).Container = null;
#if ONLY_1_1
                        list.Remove (value);
#else
                        base.Remove (value);
                        
#endif // ONLY_1_1
                }
                                                                                                    
                public
#if NET_2_0
                override
#endif // NET_2_0
                void RemoveAt (int index)
                {
                        this [index].Container = null;
#if ONLY_1_1
                        list.RemoveAt (index);
#else
                        base.RemoveAt (index);
#endif // ONLY_1_1
                }
                                                                                                    
                public
#if NET_2_0
                override
#endif // NET_2_0
                void RemoveAt (string parameterName)
                {
                        RemoveAt (IndexOf (parameterName));
                }



		#endregion // Methods

	}
}
