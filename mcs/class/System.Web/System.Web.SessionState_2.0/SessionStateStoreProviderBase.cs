//
// System.Web.Compilation.SessionStateItemCollection
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
#if NET_2_0
using System.Configuration.Provider;
using System.Web;

namespace System.Web.SessionState 
{
	public abstract class SessionStateStoreProviderBase : ProviderBase
	{
		protected SessionStateStoreProviderBase ()
		{
		}

		public abstract SessionStateStoreData CreateNewStoreData (HttpContext context, int timeout);
		public abstract void CreateUninitializedItem (HttpContext context, string id, int timeout);
		public abstract void Dispose ();
		public abstract void EndRequest (HttpContext context);
		public abstract SessionStateStoreData GetItem (HttpContext context,
							       string id,
							       out bool locked,
							       out TimeSpan lockAge,
							       out Object lockId,
							       out SessionStateActions actions);
		public abstract SessionStateStoreData GetItemExclusive (HttpContext context,
									string id,
									out bool locked,
									out TimeSpan lockAge,
									out Object lockId,
									out SessionStateActions actions);
		public abstract void InitializeRequest (HttpContext context);
		public abstract void ReleaseItemExclusive (HttpContext context,
							   string id,
							   Object lockId);
		public abstract void RemoveItem (HttpContext context,
						 string id,
						 Object lockId,
						 SessionStateStoreData item);
		public abstract void ResetItemTimeout (HttpContext context, string id);
		public abstract void SetAndReleaseItemExclusive (HttpContext context,
								 string id,
								 SessionStateStoreData item,
								 Object lockId,
								 bool newItem);
		public abstract bool SetItemExpireCallback (SessionStateItemExpireCallback expireCallback);
	}
}
#endif
