// 
// System.Web.Services.Description.MimeMultipartRelatedBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
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
	
		public MimePartCollection Parts {
			get { return parts; }
		}

		#endregion // Properties
	}
}
