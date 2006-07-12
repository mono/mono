using System;

namespace MonoTests.SystemWeb.Framework
{
	/// <summary>
	/// Contains constants 
	/// </summary>
	public class StandardUrl
	{
		/// <summary>
		/// Fake page URL which maps to the custom invoker. Used together with
		/// <seealso cref="HandlerInvoker"/>
		/// </summary>
		public const string FAKE_PAGE = "page.fake";
		/// <summary>
		/// An empty page for generic usage.
		/// </summary>
		public const string EMPTY_PAGE = "MyPage.aspx";
		/// <summary>
		/// An empty page, referencing a master page.
		/// </summary>
		public const string PAGE_WITH_MASTER = "MyPageWithMaster.aspx";
	}
}
