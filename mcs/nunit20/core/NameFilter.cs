using System;
using System.Collections;

namespace NUnit.Core
{
	/// <summary>
	/// Summary description for NameFilter.
	/// </summary>
	/// 
	[Serializable]
	public class NameFilter : Filter
	{
		private ArrayList testNodes;

		public NameFilter(Test node)
		{
			testNodes = new ArrayList();
			testNodes.Add(node);
		}

		public NameFilter(ArrayList nodes) 
		{
			testNodes = nodes;
		}

		public override bool Pass(TestSuite suite) 
		{
			bool passed = Exclude;

			foreach (Test node in testNodes) 
			{
				if (suite.IsDescendant(node) || node.IsDescendant(suite) || node == suite) 
				{
					passed = !Exclude;
					break;
				}
			}

			return passed;
		}

		public override bool Pass(TestCase test) 
		{
			bool passed = Exclude;

			foreach(Test node in testNodes) 
			{
				if (test.IsDescendant(node) || test == node) 
				{
					passed = !Exclude;
					break;
				}
			}

			return passed;
		}
	}
}
