//------------------------------------------------------------------------------
// <copyright file="ParameterCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;


    /// <devdoc>
    /// A state managed collection of Parameter objects.
    /// These are used in many DataSourceControls to filter queries.
    /// </devdoc>
    [
    Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
    ]
    public class ParameterCollection : StateManagedCollection {

        private EventHandler _parametersChangedHandler;

        private static readonly Type[] knownTypes = new Type[] {
            typeof(ControlParameter),
            typeof(CookieParameter),
            typeof(FormParameter),
            typeof(Parameter),
            typeof(QueryStringParameter),
            typeof(SessionParameter),
            typeof(ProfileParameter),
        };


        /// <devdoc>
        /// Returns the Parameter at a given index.
        /// </devdoc>
        public Parameter this[int index] {
            get {
                return (Parameter)((IList)this)[index];
            }
            set {
                ((IList)this)[index] = value;
            }
        }

        /// <devdoc>
        /// Returns the Parameter with a given name.
        /// </devdoc>
        public Parameter this[string name] {
            get {
                int parameterIndex = GetParameterIndex(name);
                if (parameterIndex == -1) {
                    return null;
                }
                return this[parameterIndex];
            }
            set {
                int parameterIndex = GetParameterIndex(name);
                if (parameterIndex == -1) {
                    Add(value);
                }
                else {
                    this[parameterIndex] = value;
                }
            }
        }

        /// <devdoc>
        /// Occurs when any of the Parameter objects in the collection change or when the collection itself changes.
        /// </devdoc>
        public event EventHandler ParametersChanged {
            add {
                _parametersChangedHandler = (EventHandler)Delegate.Combine(_parametersChangedHandler, value);
            }
            remove {
                _parametersChangedHandler = (EventHandler)Delegate.Remove(_parametersChangedHandler, value);
            }
        }

        /// <devdoc>
        /// Adds a Parameter to the collection.
        /// </devdoc>
        public int Add(Parameter parameter) {
            return ((IList)this).Add(parameter);
        }

        /// <devdoc>
        /// Adds a Parameter to the collection with a specified name and value.
        /// </devdoc>
        public int Add(string name, string value) {
            return ((IList)this).Add(new Parameter(name, TypeCode.Empty, value));
        }

        /// <devdoc>
        /// Adds a Parameter to the collection with a specified name, type, and value.
        /// </devdoc>
        public int Add(string name, TypeCode type, string value) {
            return ((IList)this).Add(new Parameter(name, type, value));
        }

        /// <devdoc>
        /// Adds a Parameter to the collection with a specified name, database type, and value.
        /// </devdoc>
        public int Add(string name, DbType dbType, string value) {
            return ((IList)this).Add(new Parameter(name, dbType, value));
        }

        /// <devdoc>
        /// Used by Parameters to raise the ParametersChanged event.
        /// </devdoc>
        internal void CallOnParametersChanged() {
            OnParametersChanged(EventArgs.Empty);
        }

        public bool Contains(Parameter parameter) {
            return ((IList)this).Contains(parameter);
        }


        public void CopyTo(Parameter[] parameterArray, int index) {
            base.CopyTo(parameterArray, index);
        }

        /// <devdoc>
        /// Creates a known type of Parameter.
        /// </devdoc>
        protected override object CreateKnownType(int index) {
            switch (index) {
                case 0:
                    return new ControlParameter();
                case 1:
                    return new CookieParameter();
                case 2:
                    return new FormParameter();
                case 3:
                    return new Parameter();
                case 4:
                    return new QueryStringParameter();
                case 5:
                    return new SessionParameter();
                case 6:
                    return new ProfileParameter();
                default:
                    throw new ArgumentOutOfRangeException("index");
            }
        }

        /// <devdoc>
        /// Returns an ArrayList of known Parameter types.
        /// </devdoc>
        protected override Type[] GetKnownTypes() {
            return knownTypes;
        }

        /// <devdoc>
        /// Returns the index of a parameter by name.
        /// </devdoc>
        private int GetParameterIndex(string name) {
            for (int i = 0; i < Count; i++) {
                if (String.Equals(this[i].Name, name, StringComparison.OrdinalIgnoreCase)) {
                    return i;
                }
            }
            return -1;
        }

        /// <devdoc>
        /// Returns an IDictionary containing Name / Value pairs of all the parameters.
        /// </devdoc>
        public IOrderedDictionary GetValues(HttpContext context, Control control) {
            UpdateValues(context, control);

                // Create dictionary
            IOrderedDictionary valueDictionary = new OrderedDictionary();

            // Add Parameters
            foreach (Parameter param in this) {
                // For the OrderedDictionary, every parameter must have a unique name, so in some cases we have to alter them.
                string uniqueName = param.Name;
                int count = 1;
                while (valueDictionary.Contains(uniqueName)) {
                    uniqueName = param.Name + count.ToString(CultureInfo.InvariantCulture);
                    count++;
                }
                valueDictionary.Add(uniqueName, param.ParameterValue);
            }

            return valueDictionary;
        }

        public int IndexOf(Parameter parameter) {
            return ((IList)this).IndexOf(parameter);
        }

        /// <devdoc>
        /// Inserts a Parameter into the collection.
        /// </devdoc>
        public void Insert(int index, Parameter parameter) {
            ((IList)this).Insert(index, parameter);
        }

        /// <devdoc>
        /// Called when the Clear() method is complete.
        /// </devdoc>
        protected override void OnClearComplete() {
            base.OnClearComplete();

            OnParametersChanged(EventArgs.Empty);
        }

        /// <devdoc>
        /// Called when the Insert() method is starting.
        /// Adds an event handler to listen to the Parameter's ParameterChanged event.
        /// </devdoc>
        protected override void OnInsert(int index, object value) {
            base.OnInsert(index, value);

            // Set owner (we are guaranteed that it is a Parameter
            // in OnValidate).
            ((Parameter)value).SetOwner(this);
        }

        /// <devdoc>
        /// Called when the Insert() method is complete.
        /// </devdoc>
        protected override void OnInsertComplete(int index, object value) {
            base.OnInsertComplete(index, value);

            OnParametersChanged(EventArgs.Empty);
        }

        /// <devdoc>
        /// Raises the ParametersChanged event.
        /// </devdoc>
        protected virtual void OnParametersChanged(EventArgs e) {
            if (_parametersChangedHandler != null) {
                _parametersChangedHandler(this, e);
            }
        }

        /// <devdoc>
        /// Called when the Remove() method is complete.
        /// </devdoc>
        protected override void OnRemoveComplete(int index, object value) {
            base.OnRemoveComplete(index, value);

            // Clear owner
            ((Parameter)value).SetOwner(null);

            OnParametersChanged(EventArgs.Empty);
        }

        /// <devdoc>
        /// Validates that an object is a Parameter.
        /// </devdoc>
        protected override void OnValidate(object o) {
            base.OnValidate(o);

            if (!(o is Parameter))
                throw new ArgumentException(SR.GetString(SR.ParameterCollection_NotParameter), "o");
        }

        /// <devdoc>
        /// Removes a Parameter from the collection.
        /// </devdoc>
        public void Remove(Parameter parameter) {
            ((IList)this).Remove(parameter);
        }

        /// <devdoc>
        /// Removes a Parameter from the collection at a given index.
        /// </devdoc>
        public void RemoveAt(int index) {
            ((IList)this).RemoveAt(index);
        }

        /// <devdoc>
        /// Marks a Parameter as dirty so that it will record its entire state into view state.
        /// </devdoc>
        protected override void SetDirtyObject(object o) {
            ((Parameter)o).SetDirty();
        }

        /// <devdoc>
        /// Updates all parameter values to possibly raise a ParametersChanged event.
        /// </devdoc>
        public void UpdateValues(HttpContext context, Control control) {
            foreach (Parameter param in this) {
                param.UpdateValue(context, control);
            }
        }
    }
}

