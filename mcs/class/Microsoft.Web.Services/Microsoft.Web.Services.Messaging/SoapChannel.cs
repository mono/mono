//
// Microsoft.Web.Services.Messaging.SoapChannel.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.IO;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Messaging
{
	public abstract class SoapChannel
	{
		
		private ISoapFormatter _formatter;
		private object _sync_obj = new object ();

		public SoapChannel (ISoapFormatter format)
		{
			if(format == null) {
				throw new ArgumentNullException ("formatter");
			}
			_formatter = format;
		}

		public abstract void Close ();

		public SoapEnvelope DeserializeMessage (Stream stream)
		{
			return _formatter.Deserialize (stream);
		}

		public void SerializeMessage (SoapEnvelope envelope, Stream stream)
		{
			_formatter.Serialize (envelope, stream);
		}

		public abstract SoapEnvelope Receive ();
		public abstract void Send (SoapEnvelope envelope);

		public abstract bool Active {
			get;
		}
		
		public abstract string Scheme {
			get;
		}

		public object SyncRoot {
			get { return _sync_obj; }
		}
		
		public IAsyncResult BeginReceive (AsyncCallback callback, object state)
		{
			return new ReceiveAsyncResult (this, callback, state);
		}

		public SoapEnvelope EndReceive (IAsyncResult result)
		{
			if(result == null) {
				throw new ArgumentNullException ("result");
			}

			ReceiveAsyncResult res = result as ReceiveAsyncResult;
			
			if(res != null) {
				ReceiveAsyncResult.End (result);
				return res.Envelope;
			}
			
			throw new InvalidOperationException ("Invalid IAsyncResult type");
		}

		public IAsyncResult BeginSend (SoapEnvelope env, AsyncCallback callback, object state)
		{
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			return new SendAsyncResult (this, env, callback, state);
		}

		public void EndSend (IAsyncResult result)
		{
			if(result == null) {
				throw new ArgumentNullException ("result");
			}
			SendAsyncResult a_res = result as SendAsyncResult;

			if(a_res != null) {
				AsyncResult.End (result);
			}
			throw new InvalidOperationException ("Invalid IAsyncResult type");
		}
		
		protected class ReceiveAsyncResult : AsyncResult
		{
			private SoapEnvelope _envelope;

			private delegate SoapEnvelope Receive ();

			public ReceiveAsyncResult (SoapChannel channel,
			                           AsyncCallback callback,
						   object state) : base (callback, state)
			{
				Receive rec = new Receive (channel.Receive);
				
				rec.BeginInvoke (new AsyncCallback (OnReceiveComplete), rec);
			}

			private void OnReceiveComplete (IAsyncResult result)
			{
				Receive rec = (Receive) result.AsyncState;

				try {
					_envelope = rec.EndInvoke (result);
					Complete (result.CompletedSynchronously);
				} catch (Exception e) {
					Complete (result.CompletedSynchronously, e);
				}
			}

			public SoapEnvelope Envelope {
				get { return _envelope; }
			}
		}

		protected class SendAsyncResult : AsyncResult
		{
			private delegate void Send (SoapEnvelope e);
			
			public SendAsyncResult (SoapChannel channel,
			                        SoapEnvelope env,
					        AsyncCallback callback,
					        object state) : base (callback, state)
			{
				Send send_delegate = new Send (channel.Send);
				send_delegate.BeginInvoke (env, new AsyncCallback (OnSendComplete), send_delegate);
			}

			private void OnSendComplete (IAsyncResult result)
			{
				Send s_del = result.AsyncState as Send;

				try {
					s_del.EndInvoke (result);
					this.Complete (result.CompletedSynchronously);
				} catch (Exception e) {
					Complete (result.CompletedSynchronously, e);
				}
			}
		}
	}
}
