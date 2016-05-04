//------------------------------------------------------------------------------
// <copyright file="TransformerConfigurationWizardBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI.WebControls;

    internal abstract class TransformerConfigurationWizardBase : Wizard, ITransformerConfigurationControl {
        private string[] _oldProviderNames;
        private string[] _oldConsumerNames;

        private const int baseIndex = 0;
        private const int oldProviderNamesIndex = 1;
        private const int oldConsumerNamesIndex = 2;
        private const int controlStateArrayLength = 3;

        private static readonly object EventCancelled = new object();
        private static readonly object EventSucceeded = new object();

        protected abstract PropertyDescriptorCollection ConsumerSchema { get; }

        protected string[] OldConsumerNames {
            get {
                return _oldConsumerNames;
            }
            set {
                _oldConsumerNames = value;
            }
        }

        protected string[] OldProviderNames {
            get {
                return _oldProviderNames;
            }
            set {
                _oldProviderNames = value;
            }
        }

        protected abstract PropertyDescriptorCollection ProviderSchema { get; }

        public event EventHandler Cancelled {
            add {
                Events.AddHandler(EventCancelled, value);
            }
            remove {
                Events.RemoveHandler(EventCancelled, value);
            }
        }

        public event EventHandler Succeeded {
            add {
                Events.AddHandler(EventSucceeded, value);
            }
            remove {
                Events.RemoveHandler(EventSucceeded, value);
            }
        }

        protected abstract void CreateWizardSteps();

        protected internal override void LoadControlState(object savedState) {
            if (savedState == null) {
                // Create wizard steps before loading base ControlState.
                CreateWizardSteps();
                base.LoadControlState(null);
            }
            else {
                object[] myState = (object[])savedState;
                if (myState.Length != controlStateArrayLength) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_ControlState));
                }

                if (myState[oldProviderNamesIndex] != null) {
                    OldProviderNames = (string[])myState[oldProviderNamesIndex];
                }
                if (myState[oldConsumerNamesIndex] != null) {
                    OldConsumerNames = (string[])myState[oldConsumerNamesIndex];
                }

                // Create wizard steps before loading base ControlState.
                CreateWizardSteps();
                base.LoadControlState(myState[baseIndex]);
            }
        }

        protected override void OnCancelButtonClick(EventArgs e) {
            OnCancelled(EventArgs.Empty);
            base.OnCancelButtonClick(e);
        }

        private void OnCancelled(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventCancelled];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected override void OnFinishButtonClick(WizardNavigationEventArgs e) {
            OnSucceeded(EventArgs.Empty);
            base.OnFinishButtonClick(e);
        }

        protected internal override void OnInit(EventArgs e) {
            DisplayCancelButton = true;
            DisplaySideBar = false;

            if (Page != null) {
                Page.RegisterRequiresControlState(this);
                Page.PreRenderComplete += new EventHandler(this.OnPagePreRenderComplete);
            }
            base.OnInit(e);
        }

        private void OnPagePreRenderComplete(object sender, EventArgs e) {
            // Copy current schemas into arrays
            string[] providerNames = ConvertSchemaToArray(ProviderSchema);
            string[] consumerNames = ConvertSchemaToArray(ConsumerSchema);

            // If current schemas are not the same as the old schemas, or if the WizardSteps have
            // not been created yet, update the old schemas and recreate the Wizard steps.
            if (StringArraysDifferent(providerNames, OldProviderNames) ||
                StringArraysDifferent(consumerNames, OldConsumerNames) || WizardSteps.Count == 0) {
                OldProviderNames = providerNames;
                OldConsumerNames = consumerNames;

                WizardSteps.Clear();
                ClearChildState();

                CreateWizardSteps();
                ActiveStepIndex = 0;
            }

            // We should always have at least 1 WizardStep before we Render
            Debug.Assert(WizardSteps.Count > 0);
        }

        private string[] ConvertSchemaToArray(PropertyDescriptorCollection schema) {
            string[] names = null;

            if (schema != null && schema.Count > 0) {
                names = new string[schema.Count * 2];
                for (int i=0; i < schema.Count; i++) {
                    PropertyDescriptor descriptor = schema[i];
                    if (descriptor != null) {
                        names[2*i] = descriptor.DisplayName;
                        names[2*i + 1] = descriptor.Name;
                    }
                }
            }

            return names;
        }

        private void OnSucceeded(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventSucceeded];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected internal override object SaveControlState() {
            object[] myState = new object[controlStateArrayLength];

            myState[baseIndex] = base.SaveControlState();
            myState[oldProviderNamesIndex] = OldProviderNames;
            myState[oldConsumerNamesIndex] = OldConsumerNames;

            for (int i=0; i < controlStateArrayLength; i++) {
                if (myState[i] != null) {
                    return myState;
                }
            }

            // More performant to return null than an array of null values
            return null;
        }

        private bool StringArraysDifferent(string[] arrA, string[] arrB) {
            int lengthA = (arrA == null) ? 0 : arrA.Length;
            int lengthB = (arrB == null) ? 0 : arrB.Length;

            if (lengthA != lengthB) {
                return true;
            }
            else {
                for (int i=0; i < lengthB; i++) {
                    if (arrA[i] != arrB[i]) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
