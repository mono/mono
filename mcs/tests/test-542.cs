//
// See bug 78113
//	

public struct ARec
	{
		decimal mVal;
		public ARec(decimal val)
		{
			mVal = Round(val, 1); 
		}

		decimal Round(int digits)
		{
			return Round(mVal, digits);
		}

		static decimal Round(decimal val, int digits)
		{
			return 0;
		}
	}

class X {
public static void Main () {
}
}
