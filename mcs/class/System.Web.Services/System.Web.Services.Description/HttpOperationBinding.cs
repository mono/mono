// 
// System.Web.Services.Description.HttpOperationBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class HttpOperationBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string location;

		#endregion // Fields

		#region Constructors
		
		public HttpOperationBinding ()
		{
			location = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public string Location { 	
			get { return location; }
			set { location = value; }
		}
	
		#endregion // Properties
	}
}
