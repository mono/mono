using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Container class for the results of a validation request.
    /// <para>
    /// Use the static <see cref="ValidationResult.Success"/> to represent successful validation.
    /// </para>
    /// </summary>
    /// <seealso cref="ValidationAttribute.GetValidationResult"/>
    public
#if SILVERLIGHT
    sealed
#endif
 class ValidationResult {
        #region Member Fields

        private IEnumerable<string> _memberNames;
        private string _errorMessage;

        /// <summary>
        /// Gets a <see cref="ValidationResult"/> that indicates Success.
        /// </summary>
        /// <remarks>
        /// The <c>null</c> value is used to indicate success.  Consumers of <see cref="ValidationResult"/>s
        /// should compare the values to <see cref="ValidationResult.Success"/> rather than checking for null.
        /// </remarks>
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "We want this to be readonly since we're just returning null")]
        public static readonly ValidationResult Success;

        #endregion

        #region All Constructors

        /// <summary>
        /// Constructor that accepts an error message.  This error message would override any error message
        /// provided on the <see cref="ValidationAttribute"/>.
        /// </summary>
        /// <param name="errorMessage">The user-visible error message.  If null, <see cref="ValidationAttribute.GetValidationResult"/>
        /// will use <see cref="ValidationAttribute.FormatErrorMessage"/> for its error message.</param>
        public ValidationResult(string errorMessage)
            : this(errorMessage, null) {
        }

        /// <summary>
        /// Constructor that accepts an error message as well as a list of member names involved in the validation.
        /// This error message would override any error message provided on the <see cref="ValidationAttribute"/>.
        /// </summary>
        /// <param name="errorMessage">The user-visible error message.  If null, <see cref="ValidationAttribute.GetValidationResult"/> 
        /// will use <see cref="ValidationAttribute.FormatErrorMessage"/> for its error message.</param>
        /// <param name="memberNames">The list of member names affected by this result.
        /// This list of member names is meant to be used by presentation layers to indicate which fields are in error.</param>
        public ValidationResult(string errorMessage, IEnumerable<string> memberNames) {
            this._errorMessage = errorMessage;
            this._memberNames = memberNames ?? new string[0];
        }

#if !SILVERLIGHT
        /// <summary>
        /// Constructor that creates a copy of an existing ValidationResult.
        /// </summary>
        /// <param name="validationResult">The validation result.</param>
        /// <exception cref="System.ArgumentNullException">The <paramref name="validationResult"/> is null.</exception>
        protected ValidationResult(ValidationResult validationResult) {
            if (validationResult == null) {
                throw new ArgumentNullException("validationResult");
            }

            this._errorMessage = validationResult._errorMessage;
            this._memberNames = validationResult._memberNames;
        }
#endif
        #endregion

        #region Properties

        /// <summary>
        /// Gets the collection of member names affected by this result.  The collection may be empty but will never be null.
        /// </summary>
        public IEnumerable<string> MemberNames {
            get {
                return this._memberNames;
            }
        }

        /// <summary>
        /// Gets the error message for this result.  It may be null.
        /// </summary>
        public string ErrorMessage {
            get {
                return this._errorMessage;
            }
            set {
                this._errorMessage = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Override the string representation of this instance, returning
        /// the <see cref="ErrorMessage"/> if not <c>null</c>, otherwise
        /// the base <see cref="Object.ToString"/> result.
        /// </summary>
        /// <remarks>
        /// If the <see cref="ErrorMessage"/> is empty, it will still qualify
        /// as being specified, and therefore returned from <see cref="ToString"/>.
        /// </remarks>
        /// <returns>The <see cref="ErrorMessage"/> property value if specified,
        /// otherwise, the base <see cref="Object.ToString"/> result.</returns>
        public override string ToString() {
            return this.ErrorMessage ?? base.ToString();
        }

        #endregion Methods

    }
}
