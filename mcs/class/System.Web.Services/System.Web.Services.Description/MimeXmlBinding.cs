// 
// System.Web.Services.Description.MimeXmlBinding.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

namespace System.Web.Services.Description {
	public sealed class MimeXmlBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string part;

		#endregion // Fields

		#region Constructors
		
		public MimeXmlBinding ()
		{
			part = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		public string Part {
			get { return part; }
			set { part = value; }
		}
		
		#endregion // Properties
	}
}
