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

using System.Collections;
using System.Data;
using System.Data.Common;
using System.ComponentModel;

namespace System.Data.OleDb
{
        [ListBindable (false)]
        [EditorAttribute ("Microsoft.VSDesigner.Data.Design.DBParametersEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
	public sealed class OleDbParameterCollection : MarshalByRefObject,
		IDataParameterCollection, IList, ICollection, IEnumerable
	{
		#region Fields

		ArrayList list = new ArrayList ();

		#endregion // Fields

		#region Constructors
                                                                                                    
                public OleDbParameterCollection () {
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
                                if (!Contains (parameterName))                                         						throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
                                this [IndexOf (parameterName)] = value;
                        }                                                                                           
		}

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
		
		 public int Add (object value) {
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

		public OleDbParameter Add (string name, object value)
		{
			return Add (new OleDbParameter (name, value));	
		}

		public OleDbParameter Add (string name, OleDbType type)
	        {
			return Add (new OleDbParameter (name, type));
		}

		public OleDbParameter Add (string name, OleDbType type, int width)
		{
			return Add (new OleDbParameter (name, type, width));
		}

		public OleDbParameter Add (string name, OleDbType type,
					   int width, string src_col)
		{
			return Add (new OleDbParameter (name, type, width, src_col));
		}

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
			((OleDbParameter[])(list.ToArray ())).CopyTo (array, index);
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
	
		public void Clear() {
                       
			foreach (OleDbParameter p in list)
                                p.Container = null;
                                                                                                    
                        list.Clear ();
                }

		public bool Contains (object value) {
	
                	if (!(value is OleDbParameter))
                                throw new InvalidCastException ("The parameter was not an OleDbParameter.");
                        return Contains (((OleDbParameter) value).ParameterName);
                }
                                                                                                    
                public bool Contains (string value) {
                
                        foreach (OleDbParameter p in list)
                                if (p.ParameterName.Equals (value))
                                        return true;
                        return false;
                }
                                                                                                    
                public void CopyTo (Array array, int index) {
                
                        list.CopyTo (array, index);
                }
                                                                                                    
                public IEnumerator GetEnumerator() {
                
                        return list.GetEnumerator ();
                }
		
	        public int IndexOf (object value) {
                
                        if (!(value is OleDbParameter))
                                throw new InvalidCastException ("The parameter was not an OleDbParameter.");
                        return IndexOf (((OleDbParameter) value).ParameterName);
                }
                                                                                                    
                public int IndexOf (string parameterName) {
                
                        for (int i = 0; i < Count; i += 1)
                                if (this [i].ParameterName.Equals (parameterName))
                                        return i;
                        return -1;
                }
                                                                                                    
                public void Insert (int index, object value) {
                
                        list.Insert (index, value);
                }
                                                                                                    
                public void Remove (object value) {
                
                        ((OleDbParameter) value).Container = null;
                        list.Remove (value);
                }
                                                                                                    
                public void RemoveAt (int index) {
                
                        this [index].Container = null;
                        list.RemoveAt (index);
                }
		
		public void RemoveAt (string parameterName) {
                
                        RemoveAt (IndexOf (parameterName));
                }


		#endregion // Methods
	}
}
