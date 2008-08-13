public class Test
{
	public delegate bool MemberFilter ();
	public static void FindMembers (MemberFilter filter) { }
	public static void GetMethodGroup (MemberFilter filter)
	{
		FindMembers (filter ?? delegate () {
			return true;
		});
	}

	public static void Main () { }
}

