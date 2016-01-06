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
using System.Globalization;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

namespace NUnit.Framework
{
    /// <summary>
    /// CultureAttribute is used to mark a test fixture or an
    /// individual method as applying to a particular Culture only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly, AllowMultiple = false, Inherited=false)]
    public class CultureAttribute : IncludeExcludeAttribute, IApplyToTest
    {
        private CultureDetector cultureDetector = new CultureDetector();
        private CultureInfo currentCulture = CultureInfo.CurrentCulture;

        /// <summary>
        /// Constructor with no cultures specified, for use
        /// with named property syntax.
        /// </summary>
        public CultureAttribute() { }

        /// <summary>
        /// Constructor taking one or more cultures
        /// </summary>
        /// <param name="cultures">Comma-deliminted list of cultures</param>
        public CultureAttribute(string cultures) : base(cultures) { }

        #region IApplyToTest members

        /// <summary>
        /// Causes a test to be skipped if this CultureAttribute is not satisfied.
        /// </summary>
        /// <param name="test">The test to modify</param>
        public void ApplyToTest(Test test)
        {
            if (test.RunState != RunState.NotRunnable && !IsCultureSupported())
            {
                test.RunState = RunState.Skipped;
                test.Properties.Set(PropertyNames.SkipReason, Reason);
            }
        }

        #endregion

        /// <summary>
        /// Tests to determine if the current culture is supported
        /// based on the properties of this attribute.
        /// </summary>
        /// <returns>True, if the current culture is supported</returns>
        private bool IsCultureSupported()
        {
            if (Include != null && !cultureDetector.IsCultureSupported(Include))
            {
                Reason = string.Format("Only supported under culture {0}", Include);
                return false;
            }

            if (Exclude != null && cultureDetector.IsCultureSupported(Exclude))
            {
                Reason = string.Format("Not supported under culture {0}", Exclude);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Test to determine if the a particular culture or comma-
        /// delimited set of cultures is in use.
        /// </summary>
        /// <param name="culture">Name of the culture or comma-separated list of culture names</param>
        /// <returns>True if the culture is in use on the system</returns>
        public bool IsCultureSupported(string culture)
        {
            culture = culture.Trim();

            if (culture.IndexOf(',') >= 0)
            {
                if (IsCultureSupported(culture.Split(new char[] { ',' })))
                    return true;
            }
            else
            {
                if (currentCulture.Name == culture || currentCulture.TwoLetterISOLanguageName == culture)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Test to determine if one of a collection of culturess
        /// is being used currently.
        /// </summary>
        /// <param name="cultures"></param>
        /// <returns></returns>
        public bool IsCultureSupported(string[] cultures)
        {
            foreach (string culture in cultures)
                if (IsCultureSupported(culture))
                    return true;

            return false;
        }
    }
}
