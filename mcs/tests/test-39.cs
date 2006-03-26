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
		
	public long LongValue {
		get {
			return 0;
		}
		set { }
	}
	
	public long[] ArrayValue {
		get {
			return new long[0];
		}
		set { }
	}
	
	public object D;
}

[Simple ("Interface test")]
public interface IFoo {
        void MethodOne (int x, int y);
        bool MethodTwo (float x, float y);
}

[Simple ("Dummy", MyNamedArg = "Dude!")]
[Simple ("Vids", MyNamedArg = "Raj", AnotherArg = "Foo")]
[Simple ("Trip", LongValue=0)]
[Simple ("Fourth", ArrayValue=new long[] { 0 })]
//[Simple ("Fifth", D=new double[] { -1 })] // runtime bug #77916
public class Blah {

        public static int Main ()
        {
                Console.WriteLine ("A dummy app which tests attribute emission");
                return 0;
        }
}
