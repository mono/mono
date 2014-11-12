using System;
using System.Security;
using System.Reflection;
using System.Security.Permissions;

namespace System.Xml
{
    internal static class BinaryCompatibility
    {
        internal static bool TargetsAtLeast_Desktop_V4_5_2 { get { return _targetsAtLeast_Desktop_V4_5_2; } }

        private static bool _targetsAtLeast_Desktop_V4_5_2 = RunningOnCheck("TargetsAtLeast_Desktop_V4_5_2");

        [SecuritySafeCritical]
        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private  static bool RunningOnCheck(string propertyName)
        {
            Type binaryCompatabilityType;

            try
            {
                binaryCompatabilityType = typeof(Object).GetTypeInfo().Assembly.GetType("System.Runtime.Versioning.BinaryCompatibility", false);
            }
            catch (TypeLoadException)
            {
                return false;
            }

            if (binaryCompatabilityType == null)
                return false;

            PropertyInfo runningOnV4_5_2_Property = binaryCompatabilityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (runningOnV4_5_2_Property == null)
                return false;

            return (bool)runningOnV4_5_2_Property.GetValue(null);
        }
    }
}
