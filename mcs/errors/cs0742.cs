// CS0742: Unexpected symbol `;'. A query body must end with select or group clause
// Line: 10

using System.Linq;

class C
{
	public static void Main ()
    {
        var q = from i in "abcd" where i;
    }
}
