// 
// System.Web.Services.Description.SoapOperationBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class SoapOperationBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string soapAction;
		SoapBindingStyle style;

		#endregion // Fields

		#region Constructors
	
		public SoapOperationBinding ()
		{
			soapAction = String.Empty;
			style = SoapBindingStyle.Document;
		}
		
		#endregion // Constructors

		#region Properties

		public string SoapAction {
			get { return soapAction; }
			set { soapAction = value; }
		}

		public SoapBindingStyle Style {
			get { return style; }
			set { style = value; }
		}

		#endregion // Properties
	}
}
