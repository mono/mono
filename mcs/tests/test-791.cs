using System;
using System.Collections;

namespace testApp
{
	public interface IA
	{
		bool GetEnumerator ();
	}

	public interface IC : IA, IEnumerable
	{
	}

	public class TestApp : IC
	{
		public static int Main ()
		{
			IC ic = new TestApp ();
			foreach (int v in ic) {
			}

			return 0;
		}

		#region IA Members

		public bool GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new int[0].GetEnumerator ();
		}

		#endregion
	}
}

