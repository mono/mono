// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.UnitTesting
{
    public static class ExtendedAssert
    {
        /// <summary>
        ///     Verifies that the two specified objects are an instance of the same type.
        /// </summary>
        public static void IsInstanceOfSameType(object expected, object actual)
        {
            if (expected == null || actual == null)
            {
                Assert.AreSame(expected, actual);
                return;
            }

            Assert.AreSame(expected.GetType(), actual.GetType());
        }

        public static void ContainsLines(string value, params string[] lines)
        {
            StringReader reader = new StringReader(value);

            int count = 0;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (count == lines.Length)
                {
                    Assert.Fail();
                }

                StringAssert.Contains(line, lines[count]);

                count++;
            }

            Assert.AreEqual(lines.Length, count, "Expectation: {0}; Result: {1}", String.Join(Environment.NewLine, lines), value);
        }

        public static void EnumsContainSameValues<TEnum1, TEnum2>()
            where TEnum1 : struct
            where TEnum2 : struct
        {
            EnumsContainSameValuesCore<TEnum1, TEnum2>();
            EnumsContainSameValuesCore<TEnum2, TEnum1>();            
        }

        private static void EnumsContainSameValuesCore<TEnum1, TEnum2>()
            where TEnum1 : struct
            where TEnum2 : struct
        {
            var values = TestServices.GetEnumValues<TEnum1>();

            foreach (TEnum1 value in values)
            {
                string name1 = Enum.GetName(typeof(TEnum1), value);
                string name2 = Enum.GetName(typeof(TEnum2), value);

                Assert.AreEqual(name1, name2, "{0} contains a value that {1} does not have. These enums need to be in sync.", typeof(TEnum1), typeof(TEnum2));
            }
        }
    }
}
