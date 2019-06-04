namespace System.Reflection
{
    partial struct CustomAttributeTypedArgument
    {
        static object CanonicalizeValue (object value)
        {
            if (value.GetType ().IsEnum)
                return ((Enum) value).GetValue ();

            return value;
        }
    }
}
