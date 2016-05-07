// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------

//Cider comment:
// - Integration of Expression error messages. The actual resource strings are in Resources.resx
// - Rather than alter the classes we ported from Blend I created this file to redirect to Resources.resx


//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\ExceptionStringTable.resx
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework
{
    internal class ExceptionStringTable
    {
        //FXCop compliance
        private ExceptionStringTable()
        {
        }

        internal static string CanOnlySetFocusScopePriorityOnAnElementThatIsAFocusScope
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_CanOnlySetFocusScopePriorityOnAnElementThatIsAFocusScope; } }
        internal static string CategoryIconLoadFailed
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_CategoryIconLoadFailed; } }
        internal static string CategoryEditorTypeLoadFailed
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_CategoryEditorTypeLoadFailed; } }
        internal static string MethodOrOperationIsNotImplemented
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_MethodOrOperationIsNotImplemented; } }
        internal static string NewItemFactoryIconLoadFailed
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_NewItemFactoryIconLoadFailed; } }
        internal static string NoConvertBackForValueToIconConverter
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_NoConvertBackForValueToIconConverter; } }
        internal static string SwitchConverterIsOneWay
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_SwitchConverterIsOneWay; } }
        internal static string UnexpectedImageSourceType
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_UnexpectedImageSourceType; } }
        internal static string UnexpectedDrawingType
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_UnexpectedDrawingType; } }
        internal static string UnexpectedBrushType
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_UnexpectedBrushType; } }
        internal static string ValueEditorLoadFailed
        { get { return System.Activities.Presentation.Internal.Properties.Resources.FromExpression_ValueEditorLoadFailed; } }
    }
}

