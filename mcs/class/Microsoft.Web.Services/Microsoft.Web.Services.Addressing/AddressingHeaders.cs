//
// Microsoft.Web.Services.Addressing.AddressingHeaders.cs
//
// Author: Todd Berman <tberman@gentoo.org>
//
// (C) 2003 Todd Berman

using System;

namespace Microsoft.Web.Services.Addressing
{

	public class AddressingHeaders
	{

		private Action _action;
		private ReplyTo _replyTo;
		private To _to;

		[MonoTODO]
		public AddressingHeaders ()
		{
			throw new NotImplementedException ();
		}

		public Action Action {
			get { return _action; }
			set { _action = value; }
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
