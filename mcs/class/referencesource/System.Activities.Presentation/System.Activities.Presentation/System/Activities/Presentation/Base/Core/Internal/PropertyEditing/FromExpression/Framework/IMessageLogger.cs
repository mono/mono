// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework
{
    // <summary>
    // Central location to handle error, warning and informational messages
    // </summary>
    internal interface IMessageLogger
    {
        void Clear();
        void Write(string text);
        void WriteLine(string text);
    }
}

