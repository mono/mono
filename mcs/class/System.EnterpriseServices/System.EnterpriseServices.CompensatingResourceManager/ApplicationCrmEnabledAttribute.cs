//
// System.EnterpriseServices.CompensatingResourceManager.ApplicationCrmEnabledAttribute.cs
//
// Author:
//   Alejandro Sánchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro Sánchez Acosta
//

namespace System.EnterpriseServices.CompensatingResourceManager {

	/// <summary>
	///   ApplicationCrmEnable Attribute for classes. 
	/// </summary>
	
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class ApplicationCrmEnabledAttribute : Attribute
	{
		public bool val;

		public ApplicationCrmEnabledAttribute()
		{
			val = true;
		}

		public ApplicationCrmEnabledAttribute (bool val)
		{
			this.val = val;
		}

		public bool Value 
		{
			get
			{
				return val;
			}
		}
	}
}
