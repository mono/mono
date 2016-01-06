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
	/// Attribute used to mark a test that is to be ignored.
	/// Ignored tests result in a warning message when the
	/// tests are run.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method|AttributeTargets.Class|AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
    public class IgnoreAttribute : NUnitAttribute, IApplyToTest
	{
		private string reason;

		/// <summary>
		/// Constructs the attribute without giving a reason 
		/// for ignoring the test.
		/// </summary>
		public IgnoreAttribute()
		{
			this.reason = "";
		}

		/// <summary>
		/// Constructs the attribute giving a reason for ignoring the test
		/// </summary>
		/// <param name="reason">The reason for ignoring the test</param>
		public IgnoreAttribute(string reason)
		{
			this.reason = reason;
        }

        #region IApplyToTest members

        /// <summary>
        /// Modifies a test by marking it as Ignored.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            if (test.RunState != RunState.NotRunnable)
            {
                test.RunState = RunState.Ignored;
                test.Properties.Set(PropertyNames.SkipReason, reason);
            }
        }

        #endregion
    }
}
