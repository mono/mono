//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System.Windows.Controls;
    using System.Windows.Input;

    // <summary>
    // WPF's RadioButton responds to Space key to trigger selection, but
    // not the Enter or Return keys.  This class responds to both.
    // </summary>
    internal class KeyboardEnabledRadioButton : RadioButton 
    {
        protected override void OnKeyDown(KeyEventArgs e) 
        {
            if (e.Key == Key.Enter ||
                e.Key == Key.Return) 
            {

                this.IsChecked = true;
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}
