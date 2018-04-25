//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;

    delegate bool CaseKeyValidationCallbackDelegate(object obj, out string reason);

    interface ICaseKeyBoxView
    {
        // Some view level functionalities required (i.e. cannot be done by data binding)
        void RegainFocus();

        // Allow ViewModel to raise View events
        void OnValueCommitted();
        void OnEditCancelled();

        // Pass public interface of this control to the ViewModel
        bool DisplayHintText { get; }
        object Value { get; set; }
        Type ValueType { get; }
        CaseKeyValidationCallbackDelegate CaseKeyValidationCallback { get; }
    }
}
