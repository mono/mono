using System;
using System.IO;
using System.Net;
using System.Collections.Specialized;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Adds the postback functionality to <see cref="BaseRequest"/>.
	/// Provides pretty low-level interface. Consider using <seealso cref="FormRequest"/>
	/// in user code.
	/// </summary>
	/// <seealso cref="BaseRequest"/>
	[Serializable]
	public class PostableRequest:BaseRequest
	{
		/// <summary>
		/// Default constructor. Does nothing.
		/// </summary>
		public PostableRequest ()
			: base ()
		{
		}

		/// <summary>
		/// Create an instance of <see cref="PostableRequest"/>,
		/// calling the base constructor with given <paramref name="url"/>.
		/// </summary>
		/// <param name="url">The URL for the request.</param>
		public PostableRequest (string url)
			: base (url)
		{
		}

		bool _isPost;
		/// <summary>
		/// Get or set the HTTP method. If <see cref="IsPost"/> is true,
		/// the request will be done with <c>POST</c> HTTP method, otherwise with <c>GET</c>.
		/// </summary>
		public virtual bool IsPost
		{
			get { return _isPost; }
			set { _isPost = value; }
		}

		byte[] entityBody;
		/// <summary>
		/// Get or set the HTTP <c>entity-body</c>.
		/// </summary>
		public virtual byte[] EntityBody
		{
			get { return entityBody; }
			set { entityBody = value; }
		}

		string postContentType;
		/// <summary>
		/// Get or set the HTTP <c>content-type</c>.
		/// </summary>
		public virtual string ContentType
		{
			get { return postContentType; }
			set { postContentType = value; }
		}

		/// <summary>
		/// Create a <see cref="PostableWorkerRequest"/> if <see cref="IsPost"/>
		/// is true or <see cref="EntityBody"/> is not null. Otherwise, call the
		/// base method.
		/// </summary>
		/// <param name="wr">The text writer that is passed to the
		/// <see cref="BaseWorkerRequest"/> constructor.</param>
		/// <returns>A new <see cref="BaseWorkerRequest"/> instance.</returns>
		/// <seealso cref="PostableWorkerRequest"/>
		/// <seealso cref="IsPost"/>
		/// <seealso cref="EntityBody"/>
		/// <seealso cref="BaseWorkerRequest"/>
		protected override BaseWorkerRequest CreateBaseWorkerRequest (TextWriter wr)
		{
			if (EntityBody == null || !IsPost)
				return base.CreateBaseWorkerRequest (wr);
			return new PostableWorkerRequest (Url, QueryString,
				wr, UserAgent, EntityBody, ContentType);
		}

		/// <summary>
		/// Override the base <see cref="CreateWebRequest"/> and add POST method functionality
		/// when necessary.
		/// </summary>
		/// <param name="baseUri">URI to send request to.</param>
		/// <param name="headers">Headers added to the request.</param>
		/// <returns>A new <see cref="WebRequest"/></returns>
		public override WebRequest CreateWebRequest(Uri baseUri, NameValueCollection headers)
		{
			//FIXME: may be it's better to override CreateHttpWebRequest?
			HttpWebRequest hwr = base.CreateHttpWebRequest (baseUri, headers);
			if (EntityBody == null || !IsPost)
				return hwr;
			hwr.ContentLength = EntityBody.Length;
			hwr.Method = "POST";
			hwr.ContentType = ContentType;
			using (Stream s = hwr.GetRequestStream ()) {
				s.Write (EntityBody, 0, EntityBody.Length);
			}
			return hwr;
		}

	}
}
