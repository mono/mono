//
// CaseSensitivityTests.cs
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
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NDESK_OPTIONS
using NDesk.Options;
#else
using Mono.Options;
#endif

using NUnit.Framework;

#if NDESK_OPTIONS
namespace Tests.NDesk.Options
#else
namespace MonoTests.Mono.Options
#endif
{
    [TestFixture]
    public class CaseSensitivityTests
    {
        [Test]
        public void Default_Case_Match ()
        {
            string argValue = null;

            var opts = new OptionSet ()
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MiXeDcAsE=Arg Value",
            };

            opts.Parse (args);

            Assert.AreEqual ("Arg Value", argValue);
        }

        [Test]
        public void Default_Case_MisMatch ()
        {
            string argValue = null;

            var opts = new OptionSet ()
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MixedCase=Arg Value",
            };

            opts.Parse (args);

            Assert.Null (argValue);
        }

        [Test]
        public void CaseSensitive_Case_Match ()
        {
            string argValue = null;

            var opts = new OptionSet (StringComparer.Ordinal)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MiXeDcAsE=Arg Value",
            };

            opts.Parse (args);

            Assert.AreEqual ("Arg Value", argValue);
        }

        [Test]
        public void CaseSensitive_Case_MisMatch ()
        {
            string argValue = null;

            var opts = new OptionSet (StringComparer.Ordinal)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MixedCase=Arg Value",
            };

            opts.Parse (args);

            Assert.Null (argValue);
        }

        [Test]
        public void CaseInsensitive_Case_Match ()
        {
            string argValue = null;

            var opts = new OptionSet (StringComparer.OrdinalIgnoreCase)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MiXeDcAsE=Arg Value",
            };

            opts.Parse (args);

            Assert.AreEqual ("Arg Value", argValue);
        }

        [Test]
        public void CaseInsensitive_Case_MisMatch ()
        {
            string argValue = null;

            var opts = new OptionSet (StringComparer.OrdinalIgnoreCase)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MixedCase=Arg Value",
            };

            opts.Parse (args);

            Assert.AreEqual ("Arg Value", argValue);
        }
    }
}
