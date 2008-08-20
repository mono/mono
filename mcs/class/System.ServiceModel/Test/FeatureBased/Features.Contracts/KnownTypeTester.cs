using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MonoTests.Features.Contracts
{
	[ServiceContract (Namespace = "http://MonoTests.Features.Contracts")]
	public interface IKnownTypeTesterContract
	{
		[OperationContract]
		Point2D Move (Point2D point, Point2D delta);

		[OperationContract]
		double Distance (Point2D point1, Point2D point2);

		[OperationContract]
		BaseContract [] foo ();
	}

	public class KnownTypeTester : IKnownTypeTesterContract
	{
		public Point2D Move (Point2D point, Point2D delta)
		{
			return new AdvPoint2D (point.X + delta.X, point.Y + delta.Y);
		}

		public double Distance (Point2D point1, Point2D point2)
		{
			return Math.Sqrt (Math.Abs (point1.X - point2.X) +
				Math.Abs (point1.Y - point2.Y));
		}

		public BaseContract [] foo () {
			return new BaseContract[] {new DerivedContract ()};
		}

	}

	[DataContract (Namespace = "http://MonoTests.Features.Contracts")]
	[KnownType (typeof (AdvPoint2D))]
	public class Point2D
	{
		int x;
		int y;
		public Point2D () { }

		public Point2D (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		[DataMember]
		public int X
		{
			get { return x; }
			set { x = value; }
		}

		[DataMember]
		public int Y
		{
			get { return y; }
			set { y = value; }
		}
	}

	[DataContract (Namespace = "http://MonoTests.Features.Contracts")]
	public class AdvPoint2D : Point2D
	{
		public AdvPoint2D (int x, int y)
			: base (x, y)
		{
		}

		[DataMember]
		public double ZeroDistance
		{
			get { return Math.Sqrt (X * X + Y * Y); }
			set { }
		}
	}

	[DataContract]
	[KnownType (typeof (DerivedContract))]
	public class BaseContract
	{
		[DataMember]
		string name;
	}

	[DataContract]
	public class DerivedContract : BaseContract
	{
		[DataMember]
		bool blah;
	}


}
