// 
// System.Web.Services.Description.MimeMultipartRelatedBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("multipartRelated", "http://schemas.xmlsoap.org/wsdl/mime/", typeof (InputBinding), typeof (OutputBinding))]
	public sealed class MimeMultipartRelatedBinding : ServiceDescriptionFormatExtension {

		#region Fields

		MimePartCollection parts;

		#endregion // Fields

		#region Constructors
		
		public MimeMultipartRelatedBinding ()
		{
			parts = new MimePartCollection ();
		}
		
		#endregion // Constructors

		#region Properties

		[XmlElement ("parts")]	
		public MimePartCollection Parts {
			get { return parts; }
		}

		#endregion // Properties
	}
}
