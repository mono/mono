//------------------------------------------------------------------------------
// <copyright file="EncoderUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Security.AntiXss {
    using System;
    using System.Text;

    /// <summary>
    /// Provides helper methods common to all Anti-XSS encoders.
    /// </summary>
    internal static class EncoderUtil {

        /// <summary>
        /// Gets an appropriately-sized StringBuilder for the output of an encoding routine.
        /// </summary>
        /// <param name="inputLength">The length (in characters) of the input string.</param>
        /// <param name="worstCaseOutputCharsPerInputChar">The worst-case ratio of output characters per input character.</param>
        /// <returns>A StringBuilder appropriately-sized to hold the output string.</returns>
        internal static StringBuilder GetOutputStringBuilder(int inputLength, int worstCaseOutputCharsPerInputChar) {
            // We treat 32KB byte size (16k chars) as a soft upper boundary for the length of any StringBuilder
            // that we allocate. We'll try to avoid going above this boundary if we can avoid it so that we
            // don't allocate objects on the LOH.
            const int upperBound = 16 * 1024;

            int charsToAllocate;
            if (inputLength >= upperBound) {
                // We know that the output will contain at least as many characters as the input, so if the
                // input length exceeds the soft upper boundary just preallocate the entire builder and hope for
                // a best-case outcome.
                charsToAllocate = inputLength;
            }
            else {
                // Allocate the worst-case if we can, but don't exceed the soft upper boundary.
                long worstCaseTotalChars = (long)inputLength * worstCaseOutputCharsPerInputChar; // don't overflow Int32
                charsToAllocate = (int)Math.Min(upperBound, worstCaseTotalChars);
            }

            // Once we have chosen an initial value for the StringBuilder size, the StringBuilder type will
            // efficiently allocate additionally blocks if necessary.
            return new StringBuilder(charsToAllocate);
        }

    }
}
