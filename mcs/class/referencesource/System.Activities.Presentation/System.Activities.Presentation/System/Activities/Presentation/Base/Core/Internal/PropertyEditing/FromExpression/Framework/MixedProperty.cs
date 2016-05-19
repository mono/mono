// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework
{
    // <summary>
    // A singleton used to indicate that several objects have different values for a property.
    // </summary>
    internal sealed class MixedProperty
    {
        // Used to indicate that a retrieved property value is mixed (akin to UnsetValue.Instance).
        public static readonly object Mixed = new MixedProperty();

        private MixedProperty()
        {
        }
    }
}
