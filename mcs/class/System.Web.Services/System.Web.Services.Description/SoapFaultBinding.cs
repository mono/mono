// 
// System.Web.Services.Description.SoapFaultBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class SoapFaultBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string encoding;
		string ns;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
		
		public SoapFaultBinding ()
		{
			encoding = String.Empty;
			ns = String.Empty;
			use = SoapBindingUse.Default;
		}
		
		#endregion // Constructors

		#region Properties

		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}
		
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

		#endregion // Properties
	}
}
