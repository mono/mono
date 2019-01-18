namespace System
{
	partial class Array
	{
		// Will require JIT tweaks to skip constants
		internal const int MaxArrayLength = 0X7FEFFFFF;
		internal const int MaxByteArrayLength = 0x7FFFFFC7;
	}
}