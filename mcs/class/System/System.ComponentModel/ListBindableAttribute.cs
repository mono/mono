//
// System.ComponentModel.ListBindableAttribute.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
	public sealed class ListBindableAttribute : Attribute
	{
		public static readonly ListBindableAttribute Default = new ListBindableAttribute (true);
		public static readonly ListBindableAttribute No = new ListBindableAttribute (false);
		public static readonly ListBindableAttribute Yes = new ListBindableAttribute (true);

		bool bindable;
		
		public ListBindableAttribute (bool listBindable)
		{
			bindable = listBindable;
		}

		public ListBindableAttribute (BindableSupport flags)
		{
            		if (flags == BindableSupport.No)
                		bindable = false;
            		else
                		bindable = true;
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ListBindableAttribute))
				return false;

			return ((ListBindableAttribute) obj).ListBindable.Equals (bindable);
		}

		public override int GetHashCode ()
		{
			return bindable.GetHashCode ();
		}

		public override bool IsDefaultAttribute ()
		{
			return Equals (Default);
		}

		public bool ListBindable {
			get {
				return bindable;
			}
		}
	}
}

