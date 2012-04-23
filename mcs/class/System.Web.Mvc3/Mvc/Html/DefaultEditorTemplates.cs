namespace System.Web.Mvc.Html {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Linq;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc.Resources;
    using System.Web.UI.WebControls;

    internal static class DefaultEditorTemplates {
        internal static string BooleanTemplate(HtmlHelper html) {
            bool? value = null;
            if (html.ViewContext.ViewData.Model != null) {
                value = Convert.ToBoolean(html.ViewContext.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return html.ViewContext.ViewData.ModelMetadata.IsNullableValueType
                        ? BooleanTemplateDropDownList(html, value)
                        : BooleanTemplateCheckbox(html, value ?? false);
        }

        private static string BooleanTemplateCheckbox(HtmlHelper html, bool value) {
            return html.CheckBox(String.Empty, value, CreateHtmlAttributes("check-box")).ToHtmlString();
        }

        private static string BooleanTemplateDropDownList(HtmlHelper html, bool? value) {
            return html.DropDownList(String.Empty, TriStateValues(value), CreateHtmlAttributes("list-box tri-state")).ToHtmlString();

        }

        internal static string CollectionTemplate(HtmlHelper html) {
            return CollectionTemplate(html, TemplateHelpers.TemplateHelper);
        }

        internal static string CollectionTemplate(HtmlHelper html, TemplateHelpers.TemplateHelperDelegate templateHelper) {
            object model = html.ViewContext.ViewData.ModelMetadata.Model;
            if (model == null) {
                return String.Empty;
            }

            IEnumerable collection = model as IEnumerable;
            if (collection == null) {
                throw new InvalidOperationException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.Templates_TypeMustImplementIEnumerable,
                        model.GetType().FullName
                    )
                );
            }

            Type typeInCollection = typeof(string);
            Type genericEnumerableType = TypeHelpers.ExtractGenericInterface(collection.GetType(), typeof(IEnumerable<>));
            if (genericEnumerableType != null) {
                typeInCollection = genericEnumerableType.GetGenericArguments()[0];
            }
            bool typeInCollectionIsNullableValueType = TypeHelpers.IsNullableValueType(typeInCollection);

            string oldPrefix = html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix;

            try {
                html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = String.Empty;

                string fieldNameBase = oldPrefix;
                StringBuilder result = new StringBuilder();
                int index = 0;

                foreach (object item in collection) {
                    Type itemType = typeInCollection;
                    if (item != null && !typeInCollectionIsNullableValueType) {
                        itemType = item.GetType();
                    }
                    ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForType(() => item, itemType);
                    string fieldName = String.Format(CultureInfo.InvariantCulture, "{0}[{1}]", fieldNameBase, index++);
                    string output = templateHelper(html, metadata, fieldName, null /* templateName */, DataBoundControlMode.Edit, null /* additionalViewData */);
                    result.Append(output);
                }

                return result.ToString();
            }
            finally {
                html.ViewContext.ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        internal static string DecimalTemplate(HtmlHelper html) {
            if (html.ViewContext.ViewData.TemplateInfo.FormattedModelValue == html.ViewContext.ViewData.ModelMetadata.Model) {
                html.ViewContext.ViewData.TemplateInfo.FormattedModelValue = String.Format(CultureInfo.CurrentCulture, "{0:0.00}", html.ViewContext.ViewData.ModelMetadata.Model);
            }

            return StringTemplate(html);
        }

        internal static string HiddenInputTemplate(HtmlHelper html) {
            string result;

            if (html.ViewContext.ViewData.ModelMetadata.HideSurroundingHtml) {
                result = String.Empty;
            }
            else {
                result = DefaultDisplayTemplates.StringTemplate(html);
            }

            object model = html.ViewContext.ViewData.Model;

            Binary modelAsBinary = model as Binary;
            if (modelAsBinary != null) {
                model = Convert.ToBase64String(modelAsBinary.ToArray());
            }
            else {
                byte[] modelAsByteArray = model as byte[];
                if (modelAsByteArray != null) {
                    model = Convert.ToBase64String(modelAsByteArray);
                }
            }

            result += html.Hidden(String.Empty, model).ToHtmlString();
            return result;
        }

        internal static string MultilineTextTemplate(HtmlHelper html) {
            return html.TextArea(String.Empty,
                                 html.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString(),
                                 0 /* rows */, 0 /* columns */,
                                 CreateHtmlAttributes("text-box multi-line")).ToHtmlString();
        }

        private static IDictionary<string, object> CreateHtmlAttributes(string className) {
            return new Dictionary<string, object>() {
                { "class", className }
            };
        }

        internal static string ObjectTemplate(HtmlHelper html) {
            return ObjectTemplate(html, TemplateHelpers.TemplateHelper);
        }

        internal static string ObjectTemplate(HtmlHelper html, TemplateHelpers.TemplateHelperDelegate templateHelper) {
            ViewDataDictionary viewData = html.ViewContext.ViewData;
            TemplateInfo templateInfo = viewData.TemplateInfo;
            ModelMetadata modelMetadata = viewData.ModelMetadata;
            StringBuilder builder = new StringBuilder();

            if (templateInfo.TemplateDepth > 1) {    // DDB #224751
                return modelMetadata.Model == null ? modelMetadata.NullDisplayText : modelMetadata.SimpleDisplayText;
            }

            foreach (ModelMetadata propertyMetadata in modelMetadata.Properties.Where(pm => ShouldShow(pm, templateInfo))) {
                if (!propertyMetadata.HideSurroundingHtml) {
                    string label = LabelExtensions.LabelHelper(html, propertyMetadata, propertyMetadata.PropertyName).ToHtmlString();
                    if (!String.IsNullOrEmpty(label)) {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"editor-label\">{0}</div>\r\n", label);
                    }

                    builder.Append("<div class=\"editor-field\">");
                }

                builder.Append(templateHelper(html, propertyMetadata, propertyMetadata.PropertyName, null /* templateName */, DataBoundControlMode.Edit, null /* additionalViewData */));

                if (!propertyMetadata.HideSurroundingHtml) {
                    builder.Append(" ");
                    builder.Append(html.ValidationMessage(propertyMetadata.PropertyName));
                    builder.Append("</div>\r\n");
                }
            }

            return builder.ToString();
        }

        internal static string PasswordTemplate(HtmlHelper html) {
            return html.Password(String.Empty,
                                 html.ViewContext.ViewData.TemplateInfo.FormattedModelValue,
                                 CreateHtmlAttributes("text-box single-line password")).ToHtmlString();
        }

        private static bool ShouldShow(ModelMetadata metadata, TemplateInfo templateInfo) {
            return
                metadata.ShowForEdit
#if !MONO
                && metadata.ModelType != typeof(EntityState)
#endif
                && !metadata.IsComplexType
                && !templateInfo.Visited(metadata);
        }

        internal static string StringTemplate(HtmlHelper html) {
            return html.TextBox(String.Empty,
                                html.ViewContext.ViewData.TemplateInfo.FormattedModelValue,
                                CreateHtmlAttributes("text-box single-line")).ToHtmlString();
        }

        internal static List<SelectListItem> TriStateValues(bool? value) {
            return new List<SelectListItem> {
                new SelectListItem { Text = MvcResources.Common_TriState_NotSet, Value = String.Empty, Selected = !value.HasValue },
                new SelectListItem { Text = MvcResources.Common_TriState_True, Value = "true", Selected = value.HasValue && value.Value },
                new SelectListItem { Text = MvcResources.Common_TriState_False, Value = "false", Selected = value.HasValue && !value.Value },
            };
        }
    }
}
