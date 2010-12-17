// Compiler options: -r:gtest-417-lib.dll

using System;
using System.Collections;

class Indirect : Base
{
}

abstract class Base : IEnumerable
{
	IEnumerator IEnumerable.GetEnumerator ()
	{
		return new int [0].GetEnumerator ();
	}
}

public class TestCase
{
	public static GlobalMonitoredCharacterCollection MonitoredCharacters;
	
	static int Main ()
	{
		MonitoredCharacters = new GlobalMonitoredCharacterCollection();
		foreach (var character in MonitoredCharacters)
		{
		}
		
		foreach (var n in new Indirect ())
		{
		}
		
		return 0;
	}
}
