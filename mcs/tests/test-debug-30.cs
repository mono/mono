class PragmaNewLinesParsing
{
#pragma warning disable RECS0029
#pragma warning restore RECS0029

	void Foo ()
	{
		return;
	}

#pragma warning disable RECS0029 // here
#pragma warning restore RECS0029 /* end */

	public static void Main ()
	{
		return;
	}

#pragma warning disable 618
#pragma warning restore 618

	void OneMore ()
	{
		return;
	}
}