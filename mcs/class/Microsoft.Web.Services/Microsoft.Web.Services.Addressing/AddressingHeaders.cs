//
// Microsoft.Web.Services.Addressing.AddressingHeaders.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;
using System.Xml;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Addressing
{

	public class AddressingHeaders
	{

		private Action _action;
		private ReplyTo _replyTo;
		private To _to;
		private FaultTo _faultTo;
		private From _from;
		private MessageID _messageID;
		private Recipient _recipient;
		private RelatesTo _relatesTo;

		public AddressingHeaders (SoapEnvelope env)
		{
			Load (env);
		}

		public AddressingHeaders ()
		{
			Defaults ();
		}

		public void Clear ()
		{
			_action = null;
			_faultTo = null;
			_from = null;
			_messageID = null;
			_recipient = null;
			_relatesTo = null;
			_replyTo = null;
			_to = null;
		}

		public AddressingHeaders Clone ()
		{
			AddressingHeaders clone = new AddressingHeaders ();

			clone.Action = _action;
			clone.FaultTo = _faultTo;
			clone.From = _from;
			clone.MessageID = _messageID;
			clone.Recipient = _recipient;
			clone.RelatesTo = _relatesTo;
			clone.ReplyTo = _replyTo;
			clone.To = _to;

			return clone;
		}

		public void Defaults ()
		{
			_action = null;
			_faultTo = null;
			_from = new From (new Uri ("http://schemas.xmlsoap.org/ws/2003/03/addressing/role/anonymous") );
			_messageID = new MessageID ();
			_recipient = null;
			_relatesTo = null;
			_replyTo = null;
			_to = null;
		}

		public void Load (SoapEnvelope env)
		{
			if(env == null) {
				throw new ArgumentNullException ("envelope");
			}
			Clear ();
			if(env.Header == null) {
				return;
			}
			int item = 0;
			while (item < env.Header.ChildNodes.Count) {
				XmlElement element = (XmlElement) env.Header.ChildNodes[item];
				if(element != null || element.NamespaceURI != "http://schemas.xmlsoap.org/ws/2003/03/addressing") {

					switch (element.LocalName) {
						case "Action":
							if(_action != null) {
								throw new AddressingFormatException ("Two or more Actions detected");
							}
							_action = new Action (element);
							break;
						case "FaultTo":
							if(_faultTo != null) {
								throw new AddressingFormatException ("Two or more FaultTos detected");
							}
							_faultTo = new FaultTo (element);
							break;
						case "From":
							if(_from != null) {
								throw new AddressingFormatException ("Two or more Froms detected");
							}
							_from = new From (element);
							break;
						case "MessageID":
							if(_messageID != null) {
								throw new AddressingFormatException ("Two or more MessageIDs detected");
							}
							_messageID = new MessageID (element);
							break;
						case "Recipient":
							if(_recipient != null) {
								throw new AddressingFormatException ("Two or more Recipients detected");
							}
							_recipient = new Recipient (element);
							break;
						case "RelatesTo":
							if(_relatesTo != null) {
								throw new AddressingFormatException ("Two or more RelatesTos detected");
							}
							_relatesTo = new RelatesTo (element);
							break;
						case "ReplyTo":
							if(_replyTo != null) {
								throw new AddressingFormatException ("Two or more ReplyTos detected");
							}
							_replyTo = new ReplyTo (element);
							break;
						case "To":
							if(_to != null) {
								throw new AddressingFormatException ("Two or more Tos detected");
							}
							_to = new To (element);
							break;
					}
					
				}
				item++;
			}
		}

		public Action Action {
			get { return _action; }
			set { _action = value; }
		}

		public FaultTo FaultTo {
			get { return _faultTo; }
			set { _faultTo = value; }
		}

		public From From {
			get { return _from; }
			set { _from = value; }
		}

		public MessageID MessageID {
			get { return _messageID; }
			set { _messageID = value; }
		}

		public Recipient Recipient {
			get { return _recipient; }
			set { _recipient = value; }
		}

		public RelatesTo RelatesTo {
			get { return _relatesTo; }
			set { _relatesTo = value; }
		}

		public ReplyTo ReplyTo {
			get { return _replyTo; }
			set { _replyTo = value; }
		}

		public To To {
			get { return _to; }
			set { _to = value; }
		}

	}

}
