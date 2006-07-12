using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Contains the HTTP response data after executing
	/// <seealso cref="WebTest.Run"/>
	/// </summary>
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
			internal set { _body = value; }
		}
	}
}
