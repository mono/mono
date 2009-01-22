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
		/// <see cref="HandlerInvoker"/>
		/// </summary>
		/// <seealso cref="HandlerInvoker"/>
		public const string FAKE_PAGE = "My.ashx";
		/// <summary>
		/// An empty page for generic usage.
		/// </summary>
		public const string EMPTY_PAGE = "MyPage.aspx";
		/// <summary>
		/// An empty page, referencing a master page.
		/// </summary>
		public const string PAGE_WITH_MASTER = "MyPageWithMaster.aspx";
		/// <summary>
		/// An empty page, referencing a master page which references another master page.
		/// </summary>
		public const string PAGE_WITH_DERIVED_MASTER = "MyPageWithDerivedMaster.aspx";
		/// <summary>
		/// A page referencing a master page which tries to use a non-existing content place
		/// holder
		/// </summary>
		public const string PAGE_WITH_MASTER_INVALID_PLACE_HOLDER = "MyPageWithMasterInvalidPlaceHolder.aspx";
	}
}
