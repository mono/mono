namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;

    public class ModelClientValidationRule {

        private readonly Dictionary<string, object> _validationParameters = new Dictionary<string, object>();
        private string _validationType;

        public string ErrorMessage {
            get;
            set;
        }

        public IDictionary<string, object> ValidationParameters {
            get {
                return _validationParameters;
            }
        }

        public string ValidationType {
            get {
                return _validationType ?? String.Empty;
            }
            set {
                _validationType = value;
            }
        }

    }
}
