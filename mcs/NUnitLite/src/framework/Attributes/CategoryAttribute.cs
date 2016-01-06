// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
using NUnit.Framework.Internal;

namespace NUnit.Framework
{
	/// <summary>
	/// Attribute used to apply a category to a test
	/// </summary>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method|AttributeTargets.Assembly, AllowMultiple=true, Inherited=true)]
    public class CategoryAttribute : NUnitAttribute, IApplyToTest
	{
		/// <summary>
		/// The name of the category
		/// </summary>
		protected string categoryName;

        /// <summary>
        /// Construct attribute for a given category based on
        /// a name. The name may not contain the characters ',',
        /// '+', '-' or '!'. However, this is not checked in the
        /// constructor since it would cause an error to arise at
        /// as the test was loaded without giving a clear indication
        /// of where the problem is located. The error is handled
        /// in NUnitFramework.cs by marking the test as not
        /// runnable.
        /// </summary>
        /// <param name="name">The name of the category</param>
        public CategoryAttribute(string name)
        {
            this.categoryName = name.Trim();
        }

		/// <summary>
		/// Protected constructor uses the Type name as the name
		/// of the category.
		/// </summary>
		protected CategoryAttribute()
		{
			this.categoryName = this.GetType().Name;
			if ( categoryName.EndsWith( "Attribute" ) )
				categoryName = categoryName.Substring( 0, categoryName.Length - 9 );
		}

		/// <summary>
		/// The name of the category
		/// </summary>
		public string Name 
		{
			get { return categoryName; }
		}

        #region IApplyToTest Members

        /// <summary>
        /// Modifies a test by adding a category to it.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            test.Properties.Add(PropertyNames.Category, this.Name);

            if (this.Name.IndexOfAny(new char[] { ',', '!', '+', '-' }) >= 0)
            {
                test.RunState = RunState.NotRunnable;
                test.Properties.Set(PropertyNames.SkipReason, "Category name must not contain ',', '!', '+' or '-'");
            }
        }

        #endregion
    }
}
