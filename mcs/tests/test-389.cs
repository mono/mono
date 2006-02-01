// This is used to debug an ordering dependent bug.
//
// Compiler options: support-389.cs -out:test-389.exe

using System;
using System.Collections;
using System.Reflection;

namespace Schemas
{
	public partial class basefieldtype
	{
		public virtual object Instantiate () { return null; }
	}

	public partial class fieldtype
	{
		public override object Instantiate ()
		{
			Console.WriteLine ("Instantiating type '{0}'", id);
			return null;
		}
	}

	public partial class compoundfield
	{
		public override object Instantiate ()
		{
			Console.WriteLine ("Instantiating compound field '{0}'", id);
			return null;
		}
	}

	public partial class field
	{
		public object Instantiate ()
		{
			Console.WriteLine ("Instantiating field '{0}'", id);
			return null;
		}
	}
	
	public partial class formdata
	{
		public object Instantiate ()
		{
			Console.WriteLine ("Instantiating form window");
			return null;
		}
	}

	public class M {
	    public static void Main () {}
	}
}
