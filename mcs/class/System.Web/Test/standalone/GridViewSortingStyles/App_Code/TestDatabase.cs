using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class TestDatabase
{
	List <TestData> data = new List<TestData> {
		new TestData { ProductID="1", ProductName="Pear"},
		new TestData { ProductID="2", ProductName="Apple"},
		new TestData { ProductID="3", ProductName="Orange"}
	};

	public List<TestData> GetData (string sortExpression)
	{
		if (String.IsNullOrEmpty (sortExpression))
			return data;

		bool ascending;
		string field;
		int subtract;

		if (sortExpression.EndsWith (" desc", StringComparison.OrdinalIgnoreCase)) {
			ascending = false;
			subtract = 5;
		} else {
			ascending = true;
			subtract = 0;
		}

		if (subtract == 0)
			field = sortExpression;
		else
			field = sortExpression.Substring (0, sortExpression.Length - subtract);

		TestDataComparer comparer;
		if (String.Compare (field, "ProductName", StringComparison.OrdinalIgnoreCase) == 0)
			comparer = new TestDataComparer (false, ascending);
		else
			comparer = new TestDataComparer (true, ascending);

		data.Sort (comparer);

		return data;
	}
}