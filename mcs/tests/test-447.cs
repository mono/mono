using System;

[assembly:CLSCompliant(true)]

namespace System {
	[AttributeUsage (AttributeTargets.All, AllowMultiple=true)]
	internal sealed class MonoTODOAttribute : Attribute {

		string comment;
		
		public MonoTODOAttribute ()
		{
		}
	}

}

namespace System.Web
{
	public partial class HttpBrowserCapabilities {

		[MonoTODO] public Version A {
			get { throw new Exception (); }
		}
	}
}

class Test { public static void Main () { } }
