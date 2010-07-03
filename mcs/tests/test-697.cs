// Compiler options: -codepage:utf8

// Tests ignorance of UTF-8 header in the middle of the file
// ->|
using ﻿ System\uFEFF;

public class C
{
	public static void Main ()
	{
	}
}
