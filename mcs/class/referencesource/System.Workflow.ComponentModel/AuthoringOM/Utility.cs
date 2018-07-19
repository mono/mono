namespace System.Workflow
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    static class Utility
    {
        [SuppressMessage("Reliability", "Reliability113", Justification = "These are the core methods that should be used for all other Guid(string) calls.")]
        internal static Guid CreateGuid(string guidString)
        {
            bool success = false;
            Guid result = Guid.Empty;

            try
            {
                result = new Guid(guidString);
                success = true;
            }
            finally
            {
                if (!success)
                {
                    Debug.Assert(false, "Creation of the Guid failed.");
                }
            }

            return result;
        }

        [SuppressMessage("Reliability", "Reliability113", Justification = "These are the core methods that should be used for all other Guid(string) calls.")]
        internal static bool TryCreateGuid(string guidString, out Guid result)
        {
            bool success = false;
            result = Guid.Empty;

            try
            {
                result = new Guid(guidString);
                success = true;
            }
            catch (ArgumentException)
            {
                // ---- this
            }
            catch (FormatException)
            {
                // ---- this
            }
            catch (OverflowException)
            {
                // ---- this
            }

            return success;
        }
    }
}
