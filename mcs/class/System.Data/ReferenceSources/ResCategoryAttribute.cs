using System.ComponentModel;

namespace System.Data
{
	sealed class ResCategoryAttribute : CategoryAttribute
	{
		public ResCategoryAttribute (string category)
			: base (category)
		{
		}
	}
}