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

namespace System.Web.Mvc.Html {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;

    public static class InputExtensions {
        public static string CheckBox(this HtmlHelper htmlHelper, string name) {
            return CheckBox(htmlHelper, name, (object)null /* htmlAttributes */);
        }

        public static string CheckBox(this HtmlHelper htmlHelper, string name, bool isChecked) {
            return CheckBox(htmlHelper, name, isChecked, (object)null /* htmlAttributes */);
        }

        public static string CheckBox(this HtmlHelper htmlHelper, string name, bool isChecked, object htmlAttributes) {
            return CheckBox(htmlHelper, name, isChecked, new RouteValueDictionary(htmlAttributes));
        }

        public static string CheckBox(this HtmlHelper htmlHelper, string name, object htmlAttributes) {
            return CheckBox(htmlHelper, name, new RouteValueDictionary(htmlAttributes));
        }

        public static string CheckBox(this HtmlHelper htmlHelper, string name, IDictionary<string, object> htmlAttributes) {
            return htmlHelper.InputHelper(InputType.CheckBox, name, "true", true /* useViewData */, false /* isChecked */, true /* setId */, false /* isExplicitValue */, htmlAttributes);
        }

        public static string CheckBox(this HtmlHelper htmlHelper, string name, bool isChecked, IDictionary<string, object> htmlAttributes) {
            // checked is an explicit parameter, but the value attribute is implicit so the dictionary's must take
            // precedence.
            RouteValueDictionary attributes = htmlAttributes == null ? new RouteValueDictionary() : new RouteValueDictionary(htmlAttributes);
            attributes.Remove("checked");
            return htmlHelper.InputHelper(InputType.CheckBox, name, "true", false /* useViewData */, isChecked, true /* setId */, false /* isExplicitValue */, attributes);
        }

        public static string Hidden(this HtmlHelper htmlHelper, string name) {
            return Hidden(htmlHelper, name, null /* value */);
        }

        public static string Hidden(this HtmlHelper htmlHelper, string name, object value) {
            return Hidden(htmlHelper, name, value, (object)null /* hmtlAttributes */);
        }

        public static string Hidden(this HtmlHelper htmlHelper, string name, object value, object htmlAttributes) {
            return Hidden(htmlHelper, name, value, new RouteValueDictionary(htmlAttributes));
        }

        public static string Hidden(this HtmlHelper htmlHelper, string name, object value, IDictionary<string, object> htmlAttributes) {
            return InputHelper(htmlHelper, InputType.Hidden, name, value, (value == null) /* useViewData */, false /* isChecked */, true /* setId */, true /* isExplicitValue */, htmlAttributes);
        }

        public static string Password(this HtmlHelper htmlHelper, string name) {
            return Password(htmlHelper, name, null /* value */);
        }

        public static string Password(this HtmlHelper htmlHelper, string name, object value) {
            return Password(htmlHelper, name, value, (object)null /* htmlAttributes */);
        }

        public static string Password(this HtmlHelper htmlHelper, string name, object value, object htmlAttributes) {
            return Password(htmlHelper, name, value, new RouteValueDictionary(htmlAttributes));
        }

        public static string Password(this HtmlHelper htmlHelper, string name, object value, IDictionary<string, object> htmlAttributes) {
            return InputHelper(htmlHelper, InputType.Password, name, value, false /* useViewData */, false /* isChecked */, true /* setId */, true /* isExplicitValue */, htmlAttributes);
        }

        public static string RadioButton(this HtmlHelper htmlHelper, string name, object value) {
            return RadioButton(htmlHelper, name, value, (object)null /* htmlAttributes */);
        }

        public static string RadioButton(this HtmlHelper htmlHelper, string name, object value, object htmlAttributes) {
            return RadioButton(htmlHelper, name, value, new RouteValueDictionary(htmlAttributes));
        }

        public static string RadioButton(this HtmlHelper htmlHelper, string name, object value, IDictionary<string, object> htmlAttributes) {
            // Determine whether or not to render the checked attribute based on the contents of ViewData.
            string valueString = Convert.ToString(value, CultureInfo.CurrentCulture);
            bool isChecked = (!String.IsNullOrEmpty(name)) && (String.Equals(htmlHelper.EvalString(name), valueString, StringComparison.OrdinalIgnoreCase));
            // checked attributes is implicit, so we need to ensure that the dictionary takes precedence.
            RouteValueDictionary attributes = htmlAttributes == null ? new RouteValueDictionary() : new RouteValueDictionary(htmlAttributes);
            if (attributes.ContainsKey("checked")) {
                return htmlHelper.InputHelper(InputType.Radio, name, value, false, false, true, true /* isExplicitValue */, attributes);
            }

            return RadioButton(htmlHelper, name, value, isChecked, htmlAttributes);
        }

        public static string RadioButton(this HtmlHelper htmlHelper, string name, object value, bool isChecked) {
            return RadioButton(htmlHelper, name, value, isChecked, (object)null /* htmlAttributes */);
        }

