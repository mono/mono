//
// System.ComponentModel.DefaultEventAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// (C) 2003 Andreas Nahr
//

using System;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DefaultEventAttribute : Attribute
	{
		private string eventName;

		public static readonly DefaultEventAttribute Default = new DefaultEventAttribute (null);

		public DefaultEventAttribute (string name)
		{
			eventName = name;
		}

		public string Name
		{
			get { return eventName; }
		}

		public override bool Equals (object o)
		{
			if (!(o is DefaultEventAttribute))
				return false;

			return (((DefaultEventAttribute) o).eventName == eventName);
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}
	}
}

