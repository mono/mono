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

		[MonoTODO]
		public HtmlFormParameterWriter () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Properties

		public override bool UsesWriteRequest {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public override void InitializeRequest (WebRequest request, object[] values)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void WriteRequest (Stream requestStream, object[] values)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
