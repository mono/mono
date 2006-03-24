using System;
using System.Drawing;
using System.Collections;

namespace DrawingTestHelper
{
	/// <summary>
	/// Summary description for PDComparer.
	/// </summary>
	public class PDComparer
	{
		static int SearchRectSize = 10;
		static double [,] ltDistances = new double[SearchRectSize,SearchRectSize];


		static PDComparer()
		{
			for (int x = 0; x < SearchRectSize; x++)
				for (int y = 0; y < SearchRectSize; y++)
				{
					ltDistances[x,y] = Math.Sqrt(x*x + y*y);
				}
		}

		public PDComparer()
		{
		}

		public static double Compare(Bitmap b1, Bitmap b2)
		{
			Point [] shapePoints = GetPointFromImage(b1);
			double [] pointsDistance = new double[ shapePoints.Length ];

			for (int i = 0; i < shapePoints.Length; i++)
			{
				pointsDistance[i] = DistanceToClosestPoint( shapePoints[i], b2 );
			}

			return Max( pointsDistance );
		}

		private static double DistanceToClosestPoint(Point p, Bitmap b)
		{
			if (IsPixelExist( b.GetPixel(p.X, p.Y) ))
				return 0;

			Rectangle r = new Rectangle(
				p.X - SearchRectSize / 2,
				p.Y - SearchRectSize / 2,
				SearchRectSize,
				SearchRectSize);

			double min_distance = SearchRectSize;

			for (int x = r.X; x < r.X + SearchRectSize; x++)
				for (int y = r.Y; y < r.Y + SearchRectSize; y++)
					if ((x < b.Width) && (y < b.Height) && (x >= 0) && (y >= 0))
					{
						if ( IsPixelExist( b.GetPixel(x, y) ) )
						{
							double d = CalculateDistance(p.X, p.Y, x, y);
							if (d < min_distance)
								min_distance = d;
						}
					}

			return min_distance;
		}

		private static double CalculateDistance(Point a, Point b)
		{
			return CalculateDistance(a.X, a.Y, b.X, b.Y);
		}

		private static double CalculateDistance(int x1, int y1, int x2, int y2)
		{
			int delta_x = Math.Abs(x2 - x1);
			int delta_y = Math.Abs(y2 - y1);
			return ltDistances[delta_x, delta_y];
		}

		private static double Max(double [] a)
		{
			double max = 0;

			for (int i = 0; i < a.Length; i++)
				if (a[i] > max)
					max = a[i];
			return max;
		}
		
		private static Point [] GetPointFromImage(Bitmap b)
		{
			ArrayList points = new ArrayList();
			
			for(int x = 0; x < b.Width; x++)
				for(int y = 0; y < b.Height; y++)
					if (IsPixelExist ( b.GetPixel(x, y) ))
						points.Add( new Point(x, y) );

			return (Point [])points.ToArray( typeof(Point) );
		}

		private static bool IsPixelExist(Color c)
		{
			return c.A > 0;
		}
	}
}
