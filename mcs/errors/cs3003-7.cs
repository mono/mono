// CS3003: Type of `C._data' is not CLS-compliant
// Line: 11
// Compiler options: -unsafe -warnaserror -warn:1

using System;

[assembly: CLSCompliant (true)]

public class C
{
	public unsafe byte* _data;
	public unsafe byte* GetData () { return _data; }
}
