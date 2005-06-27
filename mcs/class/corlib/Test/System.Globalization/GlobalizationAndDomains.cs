//
// System.Globalization and issues with appdomains.
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2005 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Data;
using System.Globalization;

namespace MonoTests.System.Globalization {
	[Serializable]
	[TestFixture]
	public class AppDomainsAndFormatInfo
	{
		public void Remote ()
		{
			int n = (int) Convert.ChangeType ("5", typeof (int));
			Assertion.AssertEquals ("n", 5, n);
		}

		[Test]
		public void NFIFromBug55978 ()
		{
			AppDomain domain = AppDomain.CreateDomain ("testdomain");
			AppDomainsAndFormatInfo test = new AppDomainsAndFormatInfo ();
			test.Remote ();
			domain.DoCallBack (new CrossAppDomainDelegate (test.Remote));
		}

		[Test]
		public void Bug55978 ()
		{
			DataTable dt = new DataTable ();
			dt.Columns.Add ("StartDate", typeof (DateTime));
	 
			DataRow dr;
			DateTime date = DateTime.Now;
	 
			for (int i = 0; i < 10; i++) {
				dr = dt.NewRow ();
				dr ["StartDate"] = date.AddDays (i);
				dt.Rows.Add (dr);
			}
	 
			DataView dv = dt.DefaultView;
			dv.RowFilter = "StartDate >= '" + DateTime.Now.AddDays (2) + "' and StartDate <= '" + DateTime.Now.AddDays (4) + "'";
			Assertion.AssertEquals ("Table", 10, dt.Rows.Count);
			Assertion.AssertEquals ("View", 2, dv.Count);
		}
	}
}

