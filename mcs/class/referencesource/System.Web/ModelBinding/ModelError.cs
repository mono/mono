namespace System.Web.ModelBinding {
    using System;

    [Serializable]
    public class ModelError {

        public ModelError(Exception exception)
            : this(exception, null /* errorMessage */) {
        }

        public ModelError(Exception exception, string errorMessage)
            : this(errorMessage) {
            if (exception == null) {
                throw new ArgumentNullException("exception");
            }

            Exception = exception;
        }

        public ModelError(string errorMessage) {
            ErrorMessage = errorMessage ?? String.Empty;
        }

        public Exception Exception {
            get;
            private set;
        }

        public string ErrorMessage {
            get;
            private set;
        }
    }
}
