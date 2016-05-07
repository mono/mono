using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Resources;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace System.ComponentModel.DataAnnotations {
    /// <summary>
    /// Attribute to provide a hint to the presentation layer about what control it should use
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "ControlParameters is exposed, just with a different type")]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class UIHintAttribute : Attribute {
        private UIHintImplementation _implementation;

        /// <summary>
        /// Gets the name of the control that is most appropriate for this associated property or field
        /// </summary>
        public string UIHint {
            get {
                return this._implementation.UIHint;
            }
        }

        /// <summary>
        /// Gets the name of the presentation layer that supports the control type in <see cref="UIHint"/>
        /// </summary>
        public string PresentationLayer {
            get {
                return this._implementation.PresentationLayer;
            }
        }

        /// <summary>
        /// Gets the name-value pairs used as parameters to the control's constructor
        /// </summary>
        /// <exception cref="InvalidOperationException"> is thrown if the current attribute is ill-formed.</exception>
        public IDictionary<string, object> ControlParameters {
            get {
                return this._implementation.ControlParameters;
            }
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets a unique identifier for this attribute.
        /// </summary>
        public override object TypeId {
            get {
                return this;
            }
        }
#endif

        /// <summary>
        /// Constructor that accepts the name of the control, without specifying which presentation layer to use
        /// </summary>
        /// <param name="uiHint">The name of the UI control.</param>
        public UIHintAttribute(string uiHint)
            : this(uiHint, null, new object[0]) {
        }

        /// <summary>
        /// Constructor that accepts both the name of the control as well as the presentation layer
        /// </summary>
        /// <param name="uiHint">The name of the control to use</param>
        /// <param name="presentationLayer">The name of the presentation layer that supports this control</param>
        public UIHintAttribute(string uiHint, string presentationLayer)
            : this(uiHint, presentationLayer, new object[0]) {
        }

        /// <summary>
        /// Full constructor that accepts the name of the control, presentation layer, and optional parameters
        /// to use when constructing the control
        /// </summary>
        /// <param name="uiHint">The name of the control</param>
        /// <param name="presentationLayer">The presentation layer</param>
        /// <param name="controlParameters">The list of parameters for the control</param>
        public UIHintAttribute(string uiHint, string presentationLayer, params object[] controlParameters) {
            this._implementation = new UIHintImplementation(uiHint, presentationLayer, controlParameters);
        }

        public override int GetHashCode() {
            return this._implementation.GetHashCode();
        }

        public override bool Equals(object obj) {
            var otherAttribute = obj as UIHintAttribute;
            if (otherAttribute == null) {
                return false;
            }
            return this._implementation.Equals(otherAttribute._implementation);
        }

        internal class UIHintImplementation {
            private IDictionary<string, object> _controlParameters;
            private object[] _inputControlParameters;

            /// <summary>
            /// Gets the name of the control that is most appropriate for this associated property or field
            /// </summary>
            public string UIHint { get; private set; }

            /// <summary>
            /// Gets the name of the presentation layer that supports the control type in <see cref="UIHint"/>
            /// </summary>
            public string PresentationLayer { get; private set; }

            public IDictionary<string, object> ControlParameters {
                get {
                    if (this._controlParameters == null) {
                        // Lazy load the dictionary. It's fine if this method executes multiple times in stress scenarios.
                        // If the method throws (indicating that the input params are invalid) this property will throw
                        // every time it's accessed.
                        this._controlParameters = this.BuildControlParametersDictionary();
                    }
                    return this._controlParameters;
                }
            }

            public UIHintImplementation(string uiHint, string presentationLayer, params object[] controlParameters) {
                this.UIHint = uiHint;
                this.PresentationLayer = presentationLayer;
                if (controlParameters != null) {
                    this._inputControlParameters = new object[controlParameters.Length];
                    Array.Copy(controlParameters, this._inputControlParameters, controlParameters.Length);
                }
            }

            /// <summary>
            /// Returns the hash code for this UIHintAttribute.
            /// </summary>
            /// <returns>A 32-bit signed integer hash code.</returns>
            public override int GetHashCode() {
                var a = this.UIHint ?? String.Empty;
                var b = this.PresentationLayer ?? String.Empty;

                return a.GetHashCode() ^ b.GetHashCode();
            }

            /// <summary>
            /// Determines whether this instance of UIHintAttribute and a specified object,
            /// which must also be a UIHintAttribute object, have the same value.
            /// </summary>
            /// <param name="obj">An System.Object.</param>
            /// <returns>true if obj is a UIHintAttribute and its value is the same as this instance; otherwise, false.</returns>
            public override bool Equals(object obj) {
                // don't need to perform a type check on obj since this is an internal class
                var otherImplementation = (UIHintImplementation)obj;

                if (this.UIHint != otherImplementation.UIHint || this.PresentationLayer != otherImplementation.PresentationLayer) {
                    return false;
                }

                IDictionary<string, object> leftParams;
                IDictionary<string, object> rightParams;

                try {
                    leftParams = this.ControlParameters;
                    rightParams = otherImplementation.ControlParameters;
                } catch (InvalidOperationException) {
                    return false;
                }

                Debug.Assert(leftParams != null, "leftParams shouldn't be null");
                Debug.Assert(rightParams != null, "rightParams shouldn't be null");
                if (leftParams.Count != rightParams.Count) {
                    return false;
                } else {
                    return leftParams.OrderBy(p => p.Key).SequenceEqual(rightParams.OrderBy(p => p.Key));
                }
            }


            /// <summary>
            /// Validates the input control parameters and throws InvalidOperationException if they are not correct.
            /// </summary>
            /// <returns>
            /// Dictionary of control parameters.
            /// </returns>
            private IDictionary<string, object> BuildControlParametersDictionary() {
                IDictionary<string, object> controlParameters = new Dictionary<string, object>();

                object[] inputControlParameters = this._inputControlParameters;

                if (inputControlParameters == null || inputControlParameters.Length == 0) {
                    return controlParameters;
                }
                if (inputControlParameters.Length % 2 != 0) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DataAnnotationsResources.UIHintImplementation_NeedEvenNumberOfControlParameters));
                }

                for (int i = 0; i < inputControlParameters.Length; i += 2) {
                    object key = inputControlParameters[i];
                    object value = inputControlParameters[i + 1];
                    if (key == null) {
                        throw new InvalidOperationException(
                            String.Format(
                            CultureInfo.CurrentCulture,
                            DataAnnotationsResources.UIHintImplementation_ControlParameterKeyIsNull,
                            i));
                    }

                    string keyString = key as string;
                    if (keyString == null) {
                        throw new InvalidOperationException(
                            String.Format(
                            CultureInfo.CurrentCulture,
                            DataAnnotationsResources.UIHintImplementation_ControlParameterKeyIsNotAString,
                            i,
                            inputControlParameters[i].ToString()));
                    }

                    if (controlParameters.ContainsKey(keyString)) {
                        throw new InvalidOperationException(
                            String.Format(
                            CultureInfo.CurrentCulture,
                            DataAnnotationsResources.UIHintImplementation_ControlParameterKeyOccursMoreThanOnce,
                            i,
                            keyString));
                    }

                    controlParameters[keyString] = value;
                }

                return controlParameters;
            }
        }
    }
}
