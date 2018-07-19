namespace System.Web.ModelBinding {
    using System;

    public class ModelValidationResult {

        private string _memberName;
        private string _message;

        public string MemberName {
            get {
                return _memberName ?? String.Empty;
            }
            set {
                _memberName = value;
            }
        }

        public string Message {
            get {
                return _message ?? String.Empty;
            }
            set {
                _message = value;
            }
        }

    }
}
