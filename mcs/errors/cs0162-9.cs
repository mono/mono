// CS0162: Unreachable code detected
// Line: 9
// Compiler options: -warnaserror -warn:2

class Error
{
	void Test ()
	{
		if (1 == 0) {
			try {
			} catch (System.Net.Sockets.SocketException sex) {
				int x = (int)sex.SocketErrorCode;
			}
		}
	}

}