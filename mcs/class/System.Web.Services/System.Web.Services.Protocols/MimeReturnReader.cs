// 
// System.Web.Services.Protocols.MimeReturnReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Net;

namespace System.Web.Services.Protocols {
	public abstract class MimeReturnReader : MimeFormatter {

		#region Constructors

		protected MimeReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public abstract object Read (WebResponse response, Stream responseStream);

		#endregion // Methods
	}
}
