using System;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for EmptyFilter.
	/// </summary>
	public class EmptyFilter : Filter
	{
		#region IFilter Members

		public override bool Pass(TestSuite suite)
		{
			return true;
		}

		public override bool Pass(TestCase test)
		{
			return true;
		}

		#endregion

		public static EmptyFilter Empty 
		{
			get { return new EmptyFilter(); }
		}
	}
}
