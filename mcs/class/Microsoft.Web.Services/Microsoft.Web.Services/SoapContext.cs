//
// SoapContext.cs: SOAP Context
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using Microsoft.Web.Services.Dime;
using Microsoft.Web.Services.Referral;
using Microsoft.Web.Services.Routing;
using Microsoft.Web.Services.Security;
using Microsoft.Web.Services.Timestamp;
#if !WSE1
using Microsoft.Web.Services.Addressing;
using Microsoft.Web.Services.Messaging;
#endif

using System;
using System.Collections;

namespace Microsoft.Web.Services {

	public sealed class SoapContext {

		private SoapEnvelope envelope;
#if WSE1
		private Uri actor;
#else
		private Uri actor = new Uri ("http://" + System.Net.Dns.GetHostName ());
#endif
		private Microsoft.Web.Services.Timestamp.Timestamp timestamp;
		private Microsoft.Web.Services.Security.Security security;
		private Hashtable table;
		private DimeAttachmentCollection attachments;
		private string contentType;
		private SecurityCollection extendedSecurity;
		private ReferralCollection referrals;
#if !WSE1
                private AddressingHeaders addressingHeaders;
		private SoapChannel _channel;
		private bool _processed = false;
		private bool _isInbound = false;
#endif
		internal SoapContext () : this (null) 
		{
		}

		internal SoapContext (SoapEnvelope env) 
		{
			timestamp = new Microsoft.Web.Services.Timestamp.Timestamp ();
#if WSE1
			table = new Hashtable ();

			envelope = env;
#else //WSE2
			addressingHeaders = new AddressingHeaders (env);

			envelope = env;
#endif
		}
#if !WSE1
		public Action Action {
			get { return addressingHeaders.Action; }
			set { addressingHeaders.Action = value; }
		}

		public ReplyTo ReplyTo {
			get { return addressingHeaders.ReplyTo; }
			set { addressingHeaders.ReplyTo = value; }
		}
  	 
		public To To {
			get { return addressingHeaders.To; }
		}

		public AddressingHeaders Addressing {
			get { return addressingHeaders; }
			set { addressingHeaders = value; }
		}

		public FaultTo FaultTo {
			get { return addressingHeaders.FaultTo; }
			set { addressingHeaders.FaultTo = value; }
		}

		public From From {
			get { return addressingHeaders.From; }
			set { addressingHeaders.From = value; }
		}

		public MessageID MessageID {
			get { return addressingHeaders.MessageID; }
			set { addressingHeaders.MessageID = value; }
		}

		public Recipient Recipient {
			get { return addressingHeaders.Recipient; }
			set { addressingHeaders.Recipient = value; }
		}

		public RelatesTo RelatesTo {
			get { return addressingHeaders.RelatesTo; }
			set { addressingHeaders.RelatesTo = value; }
		}

		public SoapChannel Channel {
			get { return _channel; }
			set { _channel = value; }
		}

		public bool Processed {
			get { return _processed; }
		}

		public void SetProcessed (bool to) {
			_processed = to;
		}

		public void SetTo (Uri uri) {
			addressingHeaders.To = uri;
		}

		public void SetTo (To to) {
			addressingHeaders.To = to;
		}

		public void SetActor (Uri act)
		{
			actor = act;
		}

		public void SetIsInbound (bool to)
		{
			_isInbound = to;
		}
#endif
		public Uri Actor { 
			get { return actor; }
		}

		public DimeAttachmentCollection Attachments { 
			get { 
				if (attachments == null)
					attachments = new DimeAttachmentCollection ();
				return attachments; 
			}
		}

		public string ContentType { 
			get { return contentType; }
		}

		public SoapEnvelope Envelope { 
			get { return envelope; }
		}

		public SecurityCollection ExtendedSecurity {
			get { return extendedSecurity; }
		}

		public object this [string key] { 
			get { return table [key]; }
			set { 
				if (key == null)
					throw new ArgumentNullException ("key");
				table [key] = value;
			} 
		}

		public Path Path { 
			get { return null; }
			set {;} 
		}

		public ReferralCollection Referrals { 
			get { return referrals; }
		}

		public Microsoft.Web.Services.Security.Security Security { 
			get { 
				if (security == null) {
					if (actor != null)
						security = new Microsoft.Web.Services.Security.Security (actor.ToString ());
					else
						security = new Microsoft.Web.Services.Security.Security ();
				}
				return security; 
			}
		}

		public Microsoft.Web.Services.Timestamp.Timestamp Timestamp { 
			get { return timestamp; }
		}

		internal bool IsReserved (string key) 
		{
			switch (key) {
				case "Actor":
				case "Attachments":
				case "ContentType":
				case "Envelope":
				case "ExtendedSecurity":
				case "IsInbound":
				case "IsIntermediary":
				case "Referrals":
				case "Path":
				case "Security":
				case "Timestamp":
				case "WebRequest":
				case "WebResponse":
					return true;
				default:
					return false;
			}
		}

		public void Add (string key, object value) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (IsReserved (key))
				throw new ArgumentException ("reserved key");
			table.Add (key, value);
		}

		public void Clear () 
		{
			foreach (DictionaryEntry entry in table) {
				string key = (string) entry.Key;
				// remove all except reserved names
				if (!IsReserved (key))
					table.Remove (key);
			}
		}

		public bool Contains (string key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			return table.Contains (key);
		}

		public void CopyTo (SoapContext context) 
		{
			if (context == null)
				throw new ArgumentNullException ("context");
			context.actor = this.actor;
			foreach (DimeAttachment da in Attachments) {
				context.Attachments.Add (da);
			}
			context.contentType = contentType;
			context.envelope = envelope;
			context.extendedSecurity = ExtendedSecurity;
			context.Path = Path;
			context.referrals = Referrals;
			context.security = security;
			context.timestamp = timestamp;
			foreach (DictionaryEntry de in table) {
				context.table.Add (de.Key, de.Value);
			}
		}

		public IDictionaryEnumerator GetEnumerator () 
		{
			return table.GetEnumerator ();
		}

		public void Remove (string key) 
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			if (IsReserved (key))
				throw new ArgumentException ("reserved key");
			table.Remove (key);
		}
	}
}
