// 
// System.Web.Services.Protocols.TextReturnReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Net;

namespace System.Web.Services.Protocols {
	public class TextReturnReader : MimeReturnReader {

		#region Constructors

		public TextReturnReader () 
		{
		}
		
		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Initialize (object o)
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
