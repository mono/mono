//
// System.Net.HttpWebRequestCreator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Net
{
	class HttpRequestCreator : IWebRequestCreate
	{
		public WebRequest Create (Uri uri)
		{
			return new HttpWebRequest (uri);
		}
	}
}

