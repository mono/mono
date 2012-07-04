namespace Mono.CodeContracts.Static.Analysis.Numerical
{
    public static class BooleanExtensions
    {
        /// <summary>
        /// Returns value and sets result to a resultValue.
        /// </summary>
        public static bool With<T>(this bool value, T resultValue, out T result)
        {
            result = resultValue;
            return value;
        }

        /// <summary>
        /// Returns value and sets result to a default(T).
        /// </summary>
        public static bool Without<T>(this bool value, out T result)
        {
            result = default(T);
            return value;
        }
    }
}