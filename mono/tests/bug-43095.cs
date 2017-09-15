using System;
using System.Runtime.CompilerServices;

class C {
	public static void Main () {
		/* depends on running with --debug */
		try {
			Bug43095.ThrowArgumentNullException ("sup");
		} catch (Exception ex) {
			string expected_offset = "[0x0000b]";
			string[] frames = ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

			if (!frames [0].Contains (expected_offset))
				throw new Exception (String.Format ("Exception should be thrown at IL offset {0}, but was: {1}", expected_offset, frames [0]));
		}
	}
}

