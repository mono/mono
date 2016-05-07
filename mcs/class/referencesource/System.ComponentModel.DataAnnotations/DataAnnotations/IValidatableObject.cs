#if !SILVERLIGHT
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Validatable", Justification = "While not in the dictionary, this is spelled to match other APIs and frameworks.")]
    public interface IValidatableObject {
        IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }
}
#endif