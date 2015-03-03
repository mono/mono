namespace System
{
    // this is the introspection only type. This type overrides all the functions with runtime semantics
    // and throws an exception.
    // The idea behind this type is that it relieves RuntimeType from doing honerous checks about ReflectionOnly
    // context.
    // This type should not derive from RuntimeType but it's doing so for convinience.
    // That should not present a security threat though it is risky as a direct call to one of the base method
    // method (RuntimeType) and an instance of this type will work around the reason to have this type in the 
    // first place. However given RuntimeType is not public all its methods are protected and require full trust
    // to be accessed
    [Serializable]
    internal class ReflectionOnlyType : MonoType {

        private ReflectionOnlyType() : base (null)
        {}

        // always throw
        public override RuntimeTypeHandle TypeHandle 
        {
            get 
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAllowedInReflectionOnly"));
            }
        }

    }
}