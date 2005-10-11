using System;

public enum ArrowType {
	Up,
	Down,
	Left,
	Right,
}

public struct Value {
	public Value (object obj) {
	}
	
	public object Val {
		get {
			return ArrowType.Left;
		}
	}
	
	public Enum Val2 {
		get {
			return ArrowType.Down;
		}
	}
}
	
public class Valtest {
	public static int Main () {
		Value val;
		ArrowType i = (ArrowType)val.Val2;
		
		if ((ArrowType)(Enum)val.Val != ArrowType.Left)
			return 1;

		Console.WriteLine ("OK");
		return 0;
	}
}

