//
// Microsoft.Web.Services.Messaging.SoapTcpTransport.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Addressing;

namespace Microsoft.Web.Services.Messaging
{
	public class SoapTcpTransport : SoapTransport, ISoapTransport
	{
		
		public static string Scheme = "soap.tcp";
		
		private Hashtable _listeners = new Hashtable ();
		private Hashtable _receivers = new Hashtable ();

		public SoapTcpChannel GetTcpChannel (Uri to)
		{
			SoapTcpChannel chan = null;
			lock (Channels.SyncRoot) {
				int port = (to.Port == -1) ? 8081 : to.Port;

				Uri uri = new Uri (String.Format("{0}://{1}:{2}", Scheme, to.Host, port));

				chan = Channels[uri.AbsoluteUri] as SoapTcpChannel;

				if(chan == null) {
					SoapTcpChannel newChan = new SoapTcpChannel (uri, Formatter);
					Channels.Add (uri.AbsoluteUri, newChan);
				
					newChan.BeginReceive(new AsyncCallback (OnReceive), newChan);
					chan = newChan;
				
				}
			}
			return chan;
		}

		public IAsyncResult BeginSend (SoapEnvelope env,
		                                Uri dest,
						AsyncCallback callback,
						object state)
		{
			SoapTcpChannel chan = GetTcpChannel (dest);

			return new SendAsyncResult (chan, env, callback, state);
		}

		public void EndSend (IAsyncResult result)
		{
			if(result == null) {
				throw new ArgumentNullException ("result");
			}

			if(result is SendAsyncResult) {
				AsyncResult.End (result);
			} else {
				throw new InvalidOperationException ("Invalid IAsyncResult Type");
			}
		}

		public void RegisterPort (Uri to, Type rType)
		{
			GenericRegister (to, rType);
		}

		public void RegisterPort (Uri to, SoapReceiver rec)
		{
			GenericRegister (to, rec);
		}

		private void GenericRegister (Uri to, object rType)
		{
			if(to == null) {
				throw new ArgumentNullException ("to");
			}
			if(Scheme != to.Scheme) {
				throw new ArgumentException ("invalid Scheme");
			}
			//if(to.Host != "localhost") {
			//	throw new ArgumentException ("Host name " + to.Host + " does not equal " + Dns.GetHostName() );
			//}

			lock(Channels.SyncRoot) {
				if(Receivers.Contains (to) == true) {
					throw new ArgumentException ("URI has already been registered");
				}
				int port = to.Port <= -1 ? 8081 : to.Port;
				Uri to_with_port = new Uri (Scheme + "://" + to.Host + ":" + port);
				SoapTcpListener tcp = (SoapTcpListener) _listeners[to_with_port];

				if(tcp == null) {
					
					//This is what Hervey says it does, not WSE2 TP tested.
					
					if(to.Host == "localhost") {
						tcp = new SoapTcpListener (IPAddress.Loopback, port);
					} else {
						tcp = new SoapTcpListener (IPAddress.Any, port);
					}

					tcp.Start ();
					tcp.AddReference ();

					tcp.BeginAcceptSocket (new AsyncCallback (OnAcceptSocket), tcp);

					_listeners[to_with_port] = tcp;
				} else {
					tcp.AddReference ();
				}

				Receivers[to] = rType;
			}
		}

		public void Send (SoapEnvelope env, Uri to)
		{
			SoapTcpChannel tcp = GetTcpChannel (to);

			tcp.Send (env);
		}

		public void UnregisterAll ()
		{
			lock (Channels.SyncRoot) {

				Receivers.Clear ();
				foreach(SoapTcpListener tcp in _listeners.Values) {
					tcp.Stop ();
				}
				_listeners.Clear ();
			}
		}

		public void UnregisterPort (Uri to)
		{
			if(to == null) {
				throw new ArgumentNullException ("to");
			}
			lock(Channels.SyncRoot) {
				int port = to.Port < 0 ? 8081 : to.Port;

				Uri newTo = new Uri (Scheme + "://" + to.Host + ":" + port);
				SoapTcpListener tcp = _listeners[newTo] as SoapTcpListener;

				if(tcp != null) {
					if(tcp.ReleaseReference () == 0) {
						_listeners.Remove(newTo);
						tcp.Stop();
					}
					Receivers.Remove(to);
				}
			}
		}

		//private string Scheme {
		//	get { return "soap.tcp"; }
		//}

		private void OnAcceptSocket (IAsyncResult result)
		{
			try {
				SoapTcpListener tcp = (SoapTcpListener) result.AsyncState;

				if(tcp != null) {
					Socket sock = tcp.EndAcceptSocket (result);
					if(tcp.IsListening != true) {
						tcp.BeginAcceptSocket (new AsyncCallback (OnAcceptSocket), result);
					}
					if(sock != null) {
						SoapTcpChannel chan = new SoapTcpChannel (sock, Formatter);
						lock (Channels.SyncRoot) {
							Channels.Add (chan.Destination.AbsoluteUri, chan);
						}
						chan.BeginReceive (new AsyncCallback (OnReceive), chan);
					}
				}
			} catch (Exception)
			{
			}
		}

		private void OnReceive (IAsyncResult result)
		{
			SoapTcpChannel tcp = result.AsyncState as SoapTcpChannel;
			SoapEnvelope env = null;

			try {
				env = tcp.EndReceive (result);
				tcp.BeginReceive (new AsyncCallback (OnReceive), result);
			} catch (Exception) {
				lock (Channels.SyncRoot) {
					Channels.Remove (tcp.Destination.AbsoluteUri);
					tcp.Close ();
				}
			}
			
			if(env != null) {
				env.Context.Channel = tcp;

				AddressingHeaders header = env.Context.Addressing;

				header.Load (env);

				//FIXME: There is a lot more checking to do here for a valid message.
				if(header.To != null) {
					if(header.To.Value.Scheme != "soap.tcp") {
						SoapReceiver.DispatchMessageFault (env, new ArgumentException ("soap error"));
					}
				}
				SoapReceiver.DispatchMessage (env);
			}
		}
		
		protected IDictionary Receivers {
			get { return _receivers; }
		}
		
	}

}
