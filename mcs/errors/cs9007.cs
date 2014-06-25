// CS9007: Primary constructor parameter `value' is not available in this context when using ref or out modifier
// Line: 12

class X (ref double value)
{
	public double Prop {
		get {
			return value;
		}
	}
}

