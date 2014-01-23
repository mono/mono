//
// This comes from bug 82064, sadly, Mono does not currently abort
// as it should on the extra value on the stack
//
using System;
using System.IO;

class Program
{
	public static void Main (string [] args)
	{
		using (StringWriter stringWriter = new StringWriter ()) {
		}
	}
}
