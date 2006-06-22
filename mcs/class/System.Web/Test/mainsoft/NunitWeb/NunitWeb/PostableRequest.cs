using System;
using System.IO;

namespace MonoTests.SystemWeb.Framework
{
	[Serializable]
	public class PostableRequest:BaseRequest
	{
		bool _isPost;
		public virtual bool IsPost
		{
			get { return _isPost; }
			set { _isPost = value; }
		}

		byte[] postData;
		public virtual byte[] PostData
		{
			get { return postData; }
			set { postData = value; }
		}

		string postContentType;
		public virtual string PostContentType
		{
			get { return postContentType; }
			set { postContentType = value; }
		}

		public PostableRequest ()
			:base ()
		{
		}

		public PostableRequest (string url)
			:base (url)
		{
		}

		protected override BaseWorkerRequest CreateBaseWorkerRequest (StringWriter wr)
		{
			if (PostData == null || !IsPost)
				return base.CreateBaseWorkerRequest (wr);
			return new PostableWorkerRequest (Url, GetQueryString (),
				wr, PostData, PostContentType);
		}
	}
}
