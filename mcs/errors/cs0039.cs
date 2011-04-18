// CS0039: Cannot convert type `decimal' to `string' via a built-in conversion
// Line: 8
class A {
        public static void Main ()
        {
		decimal tryDec;
		tryDec = 1234.2345M;

		object a = tryDec as string;
        }
}





