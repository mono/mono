//
// Microsoft.Web.Services.Messaging.SoapTransport.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Net;

namespace Microsoft.Web.Services.Messaging
{
	public abstract class SoapTransport
	{
		private SoapChannelCollection _channels;
		private ICredentials _credentials;
		private ISoapFormatter _formatter;
		private int _timeout;

		[MonoTODO("DimeFormatter is the default here, but not impl yet")]
		protected SoapTransport () : base ()
		{
			_credentials = null;
			_formatter = new SoapPlainFormatter ();
			_channels = new SoapChannelCollection ();
			_timeout = 120000;
		}

		public SoapChannelCollection Channels {
			get { return _channels; }
		}

		public ICredentials Credentials {
			get { return _credentials; }
			set { _credentials = value; }
		}

		public ISoapFormatter Formatter {
			get { return _formatter; }
			set {
				if(value == null) {
					throw new ArgumentNullException ("formatter");
				}
				_formatter = value;
			}
		}

		public int IdleTimeout {
			get { return _timeout; }
			set {
				if(value < 0) {
					throw new ArgumentNullException ("Timeout");
				}
				_timeout = value;
			}
		}

		protected class SendAsyncResult : AsyncResult
		{
			private delegate void Send (SoapEnvelope env);

			private SoapChannel _channel;

			public SendAsyncResult (SoapChannel chan,
			                        SoapEnvelope env,
						AsyncCallback callback,
						object state) : base (callback, state)
			{
				_channel = chan;
				chan.BeginSend (env, new AsyncCallback (OnSendComplete), null);
			}

			public void OnSendComplete (IAsyncResult result)
			{
				try {
					_channel.EndSend (result);
					Complete (result.CompletedSynchronously);
				} catch (Exception e) {
					Complete (result.CompletedSynchronously, e);
				}
			}

			public SoapChannel Channel {
				get { return _channel; }
			}
		}
	}
}
