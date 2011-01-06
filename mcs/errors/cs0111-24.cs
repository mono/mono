// CS0111: A member `MyClass.IMyInterface<System.String>.Prop' is already defined. Rename this member or use different parameter types
// Line: 18

using System;

interface IMyInterface<T>
{
	bool Prop { set; }
}

public class MyClass: IMyInterface<string>
{
	bool IMyInterface<string>.Prop
	{
		set {}
	}

	bool IMyInterface<System.String>.Prop
	{
		set {}
	}
}
