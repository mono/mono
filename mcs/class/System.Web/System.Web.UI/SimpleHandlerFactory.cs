//
// System.Web.UI.SimpleHandlerFactory
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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

using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	class SimpleHandlerFactory : IHttpHandlerFactory
	{
		public virtual IHttpHandler GetHandler (HttpContext context,
							string requestType,
							string virtualPath,
							string path)
		{
#if NET_2_0
			return BuildManager.CreateInstanceFromVirtualPath (virtualPath, typeof (IHttpHandler)) as IHttpHandler;
#else
			Type type = WebHandlerParser.GetCompiledType (context, virtualPath, path);
			if (!(typeof (IHttpHandler).IsAssignableFrom (type)))
				throw new HttpException ("Type does not implement IHttpHandler: " + type.FullName);

			return Activator.CreateInstance (type) as IHttpHandler;
#endif
		}

		public virtual void ReleaseHandler (IHttpHandler handler)
		{
		}
	}
}

