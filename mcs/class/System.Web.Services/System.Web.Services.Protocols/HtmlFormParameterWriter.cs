// 
// System.Web.Services.Protocols.HtmlFormParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Net;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public class HtmlFormParameterWriter : UrlEncodedParameterWriter {

		#region Constructors

		public HtmlFormParameterWriter () 
		{
		}
		
		#endregion // Constructors

		#region Properties

		public override bool UsesWriteRequest {
			get { return true; }
		}

		#endregion // Properties

		#region Methods

		public override void InitializeRequest (WebRequest request, object[] values)
		{
			if (RequestEncoding == null) request.ContentType = "application/x-www-form-urlencoded";
			else request.ContentType = "application/x-www-form-urlencoded; charset=" + RequestEncoding.BodyName;
		}

		public override void WriteRequest (Stream requestStream, object[] values)
		{
			StreamWriter sw = new StreamWriter (requestStream);
			Encode (sw, values);
			sw.Flush ();
		}

		#endregion // Methods
	}
}
