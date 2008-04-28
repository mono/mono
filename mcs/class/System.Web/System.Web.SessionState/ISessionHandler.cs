//
// System.Web.SessionState.ISessionHandler
//
// Authors:
//	Stefan Görling, (stefan@gorling.se)
//
// (C) 2003 Stefan Görling
//
// This interface is simple, but as it's internal it shouldn't be hard to change it if we need to.
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

using System.Web.Configuration;

namespace System.Web.SessionState
{
	internal interface ISessionHandler
	{
		void Dispose ();
		void Init (SessionStateModule module, HttpApplication context,
#if NET_2_0
			   SessionStateSection config
#else
			   SessionConfig config
#endif
			   );
		HttpSessionState UpdateContext (HttpContext context, SessionStateModule module, bool required,
						bool read_only, ref bool isNew);

		void UpdateHandler (HttpContext context, SessionStateModule module);
		void Touch (string sessionId, int timeout);
	}
}

