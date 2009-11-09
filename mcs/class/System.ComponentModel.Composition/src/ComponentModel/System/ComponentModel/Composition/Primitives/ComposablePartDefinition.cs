// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace System.ComponentModel.Composition.Primitives
{
    /// <summary>
    ///     Defines the <see langword="abstract"/> base class for composable part definitions, which 
    ///     describe, and allow the creation of, <see cref="ComposablePart"/> objects.
    /// </summary>
    public abstract class ComposablePartDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ComposablePartDefinition"/> class.
        /// </summary>
        protected ComposablePartDefinition()
        {
        }

        /// <summary>
        ///     Gets the export definitions that describe the exported values provided by parts 
        ///     created by the definition.
        /// </summary>
        /// <value>
        ///     An <see cref="IEnumerable{T}"/> of <see cref="ExportDefinition"/> objects describing
        ///     the exported values provided by <see cref="ComposablePart"/> objects created by the 
        ///     <see cref="ComposablePartDefinition"/>.
        /// </value>
        /// <remarks>
         ///     <note type="inheritinfo">
        ///         Overrides of this property should never return <see langword="null"/>.
        ///         If the <see cref="ComposablePart"/> objects created by the 
        ///         <see cref="ComposablePartDefinition"/> do not provide exported values, return 
        ///         an empty <see cref="IEnumerable{T}"/> instead.
        ///     </note>
        /// </remarks>
        public abstract IEnumerable<ExportDefinition> ExportDefinitions { get; }

        /// <summary>
        ///     Gets the import definitions that describe the imports required by parts created 
        ///     by the definition.
        /// </summary>
        /// <value>
        ///     An <see cref="IEnumerable{T}"/> of <see cref="ImportDefinition"/> objects describing
        ///     the imports required by <see cref="ComposablePart"/> objects created by the 
        ///     <see cref="ComposablePartDefinition"/>.
        /// </value>
        /// <remarks>
        ///     <note type="inheritinfo">
        ///         Overriders of this property should never return <see langword="null"/>.
        ///         If the <see cref="ComposablePart"/> objects created by the 
        ///         <see cref="ComposablePartDefinition"/> do not have imports, return an empty 
        ///         <see cref="IEnumerable{T}"/> instead.
        ///     </note>
        /// </remarks>
        public abstract IEnumerable<ImportDefinition> ImportDefinitions { get; }

        /// <summary>
        ///     Gets the metadata of the definition.
        /// </summary>
        /// <value>
        ///     An <see cref="IDictionary{TKey, TValue}"/> containing the metadata of the 
        ///     <see cref="ComposablePartDefinition"/>. The default is an empty, read-only
        ///     <see cref="IDictionary{TKey, TValue}"/>.
        /// </value>
        /// <remarks>
        ///     <para>
        ///         <note type="inheritinfo">
        ///             Overriders of this property should return a read-only
        ///             <see cref="IDictionary{TKey, TValue}"/> object with a case-sensitive, 
        ///             non-linguistic comparer, such as <see cref="StringComparer.Ordinal"/>, 
        ///             and should never return <see langword="null"/>. If the 
        ///             <see cref="ComposablePartDefinition"/> does contain metadata, 
        ///             return an empty <see cref="IDictionary{TKey, TValue}"/> instead.
        ///         </note>
        ///     </para>
        /// </remarks>
        public virtual IDictionary<string, object> Metadata 
        {
            get { return MetadataServices.EmptyMetadata; } 
        }

        /// <summary>
        ///     Creates a new instance of a part that the definition describes.
        /// </summary>
        /// <returns>
        ///     The created <see cref="ComposablePart"/>.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         <note type="inheritinfo">
        ///             Derived types overriding this method should return a new instance of a 
        ///             <see cref="ComposablePart"/> on every invoke and should never return 
        ///             <see langword="null"/>.
        ///         </note>
        ///     </para>
        /// </remarks>
        public abstract ComposablePart CreatePart();
    }
}
