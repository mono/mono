// 
// System.Web.Services.Protocols.MimeParameterReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web;

namespace System.Web.Services.Protocols {
	public abstract class MimeParameterReader : MimeFormatter {

		#region Constructors

		protected MimeParameterReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public abstract object[] Read (HttpRequest request);

		#endregion // Methods
	}
}
