// CS0121: The call is ambiguous between the following methods or properties: `V2.operator -(V2, V2)' and `V3.operator -(V3, V3)'
// Line: 45

public struct V3
{
	public float x, y, z;

	public V3 (float ix, float iy, float iz) { x = ix; y = iy; z = iz; }

	static public V3 operator - (V3 lhs, V3 rhs)
	{
		return new V3 (lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
	}
}

public struct V2
{
	public float x, y;

	public V2 (float ix, float iy) { x = ix; y = iy; }

	public static implicit operator V2 (V3 v)
	{
		return new V2 (v.x, v.y);
	}

	public static implicit operator V3 (V2 v)
	{
		return new V3 (v.x, v.y, 0);
	}

	static public V2 operator - (V2 lhs, V2 rhs)
	{
		return new V2 (lhs.x - rhs.x, lhs.y - rhs.y);
	}
}

internal class Test
{
	static void Main ()
	{
		V2 a = new V2 ();
		V3 b = new V3 ();

		V2 s = a - b;
	}
}
