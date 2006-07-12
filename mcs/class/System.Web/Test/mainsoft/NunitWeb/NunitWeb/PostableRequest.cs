using System;
using System.IO;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Adds the postback functionality to <seealso cref="BaseRequest"/>.
	/// Provides pretty low-level interface. Consider using <seealso cref="FormRequest"/>
	/// in user code.
	/// </summary>
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
		/// Create a <seealso cref="PostableWorkerRequest"/> if <seealso cref="IsPost"/>
		/// is true or <seealso cref="EntityBody"/> is not null. Otherwise, call the
		/// base method.
		/// </summary>
		/// <param name="wr">The text writer that is passed to the
		/// <seealso cref="BaseWorkerRequest"/> constructor.</param>
		/// <returns>A new <seealso cref="BaseWorkerRequest"/> instance.</returns>
		protected override BaseWorkerRequest CreateBaseWorkerRequest (TextWriter wr)
		{
			if (EntityBody == null || !IsPost)
				return base.CreateBaseWorkerRequest (wr);
			return new PostableWorkerRequest (Url, QueryString,
				wr, UserAgent, EntityBody, ContentType);
		}
	}
}
