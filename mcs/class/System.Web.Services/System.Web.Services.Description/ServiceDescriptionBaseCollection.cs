// 
// System.Web.Services.Description.ServiceDescriptionBaseCollection.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections;
using System.Web.Services;

namespace System.Web.Services.Description {
	public abstract class ServiceDescriptionBaseCollection : CollectionBase {
		
		#region Fields

		Hashtable table = new Hashtable ();
		protected internal object parent;

		#endregion // Fields

		#region Properties

		protected virtual IDictionary Table {
			get { return table; }
		}

		#endregion // Properties

		#region Methods

		protected virtual string GetKey (object value) 
		{
			return null; 
		}

		protected override void OnClear ()
		{
			Table.Clear ();
		}

		protected override void OnInsertComplete (int index, object value)
		{
			if (GetKey (value) != null)
				Table [GetKey (value)] = value;
			SetParent (value, parent);
		}

		protected override void OnRemove (int index, object value)
		{
			if (GetKey (value) != null)
				Table.Remove (GetKey (value));
		}

		protected override void OnSet (int index, object oldValue, object newValue)
		{
			if (GetKey (oldValue) != null) 
				Table.Remove (GetKey (oldValue));
			if (GetKey (newValue) != null)
				Table [GetKey (newValue)] = newValue;
			SetParent (newValue, parent);
		}

		protected virtual void SetParent (object value, object parent)
		{
		}
			
		#endregion // Methods
	}
}
