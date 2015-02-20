//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Services
{
    /// <summary>
    /// model change type
    /// </summary>
    public enum ModelChangeType
    {
        /// <summary>
        /// none operation
        /// </summary>
        None,

        /// <summary>
        /// a property is changed
        /// </summary>
        PropertyChanged,

        /// <summary>
        /// a collection item is added
        /// </summary>
        CollectionItemAdded,

        /// <summary>
        /// a collection item is removed
        /// </summary>
        CollectionItemRemoved,

        /// <summary>
        /// a dictionary key value is added
        /// </summary>
        DictionaryKeyValueAdded,

        /// <summary>
        /// a dictionary key value is removed
        /// </summary>
        DictionaryKeyValueRemoved,

        /// <summary>
        /// a dictionay value is changed
        /// </summary>
        DictionaryValueChanged,
    }
}
