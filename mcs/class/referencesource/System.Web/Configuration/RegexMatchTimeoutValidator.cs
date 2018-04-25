namespace System.Web.Configuration {
    using System;
    using System.Configuration;

    // Used for validating a timeout to be passed as the default match timeout for Regex
    internal sealed class RegexMatchTimeoutValidator : TimeSpanValidator {

        private static readonly TimeSpan _minValue = TimeSpan.Zero;
        private static readonly TimeSpan _maxValue = TimeSpan.FromMilliseconds(Int32.MaxValue - 1); // from Regex.cs

        public RegexMatchTimeoutValidator()
            : base(_minValue, _maxValue) {
        }

    }
}
