// cs0039.cs: as operator can not convert explicitly from type to type
// line: 8
class A {
        public static void Main ()
        {
		decimal tryDec;
		tryDec = 1234.2345M;

		object a = tryDec as string;
        }
}





