//
// Microsoft.Web.Services.Messaging.SoapSender
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Web;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Addressing;
using Microsoft.Web.Services.Configuration;

namespace Microsoft.Web.Services.Messaging {

        public class SoapSender : SoapPort
        {

		private EndpointReference _destination = null;
		private ISoapTransport _transport = null;
		
		public SoapSender () : base ()
		{
		}

		public SoapSender (EndpointReference dest) : base ()
		{
			if(dest == null) {
				throw new ArgumentNullException ("destination");
			}
			Destination = dest;
		}

		public SoapSender (Uri destination) : this (new EndpointReference (destination))
		{
		}

		protected override void FilterMessage (SoapEnvelope env)
		{
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			Pipeline.ProcessOutputMessage (env);
		}

		public void Send (SoapEnvelope env)
		{
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			if(env.Context.Action == null) {
				throw new ArgumentException ("Action not set");
			}
			if(env.Processed == true || env.Context.Processed == true) {
				throw new ArgumentException ("Attempting to re-process an envelope");
			}
			
			if(_destination == null) {
				throw new ArgumentException ("Destination is not set, cant send");
			}
			if(_transport == null) {
				throw new ArgumentException ("Transport is not set, cant send");
			}

			env.Context.SetTo(_destination.Address);

			FilterMessage (env);

			_transport.Send (env, _destination);
		}

		[MonoTODO]
		public IAsyncResult BeginSend (SoapEnvelope env, AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndSend (IAsyncResult result)
		{
			throw new NotImplementedException ();
		}

		public ISoapTransport Transport {
			get { return _transport; }
		}

		public EndpointReference Destination {
			get { return _destination; }
			set {
				if(value == null || value.Address == null || value.Address.Value == null) {
					throw new ArgumentNullException ("destination");
				}
				ISoapTransport trans = WebServicesConfiguration.MessagingConfiguration.GetTransport (value.Address.Value.Scheme);
				if(trans == null) {
					throw new ArgumentException ("Transport " + value.Address.Value.Scheme + " is not supported");
				}
				_destination = value;
				_transport = trans;
			}
		}
        }
}
