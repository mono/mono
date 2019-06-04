namespace System.Reflection {
    partial class MethodInfo {
        internal virtual int GenericParameterCount => GetGenericArguments ().Length;
    }
}
