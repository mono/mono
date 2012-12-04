// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Json
{
    /// <summary>
    /// Specifies the event type when an event is raised for a <see cref="System.Json.JsonValue"/>.
    /// </summary>
    public enum JsonValueChange
    {
        /// <summary>
        /// An element has been or will be added to the collection.
        /// </summary>
        Add,

        /// <summary>
        /// An element has been or will be removed from the collection.
        /// </summary>
        Remove,

        /// <summary>
        /// An element has been or will be replaced in the collection. Used on indexers.
        /// </summary>
        Replace,

        /// <summary>
        /// All elements of the collection have been or will be removed.
        /// </summary>
        Clear,
    }
}
