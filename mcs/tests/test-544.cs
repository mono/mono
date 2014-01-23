enum ByteEnum : byte {
	One = 1,
	Two = 2
}

class X {
	public static void Main ()
	{
		ByteEnum b = ByteEnum.One;
		
		switch (b){
		case ByteEnum.One : return;
		case ByteEnum.One | ByteEnum.Two: return;
		}
	}
}

