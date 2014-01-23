using System;
using System.Reflection;
// test bug bug#26264

interface IA {
        void doh();
}
interface IB {

        IA Prop {get;}
}
class A : IA {
        public void doh() {}
}
class T : IB {
        IA IB.Prop {
                get { return new A(); }
        }
        public A Prop {
                get { return new A(); }
        }
        public static int Main() {
		PropertyInfo[] p = typeof (T).GetProperties (BindingFlags.Public| BindingFlags.NonPublic|BindingFlags.Instance);
		if (p == null || p.Length != 2)
			return 1;
                return 0;
        }
}

