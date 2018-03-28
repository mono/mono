namespace System.Numerics
{
	static class Vector
	{
		[JitIntrinsic]
		public static bool IsHardwareAccelerated {
			get {
				return false;
			}
		}
	}
}