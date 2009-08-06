/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

// Compile with 
//    csc /r:C5.dll GConvexHull.cs

using System;
using C5;

namespace GConvexHull
{
// Find the convex hull of a point set in the plane

// An implementation of Graham's (1972) point elimination algorithm,
// as modified by Andrew (1979) to find lower and upper hull separately.

// This implementation correctly handle duplicate points, and
// multiple points with the same x-coordinate.

// 1. Sort the points lexicographically by increasing (x,y), thus 
//    finding also a leftmost point L and a rightmost point R.
// 2. Partition the point set into two lists, upper and lower, according as 
//    point is above or below the segment LR.  The upper list begins with 
//    L and ends with R; the lower list begins with R and ends with L.
// 3. Traverse the point lists clockwise, eliminating all but the extreme
//    points (thus eliminating also duplicate points).
// 4. Join the point lists (in clockwise order) in an array, 
//    leaving out L from lower and R from upper.

  public class Convexhull
  {
    public static Point[] ConvexHull(Point[] pts)
    {
      // 1. Sort points lexicographically by increasing (x, y)
      int N = pts.Length;
      Array.Sort(pts);
      Point left = pts[0], right = pts[N - 1];
      // 2. Partition into lower hull and upper hull
      IList<Point> lower = new LinkedList<Point>(),
        upper = new LinkedList<Point>();
      lower.InsertFirst(left); upper.InsertLast(left);
      for (int i = 0; i < N; i++)
      {
        double det = Point.Area2(left, right, pts[i]);
        if (det < 0)
          lower.InsertFirst(pts[i]);
        else if (det > 0)
          upper.InsertLast(pts[i]);
      }
      lower.InsertFirst(right);
      upper.InsertLast(right);
      // 3. Eliminate points not on the hull
      Eliminate(lower);
      Eliminate(upper);
      // 4. Join the lower and upper hull, leaving out lower.Last and upper.Last
      Point[] res = new Point[lower.Count + upper.Count - 2];
      lower[0, lower.Count - 1].CopyTo(res, 0);
      upper[0, upper.Count - 1].CopyTo(res, lower.Count - 1);
      return res;
    }

    // Graham's scan
    public static void Eliminate(IList<Point> lst)
    {
      IList<Point> view = lst.View(0, 0);
      int slide = 0;
      while (view.TrySlide(slide, 3))
        if (Point.Area2(view[0], view[1], view[2]) < 0)   // right turn
          slide = 1;
        else
        {                                                 // left or straight
          view.RemoveAt(1);
          slide = view.Offset != 0 ? -1 : 0;
        }
    }
  }

// ------------------------------------------------------------

// Points in the plane

  public class Point : IComparable<Point>
  {
    private static readonly C5Random rnd = new C5Random(42);

    public readonly double x, y;

    public Point(double x, double y)
    {
      this.x = x; this.y = y;
    }

    public override string ToString()
    {
      return "(" + x + ", " + y + ")";
    }

    public static Point Random(int w, int h)
    {
      return new Point(rnd.Next(w), rnd.Next(h));
    }

    public bool Equals(Point p2)
    {
      return x == p2.x && y == p2.y;
    }

    public int CompareTo(Point p2)
    {
      int major = x.CompareTo(p2.x);
      return major != 0 ? major : y.CompareTo(p2.y);
    }

    // Twice the signed area of the triangle (p0, p1, p2)
    public static double Area2(Point p0, Point p1, Point p2)
    {
      return p0.x * (p1.y - p2.y) + p1.x * (p2.y - p0.y) + p2.x * (p0.y - p1.y);
    }
  }

// ------------------------------------------------------------

  class GConvexHull
  {
    static void Main(String[] args)
    {
      if (args.Length == 1)
      {
        string arg = args[0];
        int N = int.Parse(arg);
        Point[] pts = new Point[N];
        for (int i = 0; i < N; i++)
          pts[i] = Point.Random(500, 500);
        Point[] chpts = Convexhull.ConvexHull(pts);
        Console.WriteLine("Area is " + Area(chpts));
        Print(chpts);
      }
      else
        Console.WriteLine("Usage: GConvexHull <pointcount>\n");
    }

    // The area of a polygon (represented by an array of ordered vertices)
    public static double Area(Point[] pts)
    {
      int N = pts.Length;
      Point origo = new Point(0, 0);
      double area2 = 0;
      for (int i = 0; i < N; i++)
        area2 += Point.Area2(origo, pts[i], pts[(i + 1) % N]);
      return Math.Abs(area2 / 2);
    }

    public static void Print(Point[] pts)
    {
      int N = pts.Length;
      for (int i = 0; i < N; i++)
        Console.WriteLine(pts[i]);
    }
  }
}
