// 
// System.Web.Services.Description.SoapBodyBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class SoapBodyBinding : ServiceDescriptionFormatExtension {

		#region Fields
		
		string encoding;
		string ns;
		string[] parts;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
		
		public SoapBodyBinding ()
		{
			encoding = String.Empty;
			ns = String.Empty;
			parts = null;
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

		public string[] Parts {
			get { return parts; }
			set { parts = value; }
		}

		public string PartsString {
			get { return String.Join (" ", Parts); }
			set { Parts = value.Split (' '); }
		}

		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

		#endregion // Properties
	}
}
