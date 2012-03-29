namespace System.Web.Mvc.Html {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc.Resources;
    using System.Web.UI.WebControls;

    internal static class DefaultDisplayTemplates {
        internal static string BooleanTemplate(HtmlHelper html) {
            bool? value = null;
            if (html.ViewContext.ViewData.Model != null) {
                value = Convert.ToBoolean(html.ViewContext.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return html.ViewContext.ViewData.ModelMetadata.IsNullableValueType
                        ? BooleanTemplateDropDownList(value)
                        : BooleanTemplateCheckbox(value ?? false);
        }

        private static string BooleanTemplateCheckbox(bool value) {
            TagBuilder inputTag = new TagBuilder("input");
            inputTag.AddCssClass("check-box");
            inputTag.Attributes["disabled"] = "disabled";
            inputTag.Attributes["type"] = "checkbox";
            if (value) {
                inputTag.Attributes["checked"] = "checked";
            }

            return inputTag.ToString(TagRenderMode.SelfClosing);
        }

        private static string BooleanTemplateDropDownList(bool? value) {
            StringBuilder builder = new StringBuilder();

            TagBuilder selectTag = new TagBuilder("select");
            selectTag.AddCssClass("list-box");
            selectTag.AddCssClass("tri-state");
            selectTag.Attributes["disabled"] = "disabled";
            builder.Append(selectTag.ToString(TagRenderMode.StartTag));

            foreach (SelectListItem item in DefaultEditorTemplates.TriStateValues(value)) {
                builder.Append(SelectExtensions.ListItemToOption(item));
            }

            builder.Append(selectTag.ToString(TagRenderMode.EndTag));
            return builder.ToString();
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
                    string output = templateHelper(html, metadata, fieldName, null /* templateName */, DataBoundControlMode.ReadOnly, null /* additionalViewData */);
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

        internal static string EmailAddressTemplate(HtmlHelper html) {
            return String.Format(CultureInfo.InvariantCulture,
                                 "<a href=\"mailto:{0}\">{1}</a>",
                                 html.AttributeEncode(html.ViewContext.ViewData.Model),
                                 html.Encode(html.ViewContext.ViewData.TemplateInfo.FormattedModelValue));
        }

        internal static string HiddenInputTemplate(HtmlHelper html) {
            if (html.ViewContext.ViewData.ModelMetadata.HideSurroundingHtml) {
                return String.Empty;
            }
            return StringTemplate(html);
        }

        internal static string HtmlTemplate(HtmlHelper html) {
            return html.ViewContext.ViewData.TemplateInfo.FormattedModelValue.ToString();
        }

        internal static string ObjectTemplate(HtmlHelper html) {
            return ObjectTemplate(html, TemplateHelpers.TemplateHelper);
        }

        internal static string ObjectTemplate(HtmlHelper html, TemplateHelpers.TemplateHelperDelegate templateHelper) {
            ViewDataDictionary viewData = html.ViewContext.ViewData;
            TemplateInfo templateInfo = viewData.TemplateInfo;
            ModelMetadata modelMetadata = viewData.ModelMetadata;
            StringBuilder builder = new StringBuilder();

            if (modelMetadata.Model == null) {    // DDB #225237
                return modelMetadata.NullDisplayText;
            }

            if (templateInfo.TemplateDepth > 1) {    // DDB #224751
                return modelMetadata.SimpleDisplayText;
            }

            foreach (ModelMetadata propertyMetadata in modelMetadata.Properties.Where(pm => ShouldShow(pm, templateInfo))) {
                if (!propertyMetadata.HideSurroundingHtml) {
                    string label = propertyMetadata.GetDisplayName();
                    if (!String.IsNullOrEmpty(label)) {
                        builder.AppendFormat(CultureInfo.InvariantCulture, "<div class=\"display-label\">{0}</div>", label);
                        builder.AppendLine();
                    }

                    builder.Append("<div class=\"display-field\">");
                }

                builder.Append(templateHelper(html, propertyMetadata, propertyMetadata.PropertyName, null /* templateName */, DataBoundControlMode.ReadOnly, null /* additionalViewData */));

                if (!propertyMetadata.HideSurroundingHtml) {
                    builder.AppendLine("</div>");
                }
            }

            return builder.ToString();
        }

        private static bool ShouldShow(ModelMetadata metadata, TemplateInfo templateInfo) {
            return
                metadata.ShowForDisplay
#if !MONO
                && metadata.ModelType != typeof(EntityState)
#endif
                && !metadata.IsComplexType
                && !templateInfo.Visited(metadata);
        }

        internal static string StringTemplate(HtmlHelper html) {
            return html.Encode(html.ViewContext.ViewData.TemplateInfo.FormattedModelValue);
        }

        internal static string UrlTemplate(HtmlHelper html) {
            return String.Format(CultureInfo.InvariantCulture,
                                 "<a href=\"{0}\">{1}</a>",
                                 html.AttributeEncode(html.ViewContext.ViewData.Model),
                                 html.Encode(html.ViewContext.ViewData.TemplateInfo.FormattedModelValue));
        }
    }
}
