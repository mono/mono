//
// Microsoft.Web.Services.Messaging.SoapReceiver.cs
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

        public abstract class SoapReceiver : SoapPort, IHttpHandler
        {

		private static Pipeline _defPipe = new Pipeline ();

		protected abstract void Receive (SoapEnvelope envelope);

		public SoapReceiver () : base ()
		{
		}

		protected void Receive (SoapEnvelope envelope, Exception e)
		{
		}

		public static void DispatchMessage (SoapEnvelope env)
		{
			DispatchMessage (env, null);
		}

		public static void DispatchMessage (SoapEnvelope env, RelatesTo relation)
		{
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			SoapReceiver rec = null;
			AddressingHeaders header = null;

			try {

				header = env.Context.Addressing;

				header.Load (env);

				if(header.RelatesTo != null || header.To != null || relation != null)
				{
					object to = null;
					if(header.RelatesTo != null) {
						if(WebServicesConfiguration.MessagingConfiguration.GetTransport (header.RelatesTo.Value.Scheme) == null) {
							//FIXME: Incorrect exception here, should be using something in Routing.
							throw new ArgumentException ("Transport " + header.RelatesTo.Value.Scheme + " is not supported");
						}
						to = SoapReceivers.Receiver (header.RelatesTo.Value);
						env.Context.SetActor (header.RelatesTo);
					}
					if(to == null && header.To != null) {
						if(env.Context.Channel.Scheme != header.To.Value.Scheme) {
							//FIXME: Incorrect exception again.
							throw new ArgumentException ("Channel's Scheme and To's Scheme are not the same.");
						}
						to = SoapReceivers.Receiver (header.To.Value);
						env.Context.SetActor (header.RelatesTo);
					}
					if(to == null && relation != null) {
						to = SoapReceivers.Receiver (relation.Value);
						env.Context.Addressing.RelatesTo = relation;
						env.Context.SetActor (relation);
					}
					if(to != null) {
						rec = to as SoapReceiver;
						if(rec == null) {
							rec = (SoapReceiver) Activator.CreateInstance (to as Type);
						}
						if(rec != null) {
							Exception excep = null;
							try {
								env.Context.SetIsInbound (true);
								rec.FilterMessage (env);
							} catch (Exception e) {
								excep = e;
							}
							if(excep != null) {
								//FIXME: again, wrong exception type, should be throwing something XmlElement like
								rec.Receive (env, excep);
							} else {
								rec.Receive (env);
							}
						} else {
							//FIXME: same thing
							throw new InvalidOperationException ("no receiver found");
						}
					} else {
						//FIXME: same
						throw new InvalidOperationException ("no receiver found pt 2");
					}
				}
			} catch (Exception e) {
				if(rec == null) {
					DispatchMessageFault (env, e);
				} else {
					DispatchMessageFault (env, e, rec.Pipeline);
				}
			}
		}
		
                public static void DispatchMessageFault (SoapEnvelope env, Exception e)
		{
			DispatchMessageFault (env, e, _defPipe);
		}

		public static void DispatchMessageFault (SoapEnvelope env, Exception e, Pipeline pipe)
		{
		}
		
		public bool IsReusable {
                        get {
                        	return true;
			}
                }

		protected override void FilterMessage (SoapEnvelope env)
		{
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			Pipeline.ProcessInputMessage (env);
		}

		public void ProcessSoapRequest (HttpContext context)
		{
			
		}

		public void ProcessNonSoapRequest (HttpContext context)
		{
			
		}

                public void ProcessRequest (HttpContext context)
                {
                	if(context.Request.HttpMethod == "POST" || context.Request.Headers["SOAPAction"] != null) {
				ProcessSoapRequest (context);
			}
			ProcessNonSoapRequest (context);
		}
        }
}
