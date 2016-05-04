// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Model
{
    using System.Activities.Presentation.Model;

    /// <summary>
    /// Interface for DictionaryItemsCollection
    /// </summary>
    internal interface IItemsCollection
    {
        bool ShouldUpdateDictionary { get; set; }

        ModelItemDictionaryImpl ModelDictionary { get; set; }
    }
}
