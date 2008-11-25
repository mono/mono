// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************

using System.IO;

namespace NUnit.Core
{
    /// <summary>
    /// Abstract base for classes that capture text output
    /// and redirect it to a TextWriter.
    /// </summary>
    public abstract class TextCapture
    {
        #region Private Fields
        /// <summary>
        /// True if capture is enabled
        /// </summary>
        private bool enabled;

        /// <summary>
        /// The TextWriter to which text is redirected
        /// </summary>
        private TextWriter writer;
        #endregion

        #region Properties
        /// <summary>
        /// The TextWriter to which text is redirected
        /// </summary>
        public TextWriter Writer
        {
            get { return writer; }
            set
            {
                writer = value;

                if (writer != null && enabled)
                    StartCapture();
            }
        }

        /// <summary>
        /// Controls whether text is captured or not
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set
            {
                if (enabled != value)
                {
                    if (writer != null && enabled)
                        StopCapture();

                    enabled = value;

                    if (writer != null && enabled && DefaultThreshold != "Off")
                        StartCapture();
                }
            }
        }

        /// <summary>
        /// Returns the default threshold value, which represents
        /// the degree of verbosity of the output text stream.
        /// Returns "None" in the base class. Derived classes that
        /// support verbosity levels should override it.
        /// </summary>
        public virtual string DefaultThreshold
        {
            get { return "None"; }
        }
        #endregion

        #region Abstract Members
        /// <summary>
        /// Override this to perform whatever actions are needed
        /// to start capturing text and sending it to the Writer.
        /// </summary>
        protected abstract void StartCapture();

        /// <summary>
        /// Override this to perform whatever actions are needed
        /// to flush remaining output and stop capturing text.
        /// The Writer should not be changed, allowing capture
        /// to be restarted at a future point.
        /// </summary>
        protected abstract void StopCapture();
        #endregion
    }

}
