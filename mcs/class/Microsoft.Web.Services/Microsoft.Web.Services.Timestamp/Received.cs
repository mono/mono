//
// Received.cs: 
//	Handles WS-Security "Utility" Timestamp Received
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//
// Licensed under MIT X11 (see LICENSE) with this specific addition:
//
// “This source code may incorporate intellectual property owned by Microsoft 
// Corporation. Our provision of this source code does not include any licenses
// or any other rights to you under any Microsoft intellectual property. If you
// would like a license from Microsoft (e.g. rebrand, redistribute), you need 
// to contact Microsoft directly.” 
//

using System;
using System.Xml;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Timestamp {

	public class Received : IXmlElement {

		private string id;
		private string idNS;
		private Uri actor;
		private long delay;
		private DateTime received;

		public Received (Uri actor) 
		{
			if (actor == null)
				throw new ArgumentNullException ("actor");
			this.actor = actor;
			received = DateTime.UtcNow;
		}

		public Received (XmlElement element) 
		{
			LoadXml (element);
		}

		public Uri Actor {
			get { return actor; }
			set { 
				if (value == null)
				throw new ArgumentNullException ("actor");
				actor = value; 
			}
		}

		public long Delay {
			get { return delay; }
			set { 
				if (delay < 0)
					throw new ArgumentException ("negative delay");
				delay = value;
			}
		}

		public DateTime Value {
			get { return received; }
			set { received = value; }
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new System.ArgumentNullException ("document");

			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSTimestamp.Prefix, WSTimestamp.ElementNames.Received, WSTimestamp.NamespaceURI);
			// xel.SetAttribute (WSTimestamp.AttributeNames.Actor, actor.AbsoluteUri);
			if (delay > 0)
				xel.SetAttribute (WSTimestamp.AttributeNames.Delay, delay.ToString ());
			// FIXME: commented for WSE compatibility
			// if (id != null)
			//	xel.SetAttribute (WSTimestamp.AttributeNames.Id, idNS, id);
			xel.InnerText = received.ToString (WSTimestamp.TimeFormat);
			return xel;
		}

		public void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new System.ArgumentNullException ("element");

			if ((element.LocalName != WSTimestamp.ElementNames.Received) || (element.NamespaceURI != WSTimestamp.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");

			XmlAttribute xa = element.Attributes [WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI];
			id = ((xa == null) ? null : xa.Value);
			idNS = ((xa == null) ? null : xa.NamespaceURI);

			xa = element.Attributes [WSTimestamp.AttributeNames.Actor];
			if (xa == null)
				throw new TimestampFormatException (TimestampFormatException.MissingActorAttributeInReceivedElement);
			string actorAttribute = xa.InnerText;
			if (actorAttribute == null)
				throw new TimestampFormatException (TimestampFormatException.MissingActorAttributeInReceivedElement);
			actor = new Uri (actorAttribute);

			xa = element.Attributes [WSTimestamp.AttributeNames.Delay];
			if (xa != null) {
				string delayAttribute = xa.InnerText;
				if (delayAttribute.StartsWith ("+"))
					throw new TimestampFormatException (TimestampFormatException.DelayAttributeWithPlusSign);
				try {
					delay = Convert.ToInt64 (delayAttribute);
				}
				catch {
					throw new TimestampFormatException (TimestampFormatException.BadDelayAttribute);
				}
			}

			received = DateTime.ParseExact (element.InnerText, WSTimestamp.TimeFormat, null);
			received = received.ToUniversalTime ();
		}
	}
}
