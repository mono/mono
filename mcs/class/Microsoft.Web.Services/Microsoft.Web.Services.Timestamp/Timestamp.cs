//
// Timestamp.cs: Handles WS-Security "Utility" Timestamp
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
using System.Web.Services.Protocols;
using System.Xml;
using Microsoft.Web.Services;
using Microsoft.Web.Services.Xml;

namespace Microsoft.Web.Services.Timestamp {

	// References
	// a.	Web Services Security Addendum, Version 1.0, August 18, 2002
	//	http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnglobspec/html/ws-security.asp

	public class Timestamp : SoapHeader, IXmlElement {

		private string id;
		private string idNS;
		private DateTime created;
		private string createdId;
		private string createdIdNS;
		private DateTime expired;
		private string expiredId;
		private string expiredIdNS;
		private ReceivedCollection rcoll;
		private long timeToLive;

		public Timestamp () 
		{
			created = DateTime.MinValue;
			expired = DateTime.MaxValue;
			timeToLive = 300000; // 5 minutes
			rcoll = new ReceivedCollection ();
		}

		// we must be able to fix both creation and expiration
		internal void SetTimestamp (DateTime c) 
		{
			created = c;
			expired = created.AddMilliseconds (timeToLive);
		}

		public new string Actor {
			get { return base.Actor; }
		}

		public DateTime Created {
			get { return created; }
		}

		public DateTime Expires {
			get { return expired; }
		}

		public ReceivedCollection Receivers {
			get { return rcoll; }
		}

		public long Ttl {
			get { return timeToLive; }
			set {
				if (value < 0)
					throw new System.ArgumentException ("value");
				if (value == 0)
					expired = DateTime.MaxValue;
				timeToLive = value; 
			}
		}

		// syntactically correct
		// no checks are done on the dates themselves !
		public void CheckValid () 
		{
			if (created == DateTime.MinValue)
				throw new TimestampFormatException (TimestampFormatException.MissingCreatedElement);
		}

		public XmlElement GetXml (XmlDocument document) 
		{
			if (document == null)
				throw new System.ArgumentNullException ("document");

			// much cleaner than using StringBuilder!
			XmlElement xel = document.CreateElement (WSTimestamp.Prefix, WSTimestamp.ElementNames.Timestamp, WSTimestamp.NamespaceURI);
			// FIXME: commented for WSE preview compatibility
			// if (id != null)
			//	xel.SetAttribute (WSTimestamp.AttributeNames.Id, idNS, id);

			if (created != DateTime.MinValue) {
				XmlElement xelCreated = document.CreateElement (WSTimestamp.Prefix, WSTimestamp.ElementNames.Created, WSTimestamp.NamespaceURI);
				xelCreated.InnerText = created.ToString (WSTimestamp.TimeFormat);
				if (createdId != null)
					xelCreated.SetAttribute (WSTimestamp.AttributeNames.Id, createdIdNS, createdId);
				xel.AppendChild (xelCreated);
			}
			if (expired != DateTime.MaxValue) {
				XmlElement xelExpires = document.CreateElement (WSTimestamp.Prefix, WSTimestamp.ElementNames.Expires, WSTimestamp.NamespaceURI);
				xelExpires.InnerText = expired.ToString (WSTimestamp.TimeFormat);
				if (expiredId != null)
					xelExpires.SetAttribute (WSTimestamp.AttributeNames.Id, expiredIdNS, expiredId);
				xel.AppendChild (xelExpires);
			}
			for (int i=0; i < rcoll.Count ; i++) 
			{
				XmlElement received = rcoll[i].GetXml (document);
				xel.AppendChild (received);
			}
			return xel;
		}

		public void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new System.ArgumentNullException ("element");

			if ((element.LocalName != WSTimestamp.ElementNames.Timestamp) || (element.NamespaceURI != WSTimestamp.NamespaceURI))
				throw new System.ArgumentException ("invalid LocalName or NamespaceURI");

			XmlAttribute xa = element.Attributes [WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI];
			id = ((xa == null) ? null : xa.Value);
			idNS = ((xa == null) ? null : xa.NamespaceURI);

			XmlNodeList xnl = element.GetElementsByTagName (WSTimestamp.ElementNames.Created, WSTimestamp.NamespaceURI);
			if (xnl != null) {
				switch (xnl.Count) {
					case 0:
//						throw new TimestampFormatException (TimestampFormatException.MissingCreatedElement);
						break;
					case 1:
						created = DateTime.ParseExact (xnl[0].InnerText, WSTimestamp.TimeFormat, null);
						created = created.ToUniversalTime ();
						xa = xnl[0].Attributes [WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI];
						createdId = ((xa == null) ? null : xa.Value);
						createdIdNS = ((xa == null) ? null : xa.NamespaceURI);
						break;
					default:
						throw new TimestampFormatException (TimestampFormatException.DuplicateCreatedElement);
				}
			}

			xnl = element.GetElementsByTagName (WSTimestamp.ElementNames.Expires, WSTimestamp.NamespaceURI);
			if (xnl != null) {
				switch (xnl.Count) {
					case 0:
//						throw new TimestampFormatException (TimestampFormatException.MissingCreatedElement);
						break;
					case 1:
						expired = DateTime.ParseExact (xnl[0].InnerText, WSTimestamp.TimeFormat, null);
						expired = expired.ToUniversalTime ();
						xa = xnl[0].Attributes [WSTimestamp.AttributeNames.Id, WSTimestamp.NamespaceURI];
						expiredId = ((xa == null) ? null : xa.Value);
						expiredIdNS = ((xa == null) ? null : xa.NamespaceURI);
						break;
					default:
						throw new TimestampFormatException (TimestampFormatException.DuplicateCreatedElement);
				}
			}

			xnl = element.GetElementsByTagName (WSTimestamp.ElementNames.Received, WSTimestamp.NamespaceURI);
			if (xnl != null) {
				for (int i=0; i < xnl.Count; i++) {
					Received r = new Received ((XmlElement)xnl [i]);
					rcoll.Add (r);
				}
			}
		}
	}
}
