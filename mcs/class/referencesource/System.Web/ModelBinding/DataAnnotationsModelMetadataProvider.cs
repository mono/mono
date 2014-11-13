namespace System.Web.ModelBinding {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;

    public class DataAnnotationsModelMetadataProvider : AssociatedMetadataProvider {

        protected override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName) {
            List<Attribute> attributeList = new List<Attribute>(attributes);
            DisplayColumnAttribute displayColumnAttribute = attributeList.OfType<DisplayColumnAttribute>().FirstOrDefault();
            DataAnnotationsModelMetadata result = new DataAnnotationsModelMetadata(this, containerType, modelAccessor, modelType, propertyName, displayColumnAttribute);

#if UNDEF
            // Do [HiddenInput] before [UIHint], so you can override the template hint
            HiddenInputAttribute hiddenInputAttribute = attributeList.OfType<HiddenInputAttribute>().FirstOrDefault();
            if (hiddenInputAttribute != null) {
                result.TemplateHint = "HiddenInput";
                result.HideSurroundingHtml = !hiddenInputAttribute.DisplayValue;
            }
#endif

            // We prefer [UIHint("...", PresentationLayer = "MVC")] but will fall back to [UIHint("...")]
            IEnumerable<UIHintAttribute> uiHintAttributes = attributeList.OfType<UIHintAttribute>();
            UIHintAttribute uiHintAttribute = uiHintAttributes.FirstOrDefault(a => String.Equals(a.PresentationLayer, "MVC", StringComparison.OrdinalIgnoreCase))
                                           ?? uiHintAttributes.FirstOrDefault(a => String.IsNullOrEmpty(a.PresentationLayer));
            if (uiHintAttribute != null) {
                result.TemplateHint = uiHintAttribute.UIHint;
            }

            DataTypeAttribute dataTypeAttribute = attributeList.OfType<DataTypeAttribute>().FirstOrDefault();
            if (dataTypeAttribute != null) {
                result.DataTypeName = dataTypeAttribute.ToDataTypeName();
            }

            EditableAttribute editable = attributes.OfType<EditableAttribute>().FirstOrDefault();
            if (editable != null) {
                result.IsReadOnly = !editable.AllowEdit;
            }
            else {
                ReadOnlyAttribute readOnlyAttribute = attributeList.OfType<ReadOnlyAttribute>().FirstOrDefault();
                if (readOnlyAttribute != null) {
                    result.IsReadOnly = readOnlyAttribute.IsReadOnly;
                }
            }

            DisplayFormatAttribute displayFormatAttribute = attributeList.OfType<DisplayFormatAttribute>().FirstOrDefault();
            if (displayFormatAttribute == null && dataTypeAttribute != null) {
                displayFormatAttribute = dataTypeAttribute.DisplayFormat;
            }
            if (displayFormatAttribute != null) {
                result.NullDisplayText = displayFormatAttribute.NullDisplayText;
                result.DisplayFormatString = displayFormatAttribute.DataFormatString;
                result.ConvertEmptyStringToNull = displayFormatAttribute.ConvertEmptyStringToNull;

                if (displayFormatAttribute.ApplyFormatInEditMode) {
                    result.EditFormatString = displayFormatAttribute.DataFormatString;
                }

                if (!displayFormatAttribute.HtmlEncode && String.IsNullOrWhiteSpace(result.DataTypeName)) {
                    result.DataTypeName = DataTypeUtil.HtmlTypeName;
                }
            }

            ScaffoldColumnAttribute scaffoldColumnAttribute = attributeList.OfType<ScaffoldColumnAttribute>().FirstOrDefault();
            if (scaffoldColumnAttribute != null) {
                result.ShowForDisplay = result.ShowForEdit = scaffoldColumnAttribute.Scaffold;
            }

            DisplayAttribute display = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            string name = null;
            if (display != null) {
                result.Description = display.GetDescription();
                result.ShortDisplayName = display.GetShortName();
                result.Watermark = display.GetPrompt();
                result.Order = display.GetOrder() ?? ModelMetadata.DefaultOrder;

                name = display.GetName();
            }

            if (name != null) {
                result.DisplayName = name;
            }
            else {
                DisplayNameAttribute displayNameAttribute = attributeList.OfType<DisplayNameAttribute>().FirstOrDefault();
                if (displayNameAttribute != null) {
                    result.DisplayName = displayNameAttribute.DisplayName;
                }
            }

            RequiredAttribute requiredAttribute = attributeList.OfType<RequiredAttribute>().FirstOrDefault();
            if (requiredAttribute != null) {
                result.IsRequired = true;
            }

            return result;
        }
    }
}
