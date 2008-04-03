// CS0217: A user-defined operator `UserOperatorClass.operator &(UserOperatorClass, bool)' must have parameters and return values of the same type in order to be applicable as a short circuit operator
// Line: 25

public class UserOperatorClass
{
	public static UserOperatorClass operator & (UserOperatorClass u1, bool u2)
	{
		return u1;
	}

	public static bool operator true (UserOperatorClass u)
	{
		return true;
	}

	public static bool operator false (UserOperatorClass u)
	{
		return false;
	}

	public static void Main ()
	{
		UserOperatorClass x = new UserOperatorClass ();
		bool y = true;
		UserOperatorClass z = x && y;
	}
}


