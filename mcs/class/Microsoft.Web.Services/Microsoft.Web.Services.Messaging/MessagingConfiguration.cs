//
// Microsoft.Web.Services.Messaging.Configuration.MessagingConfiguration.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using System.Collections;
using Microsoft.Web.Services.Messaging;
using Microsoft.Web.Services.Configuration;

namespace Microsoft.Web.Services.Messaging.Configuration
{
	public class MessagingConfiguration : ConfigurationBase
	{
		
		private static Hashtable _transports;

		static MessagingConfiguration ()
		{
			_transports = new Hashtable ();
		}

		public MessagingConfiguration () : base ()
		{
			//Add transports here
			AddTransport ("soap.tcp", new SoapTcpTransport ());
		}

		public void AddTransport (string scheme, ISoapTransport trans)
		{
			if(scheme == null || scheme.Length == 0) {
				throw new ArgumentNullException ("scheme");
			}
			if(trans == null) {
				throw new ArgumentNullException ("transport");
			}
			_transports[scheme] = trans;
		}

		public ISoapTransport GetTransport (string scheme)
		{
			if(scheme == null || scheme.Length == 0) {
				throw new ArgumentNullException ("scheme");
			}
			return (ISoapTransport)_transports[scheme];
		}

		public void RemoveTransport (string scheme)
		{
			if(scheme == null || scheme.Length == 0) {
				throw new ArgumentNullException ("scheme");
			}
			_transports.Remove (scheme);
		}

		[MonoTODO]
		public void Load (XmlNode node) 
		{
			throw new NotImplementedException ();
		}

		public ICollection Transports {
			get { return _transports.Values; }
		}
		
	}
}
