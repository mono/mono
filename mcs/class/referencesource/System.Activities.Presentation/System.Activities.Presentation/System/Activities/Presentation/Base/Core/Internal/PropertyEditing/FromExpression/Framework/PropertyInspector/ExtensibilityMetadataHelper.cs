
// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections;
    using System.Globalization;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;

    internal static class ExtensibilityMetadataHelper
    {
        // <summary>
        // Returns an instance of the PropertyValueEditor specified in the provided attribute list.
        // </summary>
        // <param name="attributes">A list of attributes. If an EditorAttribute is not specified in this collection, will return null.</param>
        // <param name="exceptionLogger">Interface for exception logging. If null, exceptions will be silently ignored.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        public static PropertyValueEditor GetValueEditor(IEnumerable attributes, IMessageLogger exceptionLogger)
        {
            PropertyValueEditor propertyValueEditor = null;
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    EditorAttribute editorAttribute = attribute as EditorAttribute;
                    if (editorAttribute != null)
                    {
                        try
                        {
                            Type editorType = Type.GetType(editorAttribute.EditorTypeName);
                            if (editorType != null && typeof(PropertyValueEditor).IsAssignableFrom(editorType))
                            {
                                propertyValueEditor = (PropertyValueEditor)Activator.CreateInstance(editorType);
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            if (exceptionLogger != null)
                            {
                                exceptionLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.ValueEditorLoadFailed, ExtensibilityMetadataHelper.GetExceptionMessage(e)));
                            }
                        }
                    }
                }
            }
            return propertyValueEditor;
        }

        // <summary>
        // Returns the type of the editor specified by the provided EditorAttribute.
        // </summary>
        // <param name="attribute">EditorAttribute that specifies a CategoryEditor type. If the type specified is not derived from CategoryEditor, or cannot be loaded, will return null.</param>
        // <param name="exceptionLogger">Interface for exception logging. If null, exceptions will be silently ignored.</param>
        // <returns></returns>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        public static Type GetCategoryEditorType(EditorAttribute attribute, IMessageLogger exceptionLogger)
        {
            try
            {
                Type editorType = Type.GetType(attribute.EditorTypeName);
                if (editorType != null && typeof(CategoryEditor).IsAssignableFrom(editorType))
                {
                    return editorType;
                }
            }
            catch (Exception e)
            {
                if (exceptionLogger != null)
                {
                    exceptionLogger.WriteLine(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.CategoryEditorTypeLoadFailed, ExtensibilityMetadataHelper.GetExceptionMessage(e)));
                }
            }
            return null;
        }

        public static string GetExceptionMessage(Exception e)
        {
            return (e.InnerException != null) ? e.InnerException.ToString() : e.Message;
        }

        public static bool IsEditorReusable(IEnumerable attributes)
        {
            bool isEditorReusable = true;
            if (attributes != null)
            {
                foreach (Attribute attribute in attributes)
                {
                    EditorReuseAttribute editorReuseAttribute = attribute as EditorReuseAttribute;
                    if (editorReuseAttribute != null)
                    {
                        isEditorReusable = editorReuseAttribute.ReuseEditor;
                        break;
                    }
                }
            }
            return isEditorReusable;
        }
    }
}

