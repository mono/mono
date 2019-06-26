using System;
using System.Reflection;

namespace Internal.Runtime.Augments {
	partial class RuntimeAugments {
		private static ReflectionExecutionDomainCallbacks s_reflectionExecutionDomainCallbacks = new ReflectionExecutionDomainCallbacks ();

		public static void ReportUnhandledException (Exception exception)
		{
			throw exception;
		}

		internal static ReflectionExecutionDomainCallbacks Callbacks => s_reflectionExecutionDomainCallbacks;
	}

	partial class ReflectionExecutionDomainCallbacks {
		internal Exception CreateMissingMetadataException (Type attributeType) 
		{
			return new MissingMetadataException ();
		}
	}
}