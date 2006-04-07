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

[Simple ("Fifth", D=new double[] { -1 })]
class Blah2
{
}

[Simple ("Fifth", D=new double[0])]
class Blah3
{
}

[Simple ("Dummy", MyNamedArg = "Dude!")]
[Simple ("Vids", MyNamedArg = "Raj", AnotherArg = "Foo")]
[Simple ("Trip", LongValue=0)]
[Simple ("Fourth", ArrayValue=new long[] { 0 })]
public class Blah {

        public static int Main ()
        {
				object o = (((SimpleAttribute)typeof(Blah2).GetCustomAttributes (typeof (SimpleAttribute), false)[0]).D);
				if (o.ToString () != "System.Double[]")
					return 1;

				if (((double[])o)[0].GetType () != typeof (double))
					return 2;

				o = (((SimpleAttribute)typeof(Blah3).GetCustomAttributes (typeof (SimpleAttribute), false)[0]).D);
				if (o.ToString () != "System.Double[]")
					return 3;
				
				Console.WriteLine ("OK");
                return 0;
        }
}
