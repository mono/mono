using System;
[AttributeUsage (AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
public class SimpleAttribute : Attribute {
        
        string name = null;
        
        public string MyNamedArg;
        
        private string secret;
        
        public SimpleAttribute (string name)
        {
                this.name = name;
        }
        
        public string AnotherArg {
                get {
                        return secret;
                }
                set {
                        secret = value;
                }
        }
        
}

[Simple ("Interface test")]
public interface IFoo {
        void MethodOne (int x, int y);
        bool MethodTwo (float x, float y);
}

[Simple ("Dummy", MyNamedArg = "Dude!")]
[Simple ("Vids", MyNamedArg = "Raj", AnotherArg = "Foo")]	
public class Blah {

        public static int Main ()
        {
                Console.WriteLine ("A dummy app which tests attribute emission");
                return 0;
        }
}

	
