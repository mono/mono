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

		ArrayList list = new ArrayList ();

		#endregion // Fields
	
		#region Constructors

		internal OdbcParameterCollection () {
		}

		#endregion // Constructors
	
		#region Properties

		[Browsable (false)]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
			get { return (OdbcParameter) list[index]; }
			set { list[index] = value; }
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

		#endregion // Properties

		#region Methods

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

		public OdbcParameter Add (OdbcParameter parameter)
		{
			if (parameter.Container != null)
                                throw new ArgumentException ("The OdbcParameter specified in the value parameter is already added to this or another OdbcParameterCollection.");
                                                                                                    
                        parameter.Container = this;
                        list.Add (parameter);
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
                        list.Insert (index, value);
                }
                                                                                                    
                public
#if NET_2_0
                override
#endif // NET_2_0
                void Remove (object value)
                {
                        ((OdbcParameter) value).Container = null;
                        list.Remove (value);
                }
                                                                                                    
                public
#if NET_2_0
                override
#endif // NET_2_0
                void RemoveAt (int index)
                {
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
		[MonoTODO]
		protected override DbParameter GetParameter (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override DbParameter GetParameter (int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetParameter (string name, DbParameter value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void SetParameter (int index, DbParameter value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void AddRange (Array values)
		{
			throw new NotImplementedException ();
		}
#endif
		#endregion // Methods

	}
}
