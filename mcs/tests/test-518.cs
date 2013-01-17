class Foo {
	public static int Main ()
	{
		int ret = 1;
		try {
			goto done;
		} finally {
			ret = 0;
		}
	done:
		return ret;
	}
}

