using System.Globalization;

namespace System
{
	partial class Environment
	{
		internal static string GetResourceString (string key)
		{
			switch (key) {
			case "AssertionFailed":
				return "Assertion failed.";
			case "AssumptionFailed":
				return "Assumption failed.";
			case "InvariantFailed":
				return "Invariant failed.";
			case "PostconditionFailed":
				return "Postcondition failed.";
			case "PostconditionOnExceptionFailed":
				return "Postcondition failed after throwing an exception.";
			case "PreconditionFailed":
				return "Precondition failed.";
			case "AggregateException_ToString":
				return "AggregateException_ToString={0}{1}---> (Inner Exception #{2}) {3}{4}{5}";
			case "AggregateException_ctor_DefaultMessage":
				return "One or more errors occurred.";
			}

			return key;
		}

		internal static string GetResourceString (string key, CultureInfo culture)
		{
			return key;
		}

		internal static string GetResourceString (string key, params object[] values)
		{
			switch (key) {
			case "AssertionFailed_Cnd":
				key = "Assertion failed: {0}";
				break;
			case "AssumptionFailed_Cnd":
				key = "Assumption failed: {0}";
				break;
			case "InvariantFailed_Cnd":
				key = "Invariant failed: {0}";
				break;
			case "PostconditionFailed_Cnd":
				key = "Postcondition failed: {0}";
				break;
			case "PostconditionOnExceptionFailed_Cnd":
				key = "Postcondition failed after throwing an exception: {0}";
				break;
			case "PreconditionFailed_Cnd":
				key = "Precondition failed: {0}";
				break;
			}

			return string.Format (CultureInfo.InvariantCulture, key, values);
		}
	}
}