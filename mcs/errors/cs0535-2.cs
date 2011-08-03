// CS0535: `Stack' does not implement interface member `IStack.Insert(int, object)'
// Line: 11
 
using System; 
 
public interface IStack {
	void Insert (int index, object value);
	object this [int index] { set; }
}

public class Stack : IStack {
	object IStack.this [int index] {
		set {}
	}
}

public class D {
	static void Main () {}
}
