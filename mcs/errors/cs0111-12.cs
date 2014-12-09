// CS0111: A member `S3.S3(string)' is already defined. Rename this member or use different parameter types
// Line: 6
// Compiler options: -langversion:experimental

struct S3 (string s)
{
	public S3 (string s)
		: this (1)
	{
	}

	public S3 (int i)
		: this ("")
	{
	}
}
