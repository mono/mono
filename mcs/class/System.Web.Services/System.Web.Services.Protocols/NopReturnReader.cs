// 
// System.Web.Services.Protocols.NopReturnReader.cs
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
	public class NopReturnReader : MimeReturnReader {

		#region Constructors

		public NopReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			return this;
		}

		public override void Initialize (object initializer)
		{
		}

		public override object Read (WebResponse response, Stream responseStream)
		{
			responseStream.Close ();
			return null;
		}

		#endregion // Methods
	}
}
