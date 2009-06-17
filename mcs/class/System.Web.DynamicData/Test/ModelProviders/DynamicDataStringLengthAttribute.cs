using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.DynamicData.ModelProviders;

namespace MonoTests.ModelProviders
{
	[AttributeUsage (AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	class DynamicDataStringLengthAttribute : Attribute
	{
		public int MaxLength { get; private set; }

		public DynamicDataStringLengthAttribute (int maxLength)
		{
			MaxLength = maxLength;
		}
	}
}
