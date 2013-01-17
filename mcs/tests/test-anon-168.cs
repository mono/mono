using System;

class Program
{
	public static void Main ()
	{
		Test test = new Test ();

		test.Run ((i) => {
			switch (i) {
			case 0:
				return 0;

			case 1:
				return 1;

			default:
				break;
			}

			throw new Exception ("Unknow value");
		});

		test.Run ((i) => {
			switch (i) {
			case 0:
				return 0;

			case 1:
				return 1;

			default:
				throw new Exception ("Unknow value");
			}
		});

		test.Run ((i) => {
			switch (i) {
			case 0:
				return 0;

			case 1:
				return 1;

			default:
				return 8;
			}
		});

	}
};


class Test
{
	public delegate int RunDelegate (int val);

	public void Run (RunDelegate test)
	{
		test (0);
	}
}
