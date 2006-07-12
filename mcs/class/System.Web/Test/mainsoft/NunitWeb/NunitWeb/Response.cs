using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Contains the HTTP response data after executing
	/// <see cref="WebTest.Run"/>
	/// </summary>
	/// <seealso cref="WebTest.Run"/>
	[Serializable]
	public class Response
	{
		string _body;
		/// <summary>
		/// Get the response body.
		/// </summary>
		public string Body
		{
			get { return _body; }
#if NET_2_0
			internal
#endif
			set { _body = value; }
		}
	}
}
