namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public class FieldValidationMetadata {

        private string _fieldName;
        private readonly Collection<ModelClientValidationRule> _validationRules = new Collection<ModelClientValidationRule>();

        public string FieldName {
            get {
                return _fieldName ?? String.Empty;
            }
            set {
                _fieldName = value;
            }
        }

        public bool ReplaceValidationMessageContents {
            get;
            set;
        }

        public string ValidationMessageId {
            get;
            set;
        }

        public ICollection<ModelClientValidationRule> ValidationRules {
            get {
                return _validationRules;
            }
        }

    }
}
