// 
// System.Web.Services.Description.SoapHeaderBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.Xml;

namespace System.Web.Services.Description {
	public sealed class SoapHeaderBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string encoding;
		bool mapToProperty;
		XmlQualifiedName message;
		string ns;
		string part;
		SoapBindingUse use;

		#endregion // Fields

		#region Constructors
	
		[MonoTODO]	
		public SoapHeaderBinding ()
		{
			encoding = String.Empty;
			mapToProperty = false; // FIXME: is this right?
			message = XmlQualifiedName.Empty;
			ns = String.Empty;
			part = String.Empty;
			use = SoapBindingUse.Default;
		}
		
		#endregion // Constructors

		#region Properties

		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public bool MapToProperty {	
			get { return mapToProperty; }
			set { mapToProperty = value; }
		}
	
		public XmlQualifiedName Message {
			get { return message; }
			set { message = value; }
		}
		
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		public string Part {
			get { return part; }
			set { part = value; }
		}

		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

		#endregion // Properties
	}
}
