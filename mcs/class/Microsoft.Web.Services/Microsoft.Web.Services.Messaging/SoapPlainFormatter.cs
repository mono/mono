//
// Microsoft.Web.Services.Messaging.SoapPlainFormatter.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.IO;

//FIXME: Can be removed when workaround is removed.
using System.Text;
using System.Net.Sockets;

using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging
{
	public class SoapPlainFormatter : ISoapFormatter
	{
		public SoapEnvelope Deserialize (Stream stream)
		{
			if(stream == null) {
				throw new ArgumentNullException ("stream");
			}
			SoapEnvelope env = new SoapEnvelope ();
			//env.Load (stream);
			

			//FIXME: Workaround for XmlDocument.Load's love of stream closing
			byte[] buf = new byte[1024];
			String msg = "";
			int numRead = 0;
			
			do {
				numRead = stream.Read(buf, 0, buf.Length);
				msg = String.Concat (msg, Encoding.ASCII.GetString (buf, 0, numRead));
			} while(((NetworkStream)stream).DataAvailable);
			
			env.LoadXml (msg);
			
			return env;
		}

		[MonoTODO("Should error if envelope has DimeAttachments")]
		public void Serialize (SoapEnvelope env, Stream stream)
		{
			if(stream == null) {
				throw new ArgumentNullException ("stream");
			}
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			
			env.Save (stream);
			stream.Flush();
		}
	}
}