        public static string RadioButton(this HtmlHelper htmlHelper, string name, object value, bool isChecked, object htmlAttributes) {
            return RadioButton(htmlHelper, name, value, isChecked, new RouteValueDictionary(htmlAttributes));
        }

        public static string RadioButton(this HtmlHelper htmlHelper, string name, object value, bool isChecked, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            // checked attribute is an explicit parameter so it takes precedence.
            RouteValueDictionary attributes = htmlAttributes == null ? new RouteValueDictionary() : new RouteValueDictionary(htmlAttributes);
            attributes.Remove("checked");
            return htmlHelper.InputHelper(InputType.Radio, name, value, false, isChecked, true, true /* isExplicitValue */, attributes);
        }

        public static string TextBox(this HtmlHelper htmlHelper, string name) {
            return TextBox(htmlHelper, name, null /* value */);
        }

        public static string TextBox(this HtmlHelper htmlHelper, string name, object value) {
            return TextBox(htmlHelper, name, value, (object)null /* htmlAttributes */);
        }

        public static string TextBox(this HtmlHelper htmlHelper, string name, object value, object htmlAttributes) {
            return TextBox(htmlHelper, name, value, new RouteValueDictionary(htmlAttributes));
        }

        public static string TextBox(this HtmlHelper htmlHelper, string name, object value, IDictionary<string, object> htmlAttributes) {
            return InputHelper(htmlHelper, InputType.Text, name, value, (value == null) /* useViewData */, false /* isChecked */, true /* setId */, true /* isExplicitValue */, htmlAttributes);
        }

        private static string InputHelper(this HtmlHelper htmlHelper, InputType inputType, string name, object value, bool useViewData, bool isChecked, bool setId, bool isExplicitValue, IDictionary<string, object> htmlAttributes) {
            if (String.IsNullOrEmpty(name)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }

            TagBuilder tagBuilder = new TagBuilder("input");
            tagBuilder.MergeAttributes(htmlAttributes);
            tagBuilder.MergeAttribute("type", HtmlHelper.GetInputTypeString(inputType));
            tagBuilder.MergeAttribute("name", name, true);

            string valueParameter = Convert.ToString(value, CultureInfo.CurrentCulture);
            bool usedModelState = false;

            switch (inputType) {
                case InputType.CheckBox:
                    bool? modelStateWasChecked = htmlHelper.GetModelStateValue(name, typeof(bool)) as bool?;
                    if (modelStateWasChecked.HasValue) {
                        isChecked = modelStateWasChecked.Value;
                        usedModelState = true;
                    }
                    goto case InputType.Radio;
                case InputType.Radio:
                    if (!usedModelState) {
                        string modelStateValue = htmlHelper.GetModelStateValue(name, typeof(string)) as string;
                        if (modelStateValue != null) {
                            isChecked = String.Equals(modelStateValue, valueParameter, StringComparison.Ordinal);
                            usedModelState = true;
                        }
                    }
                    if (!usedModelState && useViewData) {
                        isChecked = htmlHelper.EvalBoolean(name);
                    }
                    if (isChecked) {
                        tagBuilder.MergeAttribute("checked", "checked");
                    }
                    tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);
                    break;
                case InputType.Password:
                    if (value != null) {
                        tagBuilder.MergeAttribute("value", valueParameter, isExplicitValue);
                    }
                    break;
                default:
                    string attemptedValue = (string)htmlHelper.GetModelStateValue(name, typeof(string));
                    tagBuilder.MergeAttribute("value", attemptedValue ?? ((useViewData) ? htmlHelper.EvalString(name) : valueParameter), isExplicitValue);
                    break;
            }

            if (setId) {
                tagBuilder.GenerateId(name);
            }

            // If there are any errors for a named field, we add the css attribute.
            ModelState modelState;
            if (htmlHelper.ViewData.ModelState.TryGetValue(name, out modelState)) {
                if (modelState.Errors.Count > 0) {
                    tagBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            if (inputType == InputType.CheckBox) {
                // Render an additional <input type="hidden".../> for checkboxes. This
                // addresses scenarios where unchecked checkboxes are not sent in the request.
                // Sending a hidden input makes it possible to know that the checkbox was present
                // on the page when the request was submitted.
                StringBuilder inputItemBuilder = new StringBuilder();
                inputItemBuilder.Append(tagBuilder.ToString(TagRenderMode.SelfClosing));

                TagBuilder hiddenInput = new TagBuilder("input");
                hiddenInput.MergeAttribute("type", HtmlHelper.GetInputTypeString(InputType.Hidden));
                hiddenInput.MergeAttribute("name", name);
                hiddenInput.MergeAttribute("value", "false");
                inputItemBuilder.Append(hiddenInput.ToString(TagRenderMode.SelfClosing));
                return inputItemBuilder.ToString();
            }

            return tagBuilder.ToString(TagRenderMode.SelfClosing);
        }
    }
}
