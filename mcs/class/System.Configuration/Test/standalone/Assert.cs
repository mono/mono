using System;
using System.Globalization;

class Assert
{
	public static void AreEqual (string x, string y, string msg)
	{
		if (x == null && y == null)
			return;
		if ((x == null || y == null) || !x.Equals (y))
			throw new Exception (string.Format (CultureInfo.InvariantCulture,
				"Expected: {0}, but was: {1}. {2}",
				x == null ? "<null>" : x, y == null ? "<null>" : y, msg));
	}

	public static void AreEqual (object x, object y, string msg)
	{
		if (x == null && y == null)
			return;
		if ((x == null || y == null))
			throw new Exception (string.Format (CultureInfo.InvariantCulture,
				"Expected: {0}, but was: {1}. {2}",
				x == null ? "<null>" : x, y == null ? "<null>" : y, msg));

		bool isArrayX = x.GetType ().IsArray;
		bool isArrayY = y.GetType ().IsArray;

		if (isArrayX && isArrayY) {
			Array arrayX = (Array) x;
			Array arrayY = (Array) y;

			if (arrayX.Length != arrayY.Length)
				throw new Exception (string.Format (CultureInfo.InvariantCulture,
					"Length of arrays differs. Expected: {0}, but was: {1}. {2}",
					arrayX.Length, arrayY.Length, msg));

			for (int i = 0; i < arrayX.Length; i++) {
				object itemX = arrayX.GetValue (i);
				object itemY = arrayY.GetValue (i);
				if (!itemX.Equals (itemY))
					throw new Exception (string.Format (CultureInfo.InvariantCulture,
						"Arrays differ at position {0}. Expected: {1}, but was: {2}. {3}",
						i, itemX, itemY, msg));
			}
		} else if (!x.Equals (y)) {
			throw new Exception (string.Format (CultureInfo.InvariantCulture,
				"Expected: {0}, but was: {1}. {2}",
				x, y, msg));
		}
	}

	public static void Fail (string msg)
	{
		throw new Exception (msg);
	}

	public static void IsFalse (bool value, string msg)
	{
		if (value)
			throw new Exception (msg);
	}

	public static void IsTrue (bool value, string msg)
	{
		if (!value)
			throw new Exception (msg);
	}

	public static void IsNotNull (object value, string msg)
	{
		if (value == null)
			throw new Exception (msg);
	}

	public static void IsNull (object value, string msg)
	{
		if (value != null)
			throw new Exception (msg);
	}

	public static void AreSame (object x, object y, string msg)
	{
		if (!object.ReferenceEquals (x, y))
			throw new Exception (msg);
	}
}
