// 
// System.Web.Services.Protocols.UrlParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Web.Services;
using System.IO;

namespace System.Web.Services.Protocols {
	public class UrlParameterWriter : UrlEncodedParameterWriter {

		#region Constructors

		public UrlParameterWriter () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override string GetRequestUrl (string url, object[] parameters)
		{
			StringWriter sw = new StringWriter ();
			Encode (sw, parameters);
			return url + "?" + sw.ToString ();
		}

		#endregion // Methods
	}
}
