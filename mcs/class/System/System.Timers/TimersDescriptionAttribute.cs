//
// System.Timers.TimersDescriptionAttribute
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.ComponentModel;

namespace System.Timers
{
	[AttributeUsage(AttributeTargets.All)]
	public class TimersDescriptionAttribute : DescriptionAttribute
	{
		public TimersDescriptionAttribute (string description)
			: base (description)
		{
		}

		public override string Description
		{
			get {
				return base.Description;
			}
		}
	}
}

