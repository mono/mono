// 
// System.Web.Services.Protocols.XmlReturnReader.cs
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
	public class XmlReturnReader : MimeReturnReader {

		#region Constructors

		public XmlReturnReader () 
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
		public override object[] GetInitializers (LogicalMethodInfo[] methodInfos)
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
