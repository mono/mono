// 
// System.Web.Services.Protocols.SoapExtensionAttribute.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Protocols {
	public abstract class SoapExtensionAttribute : Attribute {

		#region Constructors

		protected SoapExtensionAttribute () 
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract Type ExtensionType {
			get;
		}

		public abstract int Priority {
			get;
			set;
		}

		#endregion // Properties
	}
}
