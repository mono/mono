// 
// System.Web.Services.Description.MimeTextBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
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
	
		public MimeTextMatchCollection Matches {
			get { return matches; }
		}

		#endregion // Properties
	}
}
