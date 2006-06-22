using System;
using System.IO;
using System.Web;

namespace MonoTests.SystemWeb.Framework
{
	public class PostableWorkerRequest:BaseWorkerRequest
	{
		byte[] postData;
		string postContentType;

		public override String GetHttpVerbName ()
		{
			if (postData == null)
				return base.GetHttpVerbName ();
			return "POST";
		}

		public override string GetKnownRequestHeader (int index)
		{
			if (postData == null)
				return base.GetKnownRequestHeader (index);
			
			switch (index) {
			case HttpWorkerRequest.HeaderContentLength:
				return postData.Length.ToString ();
			case HttpWorkerRequest.HeaderContentType:
				return postContentType;
			default:
				return base.GetKnownRequestHeader (index);
			}
		}

		public override byte[] GetPreloadedEntityBody ()
		{
			if (postData == null)
				return base.GetPreloadedEntityBody ();
			return postData;
		}

		public PostableWorkerRequest (string page, string query, TextWriter writer, byte[] postData, string postContentType)
			: base (page, query, writer)
		{
			this.postData = postData;
			this.postContentType = postContentType;
		}
	}
}
