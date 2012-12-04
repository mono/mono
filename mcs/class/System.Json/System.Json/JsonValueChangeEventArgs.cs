// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Json
{
    /// <summary>
    /// Provide data for the <see cref="System.Json.JsonValue.Changing"/> and <see cref="System.Json.JsonValue.Changed"/> events.
    /// </summary>
    public class JsonValueChangeEventArgs : EventArgs
    {
        private JsonValue child;
        private JsonValueChange change;
        private int index;
        private string key;

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Json.JsonValueChangeEventArgs"/> class for
        /// changes in a <see cref="System.Json.JsonArray"/>.
        /// </summary>
        /// <param name="child">The <see cref="System.Json.JsonValue"/> instance which will be or has been modified.</param>
        /// <param name="change">The type of change of the <see cref="System.Json.JsonValue"/> event.</param>
        /// <param name="index">The index of the element being changed in a <see cref="System.Json.JsonArray"/>.</param>
        public JsonValueChangeEventArgs(JsonValue child, JsonValueChange change, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", RS.Format(Properties.Resources.ArgumentMustBeGreaterThanOrEqualTo, index, 0));
            }

            this.child = child;
            this.change = change;
            this.index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="System.Json.JsonValueChangeEventArgs"/> class for
        /// changes in a <see cref="System.Json.JsonObject"/>.
        /// </summary>
        /// <param name="child">The <see cref="System.Json.JsonValue"/> instance which will be or has been modified.</param>
        /// <param name="change">The type of change of the <see cref="System.Json.JsonValue"/> event.</param>
        /// <param name="key">The key of the element being changed in a <see cref="System.Json.JsonObject"/>.</param>
        public JsonValueChangeEventArgs(JsonValue child, JsonValueChange change, string key)
        {
            if (change != JsonValueChange.Clear)
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
            }

            this.child = child;
            this.change = change;
            index = -1;
            this.key = key;
        }

        /// <summary>
        /// Gets the child which will be or has been modified.
        /// </summary>
        /// <remarks><p>This property is <code>null</code> for <see cref="System.Json.JsonValueChange.Clear"/> event types
        /// raised by <see cref="System.Json.JsonValue"/> instances.</p>
        /// <p>For <see cref="System.Json.JsonValueChange">Replace</see> events, this property contains the new value in
        /// the <see cref="System.Json.JsonValue.Changing"/> event, and the old value (the one being replaced) in the
        /// <see cref="System.Json.JsonValue.Changed"/> event.</p></remarks>
        public JsonValue Child
        {
            get { return child; }
        }

        /// <summary>
        /// Gets the type of change.
        /// </summary>
        public JsonValueChange Change
        {
            get { return change; }
        }

        /// <summary>
        /// Gets the index in the <see cref="System.Json.JsonArray"/> where the change happened, or
        /// <code>-1</code> if the change happened in a <see cref="System.Json.JsonValue"/> of a different type.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the key in the <see cref="System.Json.JsonObject"/> where the change happened, or
        /// <code>null</code> if the change happened in a <see cref="System.Json.JsonValue"/> of a different type.
        /// </summary>
        /// <remarks>This property can also be <code>null</code> if the event type is
        /// <see cref="System.Json.JsonValueChange">Clear</see>.</remarks>
        public string Key
        {
            get { return key; }
        }
    }
}
