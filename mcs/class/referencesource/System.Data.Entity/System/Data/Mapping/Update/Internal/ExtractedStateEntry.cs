//---------------------------------------------------------------------
// <copyright file="ExtractedStateEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
//---------------------------------------------------------------------


using System.Collections.Generic;
using System.Data.Common.CommandTrees;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
namespace System.Data.Mapping.Update.Internal
{
    /// <summary>
    /// Represents the data contained in a StateEntry using internal data structures
    /// of the UpdatePipeline.
    /// </summary>
    internal struct ExtractedStateEntry
    {
        internal readonly EntityState State;
        internal readonly PropagatorResult Original;
        internal readonly PropagatorResult Current;
        internal readonly IEntityStateEntry Source;

        internal ExtractedStateEntry(UpdateTranslator translator, IEntityStateEntry stateEntry)
        {
            Debug.Assert(null != stateEntry, "stateEntry must not be null");
            this.State = stateEntry.State;
            this.Source = stateEntry;

            switch (stateEntry.State)
            {
                case EntityState.Deleted:
                    this.Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.AllModified);
                    this.Current = null;
                    break;
                case EntityState.Unchanged:
                    this.Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.NoneModified);
                    this.Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.NoneModified);
                    break;
                case EntityState.Modified:
                    this.Original = translator.RecordConverter.ConvertOriginalValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.SomeModified);
                    this.Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.SomeModified);
                    break;
                case EntityState.Added:
                    this.Original = null;
                    this.Current = translator.RecordConverter.ConvertCurrentValuesToPropagatorResult(
                        stateEntry, ModifiedPropertiesBehavior.AllModified);
                    break;
                default:
                    Debug.Fail("unexpected IEntityStateEntry.State for entity " + stateEntry.State);
                    this.Original = null;
                    this.Current = null;
                    break;
            }
        }
    }
}
