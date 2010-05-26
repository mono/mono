/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Web.Script.Serialization;

    public class ViewContext : ControllerContext {

        private const string _clientValidationScript = @"<script type=""text/javascript"">
//<![CDATA[
if (!window.mvcClientValidationMetadata) {{ window.mvcClientValidationMetadata = []; }}
window.mvcClientValidationMetadata.push({0});
//]]>
</script>";

        // Some values have to be stored in HttpContext.Items in order to be propagated between calls
        // to RenderPartial(), RenderAction(), etc.
        private static readonly object _clientValidationEnabledKey = new object();
        private static readonly object _formContextKey = new object();
        private static readonly object _lastFormNumKey = new object();

        private Func<string> _formIdGenerator;

        // parameterless constructor used for mocking
        public ViewContext() {
        }

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "The virtual property setters are only to support mocking frameworks, in which case this constructor shouldn't be called anyway.")]
        public ViewContext(ControllerContext controllerContext, IView view, ViewDataDictionary viewData, TempDataDictionary tempData, TextWriter writer)
            : base(controllerContext) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (view == null) {
                throw new ArgumentNullException("view");
            }
            if (viewData == null) {
                throw new ArgumentNullException("viewData");
            }
            if (tempData == null) {
                throw new ArgumentNullException("tempData");
            }
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }

            View = view;
            ViewData = viewData;
            Writer = writer;
            TempData = tempData;
        }

        public virtual bool ClientValidationEnabled {
            get {
                return (HttpContext.Items[_clientValidationEnabledKey] as bool?).GetValueOrDefault();
            }
            set {
                HttpContext.Items[_clientValidationEnabledKey] = value;
            }
        }

        public virtual FormContext FormContext {
            get {
                return HttpContext.Items[_formContextKey] as FormContext;
            }
            set {
                HttpContext.Items[_formContextKey] = value;
            }
        }

        internal Func<string> FormIdGenerator {
            get {
                if (_formIdGenerator == null) {
                    _formIdGenerator = DefaultFormIdGenerator;
                }
                return _formIdGenerator;
            }
            set {
                _formIdGenerator = value;
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "The property setter is only here to support mocking this type and should not be called at runtime.")]
        public virtual TempDataDictionary TempData {
            get;
            set;
        }

        public virtual IView View {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "The property setter is only here to support mocking this type and should not be called at runtime.")]
        public virtual ViewDataDictionary ViewData {
            get;
            set;
        }

        public virtual TextWriter Writer {
            get;
            set;
        }

        private string DefaultFormIdGenerator() {
            int formNum = IncrementFormCount(HttpContext.Items);
            return String.Format(CultureInfo.InvariantCulture, "form{0}", formNum);
        }

        internal FormContext GetFormContextForClientValidation() {
            return (ClientValidationEnabled) ? FormContext : null;
        }

        private static int IncrementFormCount(IDictionary items) {
            object lastFormNum = items[_lastFormNumKey];
            int newFormNum = (lastFormNum != null) ? ((int)lastFormNum) + 1 : 0;
            items[_lastFormNumKey] = newFormNum;
            return newFormNum;
        }

        public void OutputClientValidation() {
            FormContext formContext = GetFormContextForClientValidation();
            if (formContext == null) {
                return; // do nothing
            }
                        
            string scriptWithCorrectNewLines = _clientValidationScript.Replace("\r\n", Environment.NewLine);
            string validationJson = formContext.GetJsonValidationMetadata();
            string formatted = String.Format(CultureInfo.InvariantCulture, scriptWithCorrectNewLines, validationJson);

            Writer.Write(formatted);
            FormContext = null;
        }

    }
}
