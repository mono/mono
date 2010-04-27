using System;

public delegate void D ();

public class A
{
		[Obsolete ("Please use ...", true)]
		protected event D comparer {
			add {
			}
			remove {
			}
		}
}