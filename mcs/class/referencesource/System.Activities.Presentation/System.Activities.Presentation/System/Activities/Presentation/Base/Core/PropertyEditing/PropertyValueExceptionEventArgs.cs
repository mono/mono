namespace System.Activities.Presentation.PropertyEditing
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Activities.Presentation;

    /// <summary>
    /// EventArgs class for the PropertyValueException
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    public class PropertyValueExceptionEventArgs : EventArgs
    {

        private string _message;
        private PropertyValue _value;
        private PropertyValueExceptionSource _source;
        private Exception _exception;

        /// <summary>
        /// Creates a PropertyValueExceptionEventArgs
        /// </summary>
        /// <param name="message">A message indicating what failed</param>
        /// <param name="value">The PropertyValue in which the exception is occuring</param>
        /// <param name="source">The source that generated this exception (get or set)</param>
        /// <param name="exception">The inner excpetion</param>
        /// <exception cref="ArgumentNullException">When message is null</exception>
        /// <exception cref="ArgumentNullException">When value is null</exception>
        /// <exception cref="ArgumentNullException">When exception is null</exception>
        public PropertyValueExceptionEventArgs(string message, PropertyValue value, PropertyValueExceptionSource source, Exception exception)
        {
            if (message == null) throw FxTrace.Exception.ArgumentNull("message");
            if (value == null) throw FxTrace.Exception.ArgumentNull("value");
            if (!EnumValidator.IsValid(source)) throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("source"));
            if (exception == null) throw FxTrace.Exception.ArgumentNull("exception");

            _message = message;
            _value = value;
            _source = source;
            _exception = exception;
        }

        /// <summary>
        /// Gets the message indicating what failed
        /// </summary>
        public string Message { get { return _message; } }

        /// <summary>
        /// Gets the PropertyValue for which the exception is occuring
        /// </summary>
        public PropertyValue PropertyValue { get { return _value; } }

        /// <summary>
        /// Gets the PropertyValueExceptionSource that generated the exception
        /// </summary>
        public PropertyValueExceptionSource Source { get { return _source; } }

        /// <summary>
        /// Gets returns the contained exception.
        /// </summary>
        public Exception Exception { get { return _exception; } }
    }
}
