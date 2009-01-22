using System;
using System.IO;
using System.Web;

namespace MonoTests.SystemWeb.Framework
{
	internal class PostableWorkerRequest:BaseWorkerRequest
	{
		byte[] entityBody;
		string postContentType;

		public override String GetHttpVerbName ()
		{
			if (entityBody == null)
				return base.GetHttpVerbName ();
			return "POST";
		}

		public override string GetKnownRequestHeader (int index)
		{
			if (entityBody == null)
				return base.GetKnownRequestHeader (index);
			
			switch (index) {
			case HttpWorkerRequest.HeaderContentLength:
				return entityBody.Length.ToString ();
			case HttpWorkerRequest.HeaderContentType:
				return postContentType;
			default:
				return base.GetKnownRequestHeader (index);
			}
		}

		public override byte[] GetPreloadedEntityBody ()
		{
			if (entityBody == null)
				return base.GetPreloadedEntityBody ();
			return entityBody;
		}

		public PostableWorkerRequest (string page, string query, TextWriter writer,
			string userAgent, byte[] entityBody, string postContentType)
			: base (page, query, writer, userAgent)
		{
			this.entityBody = entityBody;
			this.postContentType = postContentType;
		}
	}
}
