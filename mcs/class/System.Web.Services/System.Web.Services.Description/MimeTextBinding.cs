// 
// System.Web.Services.Description.MimeTextBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtension ("text", "http://microsoft.com/wsdl/mime/textMatching/", typeof (InputBinding), typeof (OutputBinding), typeof (MimePart))]
	[XmlFormatExtensionPrefix ("tm", "http://microsoft.com/wsdl/mime/textMatching/")]
	public sealed class MimeTextBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string Namespace = "http://microsoft.com/wsdl/mime/textMatching/";
		MimeTextMatchCollection matches;

		#endregion // Fields

		#region Constructors
		
		public MimeTextBinding ()
		{
			matches = new MimeTextMatchCollection ();
		}
		
		#endregion // Constructors

		#region Properties

		[XmlElement ("match", typeof (MimeTextMatch))]	
		public MimeTextMatchCollection Matches {
			get { return matches; }
		}

		#endregion // Properties
	}
}
