//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Runtime;

    internal class VersionEditorViewModel : ViewModel
    {
        private IVersionEditor editor;
        private Version version;

        public VersionEditorViewModel(IVersionEditor editor)
        {
            Fx.Assert(editor != null, "editor should not be null");
            this.editor = editor;
        }

        public string VersionText
        {
            get
            {
                if (this.version == null)
                {
                    return null;
                }

                return this.version.ToString();
            }

            set
            {
                if (this.VersionText != value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.Version = null;
                        return;
                    }

                    Exception exception = null;

                    try
                    {
                        this.Version = Version.Parse(value);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        exception = ex;
                    }
                    catch (ArgumentException ex)
                    {
                        exception = ex;
                    }
                    catch (FormatException ex)
                    {
                        exception = ex;
                    }
                    catch (OverflowException ex)
                    {
                        exception = ex;
                    }

                    if (exception != null)
                    {
                        this.editor.ShowErrorMessage(exception.Message);

                        // update UI to its old value
                        this.NotifyPropertyChanged("VersionText");
                    }
                }
            }
        }

        public Version Version
        {
            get
            {
                return this.version;
            }

            set
            {
                if (this.version != value)
                {
                    this.version = value;
                    this.NotifyPropertyChanged("Version");
                    this.NotifyPropertyChanged("VersionText");
                }
            }
        }
    }
}
