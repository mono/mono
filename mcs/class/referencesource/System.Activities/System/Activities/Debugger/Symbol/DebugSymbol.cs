//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Debugger.Symbol
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Xaml;
    using System.Runtime;


    [Fx.Tag.XamlVisible(false)]
    public static class DebugSymbol
    {
        static Type attachingTypeName = typeof(DebugSymbol);

        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.DoNotDeclareReadOnlyMutableReferenceTypes)]
        public static readonly AttachableMemberIdentifier SymbolName = new AttachableMemberIdentifier(attachingTypeName, "Symbol");


        [Fx.Tag.InheritThrows(From = "SetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static void SetSymbol(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, SymbolName, value);
        }

        [Fx.Tag.InheritThrows(From = "TryGetProperty", FromDeclaringType = typeof(AttachablePropertyServices))]
        public static object GetSymbol(object instance)
        {
            string value;
            if (AttachablePropertyServices.TryGetProperty(instance, SymbolName, out value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
