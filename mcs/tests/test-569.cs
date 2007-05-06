using System;

[AttributeUsage(AttributeTargets.All, AllowMultiple=true)]
public class MyAttribute : Attribute {}

public class SubAttribute : MyAttribute {}

public class test {
	[SubAttribute]
	[SubAttribute]
	public void method() {}

	public static void Main (){}
}
