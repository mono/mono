//
// System.Net.FileWebRequestCreator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//

namespace System.Net
{
	class FileWebRequestCreator : IWebRequestCreate
	{
		public WebRequest Create (Uri uri)
		{
			return new FileWebRequest (uri);
		}
	}
}

