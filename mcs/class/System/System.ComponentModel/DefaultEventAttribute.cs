//
// System.ComponentModel.DefaultEventAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DefaultEventAttribute : Attribute
	{
		private string eventName;

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

