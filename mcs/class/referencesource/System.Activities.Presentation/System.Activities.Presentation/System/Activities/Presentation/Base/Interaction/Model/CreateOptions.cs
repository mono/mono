
namespace System.Activities.Presentation.Model {

    using System;

    /// <summary>
    /// The CreateOptions flags are passed into ModelFactory
    /// to dictate how to create a new item.  
    /// </summary>
    [Flags]
    public enum CreateOptions {

        /// <summary>
        /// Just creates the object and does not perform
        /// any operations on it.  This is the default.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Creates the item and asks the object to perform
        /// any default initialization.  This flag is generally
        /// passed in when a new control or object is being
        /// created by a user.
        /// </summary>
        InitializeDefaults = 0x01
    }
}
