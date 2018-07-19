using System;
using System.Collections.Generic;

class Program
{
	public static void Main()
	{
	}

	private static IEnumerable<float> FindIntersections<TVector>(
		IBezier<TVector> bezier,
		Ray<TVector> ray,
		float epsilon,
		Range<float> t1,
		int depth) where TVector : IVector<TVector>
	{
		var bounds = bezier.GetBounds();
		if (Intersect.s(ray, bounds))
		{
			var intersections1 = new float[] { };
			var intersections2 = new float[] { };
			foreach (var t in intersections1) { yield return t; }
			foreach (var t in intersections2) { yield return t; }
		}
	}

	public static class Intersect
	{
		public static bool s<TVector>(Ray<TVector> ray, BoundingBoxN<TVector> box) where TVector : IVector<TVector>
		{
			throw new NotImplementedException();
		}
	}

	public struct Range<T>
	{
	}

	public class Ray<TVector> where TVector : IVector<TVector>
	{
	}

	public interface IBezier<TVector>
		where TVector : IVector<TVector>
	{
		BoundingBoxN<TVector> GetBounds();
	}

	public interface IVector<T> : IEpsilonEquatable<T, float>
		where T : IVector<T>
	{
	}

	public interface IEpsilonEquatable<TType, TEpsilon> // ReSharper enable TypeParameterCanBeVariant
	{
	}

	public struct BoundingBoxN<T>
		where T : IVector<T>
	{
	}
}
