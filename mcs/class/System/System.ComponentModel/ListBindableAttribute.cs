//
// System.ComponentModel.ListBindableAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class ListBindableAttribute : Attribute
	{
		public static readonly ListBindableAttribute Default = new ListBindableAttribute (true, true);
		public static readonly ListBindableAttribute No = new ListBindableAttribute (false, true);
		public static readonly ListBindableAttribute Yes = new ListBindableAttribute (true, true);

		bool deflt;
		bool bindable;

		private ListBindableAttribute (bool listBindable, bool deflt)
		{
			this.deflt = deflt;
			bindable = listBindable;
		}
		
		public ListBindableAttribute (bool listBindable)
		{
			deflt = false;
			bindable = true;
		}

		public ListBindableAttribute (BindableSupport flags)
		{
			bindable = (flags == BindableSupport.Yes);
			deflt = (flags == BindableSupport.Default);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ListBindableAttribute))
				return false;

			return (((ListBindableAttribute) obj).bindable == bindable &&
				((ListBindableAttribute) obj).deflt == deflt);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return deflt;
		}

		public bool ListBindable
		{
			get {
				return bindable;
			}
		}
	}
}

