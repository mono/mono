// CS0111: A member `N.MyClass.N.IMyInterface.MyEvent' is already defined. Rename this member or use different parameter types
// Line: 18

namespace N
{
	interface IMyInterface
	{
		bool MyEvent { set; }
	}

	public class MyClass : IMyInterface
	{
		bool IMyInterface.MyEvent
		{
			set { }
		}

		bool N.IMyInterface.MyEvent
		{
			set { }
		}
	}
}
