// cs3018.cs: 'NotCompliant.Compliant' cannot be marked as CLS-Compliant because it is a member of non CLS-Compliant type 'NotCompliant'
// Line: 9

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
