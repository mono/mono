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

		[MonoTODO]
		public NopReturnReader () 
		{
			throw new NotImplementedException ();
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (object initializer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object Read (WebResponse response, Stream responseStream)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
