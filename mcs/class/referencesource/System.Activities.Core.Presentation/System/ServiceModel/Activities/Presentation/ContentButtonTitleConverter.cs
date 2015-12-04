//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Runtime;
    using System.Activities.Core.Presentation;
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Windows.Data;

    sealed class ContentButtonTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object content = value;
            if (content != null && content is ModelItem)
            {
                content = ((ModelItem)content).GetCurrentValue();
            }
            if (content == null)
            {
                return SR.DefineContent;
            }
            else
            {
                //string contentTypeName = content.GetType().Name;
                if (content is ReceiveMessageContent || content is SendMessageContent)
                {
                    return SR.ViewMessageContent;
                }
                else if (content is ReceiveParametersContent || content is SendParametersContent)
                {
                    return SR.ViewParameterContent;
                }
                else
                {
                    Fx.Assert(false, "Content must be of either ReceiveMessageContent, ReceiveParametersContent, SendMessageContent or SendParametersContent.");
                    return null;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
