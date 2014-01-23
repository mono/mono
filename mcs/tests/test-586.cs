class Program
{
	public static int Main ()
	{
		MyColor [] c = new MyColor [1];
		c [0] += new MyColor (1.3F);
		c [0] += new MyColor (1.5F);
		if (c [0].Value != 2.8F)
			return 1;
		return 0;
	}

	public struct MyColor
	{
		private float _value;

		public MyColor (float value)
		{
			_value = value;
		}

		public float Value
		{
			get { return _value; }
		}

		public static MyColor operator + (MyColor a, MyColor b)
		{
			return new MyColor (a._value + b._value);
		}
	}
}
