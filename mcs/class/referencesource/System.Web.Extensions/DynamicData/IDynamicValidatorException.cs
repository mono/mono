namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
        Justification = "Interface is intended for implementation by Exception classes.")]
    public interface IDynamicValidatorException {

        IDictionary<string, Exception> InnerExceptions { get; }

    }

}
