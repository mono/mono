// 
// System.Web.Services.Description.HttpBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class HttpBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/http/";
		string verb;

		#endregion // Fields

		#region Constructors
		
		public HttpBinding ()
		{
			verb = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public string Verb { 	
			get { return verb; }
			set { verb = value; }
		}
	
		#endregion // Properties
	}
}
