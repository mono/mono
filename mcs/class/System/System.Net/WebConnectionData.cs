//
// System.Net.WebConnectionData
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

using System.IO;

namespace System.Net
{
	class WebConnectionData
	{
		public HttpWebRequest request;
		public int StatusCode;
		public string StatusDescription;
		public WebHeaderCollection Headers;
		public Version Version;
		public WebConnectionStream stream;
		public byte [] initialData;
		public int initialOffset;
		public int length;

		public void Init ()
		{
			request = null;
			StatusCode = -1;
			StatusDescription = null;
			Headers = null;
			stream = null;
			initialData = null;
			initialOffset = 0;
			length = 0;
		}
	}
}

