//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Windows.Controls;

    static class ComboBoxHelper
    {
        public static bool ShouldFilterUnnecessaryComboBoxEvent(ComboBox comboBox)
        {
            return comboBox != null && comboBox.IsDropDownOpen;
        }

        public static void SynchronizeComboBoxSelection(ComboBox comboBox, string value)
        {
            foreach (string item in comboBox.Items)
            {
                if (string.Equals(item, value))
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
            comboBox.SelectedIndex = -1;
        }
    }
}
