//
// System.Runtime.Remoting.Activation.UrlAttribute.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Activation {

	[Serializable]
	public sealed class UrlAttribute : ContextAttribute
	{
		string url;
		
		public UrlAttribute (string callsiteURL)
			: base (callsiteURL)
		{
			url = callsiteURL;
		}

		public string UrlValue {
			get { return url; }
		}

		public override bool Equals (object o)
		{
			if (!(o is UrlAttribute))
				return false;
			
			return (((UrlAttribute) o).UrlValue == url);
		}

		public override int GetHashCode ()
		{
			return url.GetHashCode ();
		}

		public override void GetPropertiesForNewContext (IConstructionCallMessage ctorMsg)
		{
			// No new properties
		}

		public override bool IsContextOK (Context ctx, IConstructionCallMessage msg)
		{
			return true;
		}
	}
}
