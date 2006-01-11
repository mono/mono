using System;

namespace Test
{
        public class TestBit {
		const bool a00 = false & false;
		const bool a01 = false & true;
		const bool a10 = true  & false;
		const bool a11 = true  & true;

		const bool o00 = false | false;
		const bool o01 = false | true;
		const bool o10 = true  | false;
		const bool o11 = true  | true;

		const bool x00 = false ^ false;
		const bool x01 = false ^ true;
		const bool x10 = true  ^ false;
		const bool x11 = true  ^ true;

		const bool correct = !a00 & !a01 & !a10 & a11 & !o00 & o01 & o10 & o11 & !x00 & x01 & x10 & !x11;

                public static void Main()
                {
			if (!correct)
				throw new Exception ();
                }
        }
}
