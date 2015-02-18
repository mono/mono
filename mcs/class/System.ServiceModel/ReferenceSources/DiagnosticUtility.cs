//
// Stubs for DiagnosticUtility methods
//
// Copyright 2015 Xamarin Inc
//
namespace System.ServiceModel {
	
	static partial class DiagnosticUtility {
		public static Exception ThrowHelperArgumentNullOrEmptyString (string arg)
		{
			return new ArgumentException ("Argument null or empty", arg);
		}

		internal static partial class ExceptionUtility {

			public static Exception ThrowHelperArgumentNull (string arg)
			{
				return new ArgumentNullException ("Argument is null", arg);
			}

			public static Exception ThrowHelperError (Exception e)
			{
				return e;
			}

			internal static ArgumentException ThrowHelperArgument(string paramName, string message)
			{
				return new ArgumentException(message, paramName);
			}
			     
		}

	}
}
