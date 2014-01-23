using System;
using System.Diagnostics;


class BaseClass
{
        [Conditional ("AAXXAA")]
        public virtual void ConditionalMethod ()
        {
            Environment.Exit (1);
        }
}

class TestClass: BaseClass
{
        public override void ConditionalMethod ()
        {
            base.ConditionalMethod ();
        }
}

class MainClass
{
        public static int Main()
        {
            TestClass ts = new TestClass ();
            ts.ConditionalMethod ();
            Console.WriteLine ("Succeeded");
            return 0;
        }
}
