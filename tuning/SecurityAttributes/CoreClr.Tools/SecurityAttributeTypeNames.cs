using System;

namespace CoreClr.Tools
{
	public static class SecurityAttributeTypeNames
	{
		public const string Critical = "System.Security.SecurityCriticalAttribute";
		public const string SafeCritical = "System.Security.SecuritySafeCriticalAttribute";

		public static readonly string[] All = new[] { Critical, SafeCritical };

	    public static string AttributeTypeNameFor(SecurityAttributeType securityAttributeType)
	    {
	        switch (securityAttributeType)
	        {
	            case SecurityAttributeType.Critical:
	                return Critical;
	            case SecurityAttributeType.SafeCritical:
	                return SafeCritical;
	        }
	        throw new InvalidOperationException();
	    }
	}
}

