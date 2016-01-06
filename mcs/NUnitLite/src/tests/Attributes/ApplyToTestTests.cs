// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
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

namespace NUnit.Framework.Attributes
{
    [TestFixture]
    public class ApplyToTestTests
    {
        Test test;

        [SetUp]
        public void SetUp()
        {
            test = new TestDummy();
            test.RunState = RunState.Runnable;
        }

        #region CategoryAttribute

        [Test]
        public void CategoryAttributeSetsCategory()
        {
            new CategoryAttribute("database").ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.Category), Is.EqualTo("database"));
        }

        [Test]
        public void CategoryAttributeSetsMultipleCategories()
        {
            new CategoryAttribute("group1").ApplyToTest(test);
            new CategoryAttribute("group2").ApplyToTest(test);
            Assert.That(test.Properties[PropertyNames.Category],
                Is.EquivalentTo(new string[] { "group1", "group2" }));
        }

        #endregion

        #region DescriptionAttribute

        [Test]
        public void DescriptionAttributeSetsDescription()
        {
            new DescriptionAttribute("Cool test!").ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.Description), Is.EqualTo("Cool test!"));
        }

        #endregion

        #region IgnoreAttribute

        [Test]
        public void IgnoreAttributeIgnoresTest()
        {
            new IgnoreAttribute().ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Ignored));
        }

        [Test]
        public void IgnoreAttributeSetsIgnoreReason()
        {
            new IgnoreAttribute("BECAUSE").ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Ignored));
            Assert.That(test.Properties.Get(PropertyNames.SkipReason), Is.EqualTo("BECAUSE"));
        }

        #endregion

        #region ExplicitAttribute

        [Test]
        public void ExplicitAttributeMakesTestExplicit()
        {
            new ExplicitAttribute().ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Explicit));
        }

        [Test]
        public void ExplicitAttributeSetsIgnoreReason()
        {
            new ExplicitAttribute("BECAUSE").ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Explicit));
            Assert.That(test.Properties.Get(PropertyNames.SkipReason), Is.EqualTo("BECAUSE"));
        }

        #endregion

        #region CombinatorialAttribute

        [Test]
        public void CombinatorialAttributeSetsJoinType()
        {
            new CombinatorialAttribute().ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.JoinType), Is.EqualTo("Combinatorial"));
        }

        #endregion

        #region CultureAttribute

        [Test]
        public void CultureAttributeIncludingCurrentCultureRunsTest()
        {
            string name = System.Globalization.CultureInfo.CurrentCulture.Name;
            new CultureAttribute(name).ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Runnable));
        }

        [Test]
        public void CultureAttributeExcludingCurrentCultureSkipsTest()
        {
            string name = System.Globalization.CultureInfo.CurrentCulture.Name;
            CultureAttribute attr = new CultureAttribute(name);
            attr.Exclude = name;
            attr.ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Skipped));
            Assert.That(test.Properties.Get(PropertyNames.SkipReason),
                Is.EqualTo("Not supported under culture " + name));
        }

        [Test]
        public void CultureAttributeIncludingOtherCultureSkipsTest()
        {
            string name = "fr-FR";
            if (System.Globalization.CultureInfo.CurrentCulture.Name == name)
                name = "en-US";

            new CultureAttribute(name).ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Skipped));
            Assert.That(test.Properties.Get(PropertyNames.SkipReason),
                Is.EqualTo("Only supported under culture " + name));
        }

        [Test]
        public void CultureAttributeExcludingOtherCultureRunsTest()
        {
            string other = "fr-FR";
            if (System.Globalization.CultureInfo.CurrentCulture.Name == other)
                other = "en-US";

            CultureAttribute attr = new CultureAttribute();
            attr.Exclude = other;
            attr.ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Runnable));
        }

        [Test]
        public void CultureAttributeWithMultipleCulturesIncluded()
        {
            string current = System.Globalization.CultureInfo.CurrentCulture.Name;
            string other = current == "fr-FR" ? "en-US" : "fr-FR";
            string cultures = current + "," + "other";

            new CultureAttribute(cultures).ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Runnable));
        }

        #endregion

        #region MaxTimeAttribute

        [Test]
        public void MaxTimeAttributeSetsMaxTime()
        {
            new MaxTimeAttribute(2000).ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.MaxTime), Is.EqualTo(2000));
        }

        #endregion

        #region PairwiseAttribute

        [Test]
        public void PairwiseAttributeSetsJoinType()
        {
            new PairwiseAttribute().ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.JoinType), Is.EqualTo("Pairwise"));
        }

        #endregion

        #region PlatformAttribute

        [Test]
        public void PlatformAttributeRunsTest()
        {
            string myPlatform = System.IO.Path.DirectorySeparatorChar == '/'
                ? "Linux" : "Win";
            new PlatformAttribute(myPlatform).ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Runnable));
        }

        [Test]
        public void PlatformAttributeSkipsTest()
        {
            string notMyPlatform = System.IO.Path.DirectorySeparatorChar == '/'
                ? "Win" : "Linux";
            new PlatformAttribute(notMyPlatform).ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Skipped));
        }

        #endregion

#if !NUNITLITE

        #region RepeatAttribute

        public void RepeatAttributeSetsRepeatCount()
        {
            new RepeatAttribute(5).ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.RepeatCount), Is.EqualTo(5));
        }

        #endregion

        #region RequiredAddinAttribute

        [Test, Ignore("NYI")]
        public void RequiredAddinAttributeSkipsTest()
        {
            new RequiredAddinAttribute("JUNK").ApplyToTest(test);
            Assert.That(test.RunState, Is.EqualTo(RunState.Skipped));
        }

        #endregion

        #region RequiresMTAAttribute

        [Test]
        public void RequiresMTAAttributeSetsApartmentState()
        {
            new RequiresMTAAttribute().ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.ApartmentState),
                Is.EqualTo(System.Threading.ApartmentState.MTA));
        }

        #endregion

        #region RequiresSTAAttribute

        [Test]
        public void RequiresSTAAttributeSetsApartmentState()
        {
            new RequiresSTAAttribute().ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.ApartmentState),
                Is.EqualTo(System.Threading.ApartmentState.STA));
        }

        #endregion

        #region RequiresThreadAttribute

        [Test]
        public void RequiresThreadAttributeSetsRequiresThread()
        {
            new RequiresThreadAttribute().ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.RequiresThread), Is.EqualTo(true));
        }

        [Test]
        public void RequiresThreadAttributeMaySetApartmentState()
        {
            new RequiresThreadAttribute(System.Threading.ApartmentState.STA).ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.RequiresThread), Is.EqualTo(true));
            Assert.That(test.Properties.Get(PropertyNames.ApartmentState),
                Is.EqualTo(System.Threading.ApartmentState.STA));
        }

        #endregion

#endif

        #region SequentialAttribute

        [Test]
        public void SequentialAttributeSetsJoinType()
        {
            new SequentialAttribute().ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.JoinType), Is.EqualTo("Sequential"));
        }

        #endregion

#if !NETCF

        #region SetCultureAttribute

        public void SetCultureAttributeSetsSetCultureProperty()
        {
            new SetCultureAttribute("fr-FR").ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.SetCulture), Is.EqualTo("fr-FR"));
        }

        #endregion

        #region SetUICultureAttribute

        public void SetUICultureAttributeSetsSetUICultureProperty()
        {
            new SetUICultureAttribute("fr-FR").ApplyToTest(test);
            Assert.That(test.Properties.Get(PropertyNames.SetUICulture), Is.EqualTo("fr-FR"));
        }

        #endregion

#endif
    }
}
