//
// System.ComponentModel.MergablePropertyAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public sealed class MergablePropertyAttribute : Attribute
	{

		private bool mergable;

		public static readonly MergablePropertyAttribute Default = new MergablePropertyAttribute (true);
		public static readonly MergablePropertyAttribute No = new MergablePropertyAttribute (false);
		public static readonly MergablePropertyAttribute Yes = new MergablePropertyAttribute (true);

		public MergablePropertyAttribute (bool allowMerge)
		{
			this.mergable = allowMerge;
		}

		public bool AllowMerge {
			get { return mergable; }
		}

		public override bool Equals (object obj)
		{
			if (!(obj is MergablePropertyAttribute))
				return false;
			if (obj == this)
				return true;
			return ((MergablePropertyAttribute) obj).AllowMerge == mergable;
		}

		public override int GetHashCode()
		{
			return mergable.GetHashCode ();
		}

		public override bool IsDefaultAttribute()
		{
			return mergable == MergablePropertyAttribute.Default.AllowMerge;
		}
	}
}
