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

		byte[] entityBody;
		public virtual byte[] EntityBody
		{
			get { return entityBody; }
			set { entityBody = value; }
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
			if (EntityBody == null || !IsPost)
				return base.CreateBaseWorkerRequest (wr);
			return new PostableWorkerRequest (Url, GetQueryString (),
				wr, UserAgent, EntityBody, PostContentType);
		}
	}
}
