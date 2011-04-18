// CS0659: `Test.Test' overrides Object.Equals(object) but does not override Object.GetHashCode()
// Line: 7
// Compiler options: -warnaserror -warn:3

namespace Test{  
    public partial class Test{  
	public override bool Equals(object obj){  
	    return true;  
	}  
	  
	static void Main () {}
    }  
}
