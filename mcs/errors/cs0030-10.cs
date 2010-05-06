// CS0030: Cannot convert type `TestCase.MyEnum' to `TestCase.OSType'
// Line: 9

public class TestCase
{
	static void Main ()
	{
		MyEnum me = MyEnum.Value1;
		OSType os = (OSType)me;
	}

	struct OSType {
		int value;
		
		public int Value {
			get { return Value; }
		}

		public OSType (int value)
		{
			this.value = value;
		}

		public static implicit operator OSType (int i)
		{
			return new OSType (i);
		}
	}

	enum MyEnum {
		Value1
	}
}
