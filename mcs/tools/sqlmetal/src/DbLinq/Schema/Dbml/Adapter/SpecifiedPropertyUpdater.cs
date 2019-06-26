#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System.ComponentModel;

namespace DbLinq.Schema.Dbml.Adapter
{
    /// <summary>
    /// The schema generates *Specified properties that we must set when the related property changes
    /// So we use the notification to set the value
    /// </summary>
    internal static class SpecifiedPropertyUpdater
    {
        /// <summary>
        /// Registers the specified notify.
        /// </summary>
        /// <param name="notify">The notify.</param>
        public static void Register(INotifyPropertyChanged notify)
        {
            notify.PropertyChanged += Notify_PropertyChanged;
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Notify control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void Notify_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // if there is a property for us
            var specifiedPropertyInfo = sender.GetType().GetProperty(e.PropertyName + "Specified");
            // then we set it to true
            if (specifiedPropertyInfo != null)
                specifiedPropertyInfo.GetSetMethod().Invoke(sender, new object[] {true});
        }
    }
}