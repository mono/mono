// Microsoft.Web.Services.Messaging.ISoapFormmatter.cs
//
// Author: Duncan Mak  <duncan@ximian.com>
//
// (C) Ximian, Inc. 2003.

using System.IO;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging {
	public interface ISoapFormatter {

		SoapEnvelope Deserialize (Stream stream);

		void Serialize (SoapEnvelope envelope, Stream stream);
	}
}
