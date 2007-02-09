// CS1644: Feature `switch expression of boolean type' cannot be used because it is not part of the C# 1.0 language specification
// Line: 8
// Compiler options: -langversion:ISO-1

class Class {
	public void Foo (bool b)
	{
		switch (b)
		{
			case true:
				break;
			case false:
				break;
		}
	}
}
