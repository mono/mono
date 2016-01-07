// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal
{
	/// <summary>
	/// Interface to be implemented by filters applied to tests.
	/// The filter applies when running the test, after it has been
	/// loaded, since this is the only time an ITest exists.
	/// </summary>
	[Serializable]
	public abstract class TestFilter : ITestFilter
	{
		/// <summary>
		/// Unique Empty filter.
		/// </summary>
        public static TestFilter Empty = new EmptyFilter();

        /// <summary>
        /// Indicates whether this is the EmptyFilter
        /// </summary>
        public bool IsEmpty
        {
            get { return this is TestFilter.EmptyFilter; }
        }

		/// <summary>
		/// Determine if a particular test passes the filter criteria. The default 
		/// implementation checks the test itself, its parents and any descendants.
		/// 
		/// Derived classes may override this method or any of the Match methods
		/// to change the behavior of the filter.
		/// </summary>
		/// <param name="test">The test to which the filter is applied</param>
		/// <returns>True if the test passes the filter, otherwise false</returns>
		public virtual bool Pass( ITest test )
		{
			return Match(test) || MatchParent(test) || MatchDescendant(test);
		}

		/// <summary>
		/// Determine whether the test itself matches the filter criteria, without
		/// examining either parents or descendants.
		/// </summary>
		/// <param name="test">The test to which the filter is applied</param>
		/// <returns>True if the filter matches the any parent of the test</returns>
		public abstract bool Match(ITest test);

		/// <summary>
		/// Determine whether any ancestor of the test matches the filter criteria
		/// </summary>
		/// <param name="test">The test to which the filter is applied</param>
		/// <returns>True if the filter matches the an ancestor of the test</returns>
		protected virtual bool MatchParent(ITest test)
		{
            return (test.RunState != RunState.Explicit && test.Parent != null &&
                (Match(test.Parent) || MatchParent(test.Parent)));
		}

		/// <summary>
		/// Determine whether any descendant of the test matches the filter criteria.
		/// </summary>
		/// <param name="test">The test to be matched</param>
		/// <returns>True if at least one descendant matches the filter criteria</returns>
		protected virtual bool MatchDescendant(ITest test)
		{
            if (test.Tests == null)
                return false;

            foreach (ITest child in test.Tests)
            {
                if (Match(child) || MatchDescendant(child))
                    return true;
            }

            return false;
		}

#if !NUNITLITE
        public static TestFilter FromXml(string xmlText)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlText);
            XmlNode topNode = doc.FirstChild;

            if (topNode.Name != "filter")
                throw new Exception("Expected filter element at top level");

            // Initially, an empty filter
            TestFilter result = TestFilter.Empty;
            bool isEmptyResult = true;

            XmlNodeList testNodes = topNode.SelectNodes("tests/test");
            XmlNodeList includeNodes = topNode.SelectNodes("include/category");
            XmlNodeList excludeNodes = topNode.SelectNodes("exclude/category");

            if (testNodes.Count > 0)
            {
                SimpleNameFilter nameFilter = new SimpleNameFilter();
                foreach (XmlNode testNode in topNode.SelectNodes("tests/test"))
                    nameFilter.Add(testNode.InnerText);

                result = nameFilter;
                isEmptyResult = false;
            }

            if (includeNodes.Count > 0)
            {
                //CategoryFilter includeFilter = new CategoryFilter();
                //foreach (XmlNode includeNode in includeNodes)
                //    includeFilter.AddCategory(includeNode.InnerText);

                // Temporarily just look at the first element
                XmlNode includeNode = includeNodes[0];
                TestFilter includeFilter = new CategoryExpression(includeNode.InnerText).Filter;

                if (isEmptyResult)
                    result = includeFilter;
                else
                    result = new AndFilter(result, includeFilter);
                isEmptyResult = false;
            }

            if (excludeNodes.Count > 0)
            {
                CategoryFilter categoryFilter = new CategoryFilter();
                foreach (XmlNode excludeNode in excludeNodes)
                    categoryFilter.AddCategory(excludeNode.InnerText);
                TestFilter excludeFilter = new NotFilter(categoryFilter);

                if (isEmptyResult)
                    result = excludeFilter;
                else
                    result = new AndFilter(result, excludeFilter);
                isEmptyResult = false;
            }

            return result;
        }
#endif

		/// <summary>
		/// Nested class provides an empty filter - one that always
		/// returns true when called, unless the test is marked explicit.
		/// </summary>
		[Serializable]
		private class EmptyFilter : TestFilter
		{
			public override bool Match( ITest test )
			{
				return test.RunState != RunState.Explicit;
			}

			public override bool Pass( ITest test )
			{
				return test.RunState != RunState.Explicit;
			}
		}
	}
}
