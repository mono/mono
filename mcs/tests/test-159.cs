using System;
namespace A {
        public class Iface {
                void bah() {}
        }
        class my {
                A.Iface b;
                void doit (Object A) {
                        b = (A.Iface)A;
                }
                public static int Main () {
                        return 0;
                }
        }
}
