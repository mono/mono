using System.ComponentModel;

namespace System.Data
{
	sealed class ResDescriptionAttribute : DescriptionAttribute
	{
		public ResDescriptionAttribute (string description)
			: base (description)
		{
		}
	}
}