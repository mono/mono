namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections;
    using System.Globalization;

    #region Class ValidationError

    [Serializable()]
    public sealed class ValidationError
    {
        private string errorText = string.Empty;
        private int errorNumber = 0;
        private Hashtable userData = null;
        private bool isWarning = false;
        string propertyName = null;

        public ValidationError(string errorText, int errorNumber)
            : this(errorText, errorNumber, false, null)
        {
        }

        public ValidationError(string errorText, int errorNumber, bool isWarning)
            : this(errorText, errorNumber, isWarning, null)
        {
        }

        public ValidationError(string errorText, int errorNumber, bool isWarning, string propertyName)
        {
            this.errorText = errorText;
            this.errorNumber = errorNumber;
            this.isWarning = isWarning;
            this.propertyName = propertyName;
        }
        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
            set
            {
                this.propertyName = value;
            }
        }
        public string ErrorText
        {
            get
            {
                return this.errorText;
            }
        }
        public bool IsWarning
        {
            get
            {
                return this.isWarning;
            }
        }
        public int ErrorNumber
        {
            get
            {
                return this.errorNumber;
            }
        }
        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                    this.userData = new Hashtable();
                return this.userData;
            }
        }

        public static ValidationError GetNotSetValidationError(string propertyName)
        {
            ValidationError error = new ValidationError(SR.GetString(SR.Error_PropertyNotSet, propertyName), ErrorNumbers.Error_PropertyNotSet);
            error.PropertyName = propertyName;
            return error;
        }

        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "{0} {1}: {2}", this.isWarning ? "warning" : "error", this.errorNumber, this.errorText);
        }
    }

    #endregion
}
