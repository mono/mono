// CS0278: `Testing.IMixedEnumerable' contains ambiguous implementation of `enumerable' pattern. Method `System.Collections.IEnumerable.GetEnumerator()' is ambiguous with method `Testing.ICustomEnumerable.GetEnumerator()'
// Line: 28
// Compiler options: -warnaserror -warn:2

using System;
using System.Collections;

namespace Testing {
        interface ICustomEnumerable {
                IEnumerator GetEnumerator();
        }

        interface IMixedEnumerable : IEnumerable, ICustomEnumerable {}

        class TestCollection : IMixedEnumerable {
                IEnumerator IEnumerable.GetEnumerator() {
                        return null;
                }

                IEnumerator ICustomEnumerable.GetEnumerator()  {
                        return null;
                }
        }

        class Test {
                public static void Main(string[] args) {
                        IMixedEnumerable c = new TestCollection();
                        foreach(object o in c) {}
                }
        }
}
