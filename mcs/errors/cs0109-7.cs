// cs0109-7.cs: The member `Test.this[string]' does not hide an inherited member. The new keyword is not required
// Line: 9
// Compiler options: -warnaserror -warn:4

using System.Collections;

public class Test: ArrayList
{
    public new string this[string index]
    {
	set
	{
	}
    }
}
