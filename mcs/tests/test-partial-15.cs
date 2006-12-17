namespace Foo {
        public partial class X
        {
                public static void Main () {}
        }
}

namespace Foo {
        using System;
        using System.Collections;

        public partial class X
        {
                public static IEnumerable Attempts2()
                {
                        AttributeTargets t = AttributeTargets.All;
                        yield break;
                }

                public static IEnumerable Attempts {
                        get {
                                AttributeTargets t = AttributeTargets.All;
                                yield break;
                        }
                }
                
                public IEnumerable this [int i] {
                        get {
                                AttributeTargets t = AttributeTargets.All;
                                yield break;
                        }
                }
        }
}