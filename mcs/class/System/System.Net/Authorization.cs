//
// System.Net.Authorization.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Net {

	public class Authorization {
		string token;
		bool complete;
		
		public Authorization (string token)
		{
			this.complete = true;
			this.token = token;
		}

		public Authorization (string token, bool complete)
		{
			this.complete = complete;
			this.token = token;
		}

		public bool Complete {
			get {
				return finished;
			}
		}

		
	}
}
