// 
// System.Web.Services.Description.ServiceDescriptionFormatExtension.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;

namespace System.Web.Services.Description {
	public abstract class ServiceDescriptionFormatExtension {

		#region Fields
		
		bool handled;
		object parent;
		bool required;

		#endregion // Fields

		#region Constructors

		protected ServiceDescriptionFormatExtension () 
		{
			handled = false;
			parent = null;
			required = false;	
		}
		
		#endregion // Constructors

		#region Properties

		public bool Handled {
			get { return handled; }
			set { handled = value; }
		}

		public object Parent {
			get { return parent; }
		}

		public bool Required {	
			get { return required; }
			set { required = value; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (object value)
		{
			parent = value; 
		}
		
		#endregion // Methods
	}
}
