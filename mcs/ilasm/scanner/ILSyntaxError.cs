// ILSyntaxError.cs
// Author: Sergey Chaban (serge@wildwestsoftware.com)

using System;

namespace Mono.ILASM {

	public class ILSyntaxError : Exception {
		private Location loc = Location.Unknown;

		public ILSyntaxError () : base ()
		{
		}

		public ILSyntaxError (string msg) : base (msg)
		{
		}


		public ILSyntaxError (string msg, Location loc) : base (msg)
		{
			this.loc = loc.Clone () as Location;
		}
	}
}

