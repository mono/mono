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

		#endregion // Fields

		#region Properties

		protected virtual IDictionary Table {
			get { return table; }
		}

		#endregion // Properties

		#region Methods

		protected virtual string GetKey (object value) 
		{
			return null; // per .NET documentation
		}

		[MonoTODO]
		protected override void OnClear ()
		{
		}

		[MonoTODO]
		protected override void OnInsertComplete (int index, object value)
		{
		}

		[MonoTODO]
		protected override void OnRemove (int index, object value)
		{
		}

		[MonoTODO]
		protected override void OnSet (int index, object oldValue, object newValue)
		{
		}

		[MonoTODO]
		protected virtual void SetParent (object value, object parent)
		{
		}
			
		#endregion // Methods
	}
}
