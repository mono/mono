using System;
using System.Numerics;
using System.Runtime.CompilerServices;

/*
 * Tests for the SIMD intrinsics in the System.Numerics.Vectors assembly.
 */
public class VectorTests {

#if !MOBILE
	public static int Main (string[] args) {
		return TestDriver.RunTests (typeof (VectorTests), args);
	}
#endif

	//
	// Vector2 tests
	//

	public static int test_0_vector2_ctor_1 () {
		var v = new Vector2 (1.0f);

		if (v.X != 1.0f)
			return 1;
		if (v.Y != 1.0f)
			return 2;
		return 0;
	}

	public static int test_0_vector2_ctor_2 () {
		var v = new Vector2 (1.0f, 2.0f);

		if (v.X != 1.0f)
			return 1;
		if (v.Y != 2.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static bool vector2_equals (Vector2 v1, Vector2 v2) {
		// cmpeqps+pmovmskb
		return v1.Equals (v2);
	}

	public static int test_0_vector2_equals () {
		var v1 = new Vector2 (1.0f, 2.0f);
		var v2 = new Vector2 (2.0f, 2.0f);

		if (vector2_equals (v1, v2))
			return 1;
		if (!vector2_equals (v1, v1))
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static float vector2_dot (Vector2 v1, Vector2 v2) {
		return Vector2.Dot (v1, v2);
	}

	public static int test_0_vector2_dot () {
		var v1 = new Vector2 (1.0f, 1.0f);
		var v2 = new Vector2 (2.0f, 2.0f);

		float f = vector2_dot (v1, v2);
		if (f != 4.0f)
			return 1;
		f = vector2_dot (v1, v1);
		if (f != 2.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_min (Vector2 v1, Vector2 v2) {
		return Vector2.Min (v1, v2);
	}

	public static int test_0_vector2_min () {
		var v1 = new Vector2 (1.0f, 1.0f);
		var v2 = new Vector2 (2.0f, 2.0f);

		var v3 = vector2_min (v1, v2);
		if (v3.X != 1.0f || v3.Y != 1.0f)
			return 1;
		v3 = vector2_min (v2, v2);
		if (v3.X != 2.0f || v3.Y != 2.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_max (Vector2 v1, Vector2 v2) {
		return Vector2.Max (v1, v2);
	}

	public static int test_0_vector2_max () {
		var v1 = new Vector2 (1.0f, 1.0f);
		var v2 = new Vector2 (2.0f, 2.0f);

		var v3 = vector2_max (v1, v2);
		if (v3.X != 2.0f || v3.Y != 2.0f)
			return 1;
		v3 = vector2_min (v1, v1);
		if (v3.X != 1.0f || v3.Y != 1.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_abs (Vector2 v1) {
		return Vector2.Abs (v1);
	}

	public static int test_0_vector2_abs () {
		var v1 = new Vector2 (-1.0f, -2.0f);
		var v2 = new Vector2 (1.0f, 2.0f);

		var v3 = vector2_abs (v1);
		if (v3.X != 1.0f || v3.Y != 2.0f)
			return 1;
		v3 = vector2_abs (v2);
		if (v3.X != 1.0f || v3.Y != 2.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_sqrt (Vector2 v1) {
		return Vector2.SquareRoot (v1);
	}

	public static int test_0_vector2_sqrt () {
		var v1 = new Vector2 (1.0f, 0.0f);

		var v3 = vector2_sqrt (v1);
		if (v3.X != 1.0f || v3.Y != 0.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_add (Vector2 v1, Vector2 v2) {
		return v1 + v2;
	}

	public static int test_0_vector2_add () {
		var v1 = new Vector2 (1.0f, 2.0f);
		var v2 = new Vector2 (3.0f, 4.0f);

		var v3 = vector2_add (v1, v2);
		if (v3.X != 4.0f || v3.Y != 6.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_sub (Vector2 v1, Vector2 v2) {
		return v1 - v2;
	}

	public static int test_0_vector2_sub () {
		var v1 = new Vector2 (1.0f, 2.0f);
		var v2 = new Vector2 (3.0f, 5.0f);

		var v3 = vector2_sub (v2, v1);
		if (v3.X != 2.0f || v3.Y != 3.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_mul (Vector2 v1, Vector2 v2) {
		return v1 * v2;
	}

	public static int test_0_vector2_mul () {
		var v1 = new Vector2 (1.0f, 2.0f);
		var v2 = new Vector2 (3.0f, 5.0f);

		var v3 = vector2_mul (v2, v1);
		if (v3.X != 3.0f || v3.Y != 10.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_mul_left (float v1, Vector2 v2) {
		return v1 * v2;
	}

	public static int test_0_vector2_mul_left () {
		var v1 = new Vector2 (3.0f, 5.0f);

		var v3 = vector2_mul_left (2.0f, v1);
		if (v3.X != 6.0f || v3.Y != 10.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_mul_right (Vector2 v1, float v2) {
		return v1 * v2;
	}

	public static int test_0_vector2_mul_right () {
		var v1 = new Vector2 (3.0f, 5.0f);

		var v3 = vector2_mul_right (v1, 2.0f);
		if (v3.X != 6.0f || v3.Y != 10.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_div (Vector2 v1, Vector2 v2) {
		return v1 / v2;
	}

	public static int test_0_vector2_div () {
		var v1 = new Vector2 (9.0f, 10.0f);
		var v2 = new Vector2 (3.0f, 5.0f);

		var v3 = vector2_div (v1, v2);
		if (v3.X != 3.0f || v3.Y != 2.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector2 vector2_div_right (Vector2 v1, float v2) {
		return v1 / v2;
	}

	public static int test_0_vector2_div_right () {
		var v1 = new Vector2 (9.0f, 15.0f);

		var v3 = vector2_div_right (v1, 3.0f);
		if (v3.X != 3.0f || v3.Y != 5.0f)
			return 1;
		return 0;
	}

	//
	// Vector4 tests
	//

	public static int test_0_vector4_ctor_1 () {
		var v = new Vector4 (1.0f);

		if (v.X != 1.0f)
			return 1;
		if (v.Y != 1.0f)
			return 2;
		if (v.Z != 1.0f)
			return 3;
		if (v.W != 1.0f)
			return 4;
		return 0;
	}

	public static int test_0_vector4_ctor_2 () {
		var v = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);

		if (v.X != 1.0f)
			return 1;
		if (v.Y != 2.0f)
			return 2;
		if (v.Z != 3.0f)
			return 3;
		if (v.W != 4.0f)
			return 4;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static bool vector4_equals (Vector4 v1, Vector4 v2) {
		// cmpeqps+pmovmskb
		return v1.Equals (v2);
	}

	public static int test_0_vector4_equals () {
		var v1 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);
		var v2 = new Vector4 (2.0f, 2.0f, 2.0f, 2.0f);

		if (vector4_equals (v1, v2))
			return 1;
		if (!vector4_equals (v1, v1))
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static float vector4_dot (Vector4 v1, Vector4 v2) {
		return Vector4.Dot (v1, v2);
	}

	public static int test_0_vector4_dot () {
		var v1 = new Vector4 (1.0f, 1.0f, 1.0f, 1.0f);
		var v2 = new Vector4 (2.0f, 2.0f, 2.0f, 2.0f);

		float f = vector4_dot (v1, v2);
		if (f != 8.0f)
			return 1;
		f = vector4_dot (v1, v1);
		if (f != 4.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_min (Vector4 v1, Vector4 v2) {
		return Vector4.Min (v1, v2);
	}

	public static int test_0_vector4_min () {
		var v1 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);
		var v2 = new Vector4 (5.0f, 6.0f, 7.0f, 8.0f);

		var v3 = vector4_min (v1, v2);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f || v3.W != 4.0f)
			return 1;
		v3 = vector4_min (v2, v2);
		if (v3.X != 5.0f || v3.Y != 6.0f || v3.Z != 7.0f || v3.W != 8.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_max (Vector4 v1, Vector4 v2) {
		return Vector4.Max (v1, v2);
	}

	public static int test_0_vector4_max () {
		var v1 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);
		var v2 = new Vector4 (5.0f, 6.0f, 7.0f, 8.0f);

		var v3 = vector4_max (v1, v2);
		if (v3.X != 5.0f || v3.Y != 6.0f || v3.Z != 7.0f || v3.W != 8.0f)
			return 1;
		v3 = vector4_max (v1, v1);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f || v3.W != 4.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_abs (Vector4 v1) {
		return Vector4.Abs (v1);
	}

	public static int test_0_vector4_abs () {
		var v1 = new Vector4 (-1.0f, -2.0f, -3.0f, -4.0f);
		var v2 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);

		var v3 = vector4_abs (v1);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f || v3.W != 4.0f)
			return 1;
		v3 = vector4_abs (v2);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f || v3.W != 4.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_sqrt (Vector4 v1) {
		return Vector4.SquareRoot (v1);
	}

	public static int test_0_vector4_sqrt () {
		var v1 = new Vector4 (1.0f, 0.0f, 1.0f, 0.0f);

		var v3 = vector4_sqrt (v1);
		if (v3.X != 1.0f || v3.Y != 0.0f || v3.Z != 1.0f || v3.W != 0.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_add (Vector4 v1, Vector4 v2) {
		return v1 + v2;
	}

	public static int test_0_vector4_add () {
		var v1 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);
		var v2 = new Vector4 (5.0f, 6.0f, 7.0f, 8.0f);

		var v3 = vector4_add (v1, v2);
		if (v3.X != 6.0f || v3.Y != 8.0f || v3.Z != 10.0f || v3.W != 12.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_sub (Vector4 v1, Vector4 v2) {
		return v1 - v2;
	}

	public static int test_0_vector4_sub () {
		var v1 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);
		var v2 = new Vector4 (3.0f, 5.0f, 7.0f, 9.0f);

		var v3 = vector4_sub (v2, v1);
		if (v3.X != 2.0f || v3.Y != 3.0f || v3.Z != 4.0f || v3.W != 5.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_mul (Vector4 v1, Vector4 v2) {
		return v1 * v2;
	}

	public static int test_0_vector4_mul () {
		var v1 = new Vector4 (1.0f, 2.0f, 3.0f, 4.0f);
		var v2 = new Vector4 (3.0f, 5.0f, 6.0f, 7.0f);

		var v3 = vector4_mul (v2, v1);
		if (v3.X != 3.0f || v3.Y != 10.0f || v3.Z != 18.0f || v3.W != 28.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_mul_left (float v1, Vector4 v2) {
		return v1 * v2;
	}

	public static int test_0_vector4_mul_left () {
		var v1 = new Vector4 (3.0f, 5.0f, 6.0f, 7.0f);

		var v3 = vector4_mul_left (2.0f, v1);
		if (v3.X != 6.0f || v3.Y != 10.0f || v3.Z != 12.0f || v3.W != 14.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_mul_right (Vector4 v1, float v2) {
		return v1 * v2;
	}

	public static int test_0_vector4_mul_right () {
		var v1 = new Vector4 (3.0f, 5.0f, 6.0f, 7.0f);

		var v3 = vector4_mul_right (v1, 2.0f);
		if (v3.X != 6.0f || v3.Y != 10.0f || v3.Z != 12.0f || v3.W != 14.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_div (Vector4 v1, Vector4 v2) {
		return v1 / v2;
	}

	public static int test_0_vector4_div () {
		var v1 = new Vector4 (9.0f, 10.0f, 12.0f, 21.0f);
		var v2 = new Vector4 (3.0f, 5.0f, 6.0f, 7.0f);

		var v3 = vector4_div (v1, v2);
		if (v3.X != 3.0f || v3.Y != 2.0f || v3.Z != 2.0f || v3.W != 3.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector4 vector4_div_right (Vector4 v1, float v2) {
		return v1 / v2;
	}

	public static int test_0_vector4_div_right () {
		var v1 = new Vector4 (9.0f, 15.0f, 21.0f, 30.0f);

		var v3 = vector4_div_right (v1, 3.0f);
		if (v3.X != 3.0f || v3.Y != 5.0f || v3.Z != 7.0f || v3.W != 10.0f)
			return 1;
		return 0;
	}

	public static int test_0_vector4_length () {
		var v = new Vector4 (2.0f, 2.0f, 2.0f, 2.0f);
		return v.Length () == 4.0f ? 0 : 1;
	}

	//
	// Vector3 tests
	//

	public static int test_0_vector3_ctor_1 () {
		var v = new Vector3 (1.0f);

		if (v.X != 1.0f)
			return 1;
		if (v.Y != 1.0f)
			return 2;
		if (v.Z != 1.0f)
			return 3;
		return 0;
	}

	public static int test_0_vector3_ctor_2 () {
		var v = new Vector3 (1.0f, 2.0f, 3.0f);

		if (v.X != 1.0f)
			return 1;
		if (v.Y != 2.0f)
			return 2;
		if (v.Z != 3.0f)
			return 3;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static bool vector3_equals (Vector3 v1, Vector3 v2) {
		// cmpeqps+pmovmskb
		return v1.Equals (v2);
	}

	public static int test_0_vector3_equals () {
		var v1 = new Vector3 (1.0f, 2.0f, 3.0f);
		var v2 = new Vector3 (2.0f, 2.0f, 2.0f);

		if (vector3_equals (v1, v2))
			return 1;
		if (!vector3_equals (v1, v1))
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static float vector3_dot (Vector3 v1, Vector3 v2) {
		return Vector3.Dot (v1, v2);
	}

	public static int test_0_vector3_dot () {
		var v1 = new Vector3 (1.0f, 1.0f, 1.0f);
		var v2 = new Vector3 (2.0f, 2.0f, 2.0f);

		float f = vector3_dot (v1, v2);
		if (f != 6.0f)
			return 1;
		f = vector3_dot (v1, v1);
		if (f != 3.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_min (Vector3 v1, Vector3 v2) {
		return Vector3.Min (v1, v2);
	}

	public static int test_0_vector3_min () {
		var v1 = new Vector3 (1.0f, 2.0f, 3.0f);
		var v2 = new Vector3 (5.0f, 6.0f, 7.0f);

		var v3 = vector3_min (v1, v2);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f)
			return 1;
		v3 = vector3_min (v2, v2);
		if (v3.X != 5.0f || v3.Y != 6.0f || v3.Z != 7.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_max (Vector3 v1, Vector3 v2) {
		return Vector3.Max (v1, v2);
	}

	public static int test_0_vector3_max () {
		var v1 = new Vector3 (1.0f, 2.0f, 3.0f);
		var v2 = new Vector3 (5.0f, 6.0f, 7.0f);

		var v3 = vector3_max (v1, v2);
		if (v3.X != 5.0f || v3.Y != 6.0f || v3.Z != 7.0f)
			return 1;
		v3 = vector3_max (v1, v1);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_abs (Vector3 v1) {
		return Vector3.Abs (v1);
	}

	public static int test_0_vector3_abs () {
		var v1 = new Vector3 (-1.0f, -2.0f, -3.0f);
		var v2 = new Vector3 (1.0f, 2.0f, 3.0f);

		var v3 = vector3_abs (v1);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f)
			return 1;
		v3 = vector3_abs (v2);
		if (v3.X != 1.0f || v3.Y != 2.0f || v3.Z != 3.0f)
			return 2;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_sqrt (Vector3 v1) {
		return Vector3.SquareRoot (v1);
	}

	public static int test_0_vector3_sqrt () {
		var v1 = new Vector3 (1.0f, 0.0f, 1.0f);

		var v3 = vector3_sqrt (v1);
		if (v3.X != 1.0f || v3.Y != 0.0f || v3.Z != 1.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_add (Vector3 v1, Vector3 v2) {
		return v1 + v2;
	}

	public static int test_0_vector3_add () {
		var v1 = new Vector3 (1.0f, 2.0f, 3.0f);
		var v2 = new Vector3 (5.0f, 6.0f, 7.0f);

		var v3 = vector3_add (v1, v2);
		if (v3.X != 6.0f || v3.Y != 8.0f || v3.Z != 10.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_sub (Vector3 v1, Vector3 v2) {
		return v1 - v2;
	}

	public static int test_0_vector3_sub () {
		var v1 = new Vector3 (1.0f, 2.0f, 3.0f);
		var v2 = new Vector3 (3.0f, 5.0f, 7.0f);

		var v3 = vector3_sub (v2, v1);
		if (v3.X != 2.0f || v3.Y != 3.0f || v3.Z != 4.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_mul (Vector3 v1, Vector3 v2) {
		return v1 * v2;
	}

	public static int test_0_vector3_mul () {
		var v1 = new Vector3 (1.0f, 2.0f, 3.0f);
		var v2 = new Vector3 (3.0f, 5.0f, 6.0f);

		var v3 = vector3_mul (v2, v1);
		if (v3.X != 3.0f || v3.Y != 10.0f || v3.Z != 18.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_mul_left (float v1, Vector3 v2) {
		return v1 * v2;
	}

	public static int test_0_vector3_mul_left () {
		var v1 = new Vector3 (3.0f, 5.0f, 6.0f);

		var v3 = vector3_mul_left (2.0f, v1);
		if (v3.X != 6.0f || v3.Y != 10.0f || v3.Z != 12.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_mul_right (Vector3 v1, float v2) {
		return v1 * v2;
	}

	public static int test_0_vector3_mul_right () {
		var v1 = new Vector3 (3.0f, 5.0f, 6.0f);

		var v3 = vector3_mul_right (v1, 2.0f);
		if (v3.X != 6.0f || v3.Y != 10.0f || v3.Z != 12.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_div (Vector3 v1, Vector3 v2) {
		return v1 / v2;
	}

	public static int test_0_vector3_div () {
		var v1 = new Vector3 (9.0f, 10.0f, 12.0f);
		var v2 = new Vector3 (3.0f, 5.0f, 6.0f);

		var v3 = vector3_div (v1, v2);
		if (v3.X != 3.0f || v3.Y != 2.0f || v3.Z != 2.0f)
			return 1;
		return 0;
	}

	[MethodImplAttribute (MethodImplOptions.NoInlining)]
	public static Vector3 vector3_div_right (Vector3 v1, float v2) {
		return v1 / v2;
	}

	public static int test_0_vector3_div_right () {
		var v1 = new Vector3 (9.0f, 15.0f, 21.0f);

		var v3 = vector3_div_right (v1, 3.0f);
		if (v3.X != 3.0f || v3.Y != 5.0f || v3.Z != 7.0f)
			return 1;
		return 0;
	}

}
