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
        public void Default_Case_Match()
        {
            string argValue = null;

            var opts = new OptionSet()
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MiXeDcAsE=Arg Value",
            };

            opts.Parse(args);

            Assert.AreEqual("Arg Value", argValue);
        }

        [Test]
        public void Default_Case_MisMatch()
        {
            string argValue = null;

            var opts = new OptionSet()
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MixedCase=Arg Value",
            };

            opts.Parse(args);

            Assert.Null(argValue);
        }

        [Test]
        public void CaseSensitive_Case_Match()
        {
            string argValue = null;

            var opts = new OptionSet(StringComparer.Ordinal)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MiXeDcAsE=Arg Value",
            };

            opts.Parse(args);

            Assert.AreEqual("Arg Value", argValue);
        }

        [Test]
        public void CaseSensitive_Case_MisMatch()
        {
            string argValue = null;

            var opts = new OptionSet(StringComparer.Ordinal)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MixedCase=Arg Value",
            };

            opts.Parse(args);

            Assert.Null(argValue);
        }

        [Test]
        public void CaseInsensitive_Case_Match()
        {
            string argValue = null;

            var opts = new OptionSet(StringComparer.OrdinalIgnoreCase)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MiXeDcAsE=Arg Value",
            };

            opts.Parse(args);

            Assert.AreEqual("Arg Value", argValue);
        }

        [Test]
        public void CaseInsensitive_Case_MisMatch()
        {
            string argValue = null;

            var opts = new OptionSet(StringComparer.OrdinalIgnoreCase)
            {
                {"MiXeDcAsE=", "arg desc", v => argValue = v}
            };

            var args = new string[]
            {
                "/MixedCase=Arg Value",
            };

            opts.Parse(args);

            Assert.AreEqual("Arg Value", argValue);
        }
    }
}
