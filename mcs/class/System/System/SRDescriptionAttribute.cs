//
// System.Diagnostics.SRDescriptionAttribute.cs
//
// Authors:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;

namespace System
{
	[AttributeUsage(AttributeTargets.All)]
	internal class SRDescriptionAttribute : DescriptionAttribute
	{
		private bool isReplaced = false;

		public SRDescriptionAttribute (string description)
			: base (description)
		{
		}

		public override string Description {
			get {
				if (!isReplaced) {
					isReplaced = true;
					DescriptionValue = Locale.GetText (DescriptionValue);
				}
				return DescriptionValue;
			}
		}
	}
}
