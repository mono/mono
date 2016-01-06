using System;
using System.Collections;
using NUnit.Framework;

namespace NUnit.TestUtilities
{
    public class UniqueValues
    {
        public static int Count(IEnumerable actual)
        {
            NUnit.ObjectList list = new NUnit.ObjectList();

            foreach (object o1 in actual)
                if (!list.Contains(o1))
                    list.Add(o1);

            return list.Count;
        }

        public static void Check(IEnumerable values, int minExpected)
        {
            int count = Count(values);
            Assert.That(count, Is.Not.EqualTo(1), "All values were the same!");
            // TODO: Change to an actual warning once we implement them
            Assert.That(count, Is.GreaterThanOrEqualTo(minExpected), "WARNING: The number of unique values less than expected.");
        }

        #region Self-test

        [TestCase(1, 2, 3, 4, 5, ExpectedResult = 5)]
        [TestCase(1, 2, 3, 4, 3, ExpectedResult = 4)]
        [TestCase(1, 1, 1, 1, 1, ExpectedResult = 1)]
        [TestCase(1, 2, 1, 2, 1, ExpectedResult = 2)]
        [TestCase(1, 1, 1, 2, 2, ExpectedResult = 2)]
#if !NET_1_1
        [TestCase(ExpectedResult = 0)]
#endif
        public static int CountUniqueValuesTest(params int[] values)
        {
            return UniqueValues.Count(values);
        }

        #endregion
    }
}
