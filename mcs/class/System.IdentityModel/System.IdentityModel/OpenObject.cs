using System;
using System.Collections.Generic;

namespace System.IdentityModel
{
	public abstract class OpenObject
	{
		private Dictionary<string, object> properties = new Dictionary<string, object> ();

		public Dictionary<string, object> Properties { get { return properties; } }
	}
}