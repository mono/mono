using System;

namespace Foo
{
	public static class Magic
	{
		public interface IUpDown
		{
			int DestinationDimension { get; }
		}

		public static int Main ()
		{
			Magic<decimal>.Upsample (new Instance ());
			return 0;
		}
	}

	public static class Magic<T>
	{
		public interface IAccessible { T this[int index] { get; set; } }

		public interface IUpDown : Magic.IUpDown, IAccessible { }

		public static void Upsample (IUpDown o)
		{
			var count = o.DestinationDimension;
		}
	}

	class Instance : Magic<decimal>.IUpDown
	{
		#region IUpDown Members

		public int DestinationDimension
		{
			get
			{
				return 1;
			}
		}

		#endregion

		#region IAccessible Members

		public decimal this[int index]
		{
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}

		#endregion
	}
}