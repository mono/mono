using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class TestDataComparer : IComparer<TestData>
{
	bool byID;
	bool ascending;

	public TestDataComparer (bool byID, bool ascending)
	{
		this.byID = byID;
		this.ascending = ascending;
	}

	public int Compare (TestData x, TestData y)
	{
		if (x == null) {
			if (y == null)
				return 0;
			else
				return -1;
		}

		if (y == null)
			return 1;

		if (byID) {
			int xid = Int32.Parse (x.ProductID);
			int yid = Int32.Parse (y.ProductID);

			return ascending ? xid - yid : yid - xid;
		} else {
			int ret = String.Compare (x.ProductName, y.ProductName);
			return ascending ? ret : -ret;
		}
	}
}