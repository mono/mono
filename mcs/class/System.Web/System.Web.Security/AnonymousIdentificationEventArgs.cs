//
// System.Web.Security.AnonymousIdentificationEventArgs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
namespace System.Web.Security {
	public sealed class AnonymousIdentificationEventArgs : EventArgs {
		public AnonymousIdentificationEventArgs (HttpContext context)
		{
			this.context = context;
		}
		
		HttpContext context;
		public HttpContext Context {
			get { return context; }
		}
		
		string anonymousId;
		public string AnonymousId {
			get { return anonymousId; }
			set { anonymousId = value; }
		}
	}
}
#endif

