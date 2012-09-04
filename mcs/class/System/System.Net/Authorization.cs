//
// System.Net.Authorization.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lawrence Pit (loz@cable.a2000.nl)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace System.Net {

	public class Authorization {
		string token;
		bool complete;
		string connectionGroupId;
		string [] protectionRealm;
		IAuthenticationModule module;
		
		public Authorization (string token) : this (token, true)
		{
		}

		public Authorization (string token, bool finished) 
			: this (token, finished, null)
		{
		}
		
		public Authorization (string token, bool finished, string connectionGroupId)
		{
			this.token = token;
			this.complete = finished;
			this.connectionGroupId = connectionGroupId;
		}

		public string Message {
			get { return token; }
		}

		public bool Complete {
			get { return complete; }
		}

		public string ConnectionGroupId {
			get { return connectionGroupId; }
		}	
		
		public string[] ProtectionRealm {
			get { return protectionRealm; }
			set { protectionRealm = value; }
		}

		internal IAuthenticationModule Module {
			get { return module; }
			set { module = value; }
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool MutuallyAuthenticated
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
	}
}
