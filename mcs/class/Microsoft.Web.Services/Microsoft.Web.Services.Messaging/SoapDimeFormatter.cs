//
// Microsoft.Web.Services.Messaging.SoapDimeFormatter.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.IO;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging
{
	public class SoapDimeFormatter : ISoapFormatter
	{

		[MonoTODO]
		public SoapEnvelope Deserialize (Stream stream)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Serialize (SoapEnvelope envelope, Stream stream)
		{
			throw new NotImplementedException ();
		}
	}
}
