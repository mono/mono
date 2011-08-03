// CS3018: `NotCompliant.Compliant' cannot be marked as CLS-compliant because it is a member of non CLS-compliant type `NotCompliant'
// Line: 12
// Compiler options: -warnaserror -warn:1

using System;
[assembly: CLSCompliant (true)]

[CLSCompliant (false)]
public class NotCompliant
{
		[CLSCompliant (true)]
		public class Compliant
		{
		}
}
