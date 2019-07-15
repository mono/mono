namespace System.Reflection {
    [Serializable]
    partial class MethodInfo {
        internal virtual int GenericParameterCount => GetGenericArguments ().Length;
    }
}
