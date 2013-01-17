// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Internal;
using Microsoft.Internal.Collections;
using System.ComponentModel.Composition.Hosting;

namespace System.ComponentModel.Composition.ReflectionModel
{
    internal class ReflectionComposablePartDefinition : ComposablePartDefinition, ICompositionElement
    {
        private readonly IReflectionPartCreationInfo _creationInfo;

        private volatile IEnumerable<ImportDefinition> _imports;
        private volatile IEnumerable<ExportDefinition> _exports;
        private volatile IDictionary<string, object> _metadata;
        private volatile ConstructorInfo _constructor;
        private object _lock = new object();

        public ReflectionComposablePartDefinition(IReflectionPartCreationInfo creationInfo)
        {
            Assumes.NotNull(creationInfo);
            this._creationInfo = creationInfo;
        }

        public Type GetPartType()
        {
            return this._creationInfo.GetPartType();
        }

        public Lazy<Type> GetLazyPartType()
        {
            return this._creationInfo.GetLazyPartType();
        }

        public ConstructorInfo GetConstructor()
        {
            if (this._constructor == null)
            {
                ConstructorInfo constructor = this._creationInfo.GetConstructor();
                lock (this._lock)
                {
                    if (this._constructor == null)
                    {
                        this._constructor = constructor;
                    }
                }
            }

            return this._constructor;
        }

        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get
            {
                if (this._exports == null)
                {
                    ExportDefinition[] exports = this._creationInfo.GetExports().ToArray();
                    lock (this._lock)
                    {
                        if (this._exports == null)
                        {
                            this._exports = exports;
                        }
                    }
                }
                return this._exports;
            }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get
            {
                if (this._imports == null)
                {
                    ImportDefinition[] imports = this._creationInfo.GetImports().ToArray();
                    lock (this._lock)
                    {
                        if (this._imports == null)
                        {
                            this._imports = imports;
                        }
                    }
                }
                return this._imports;
            }
        }

        public override IDictionary<string, object> Metadata
        {
            get
            {
                if (this._metadata == null)
                {
                    IDictionary<string, object> metadata = this._creationInfo.GetMetadata().AsReadOnly();
                    lock (this._lock)
                    {
                        if (this._metadata == null)
                        {
                            this._metadata = metadata;
                        }
                    }
                }
                return this._metadata;
            }
        }

        internal bool IsDisposalRequired
        {
            get
            {
                return this._creationInfo.IsDisposalRequired;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public override ComposablePart CreatePart()
        {
            if (this.IsDisposalRequired)
            {
                return new DisposableReflectionComposablePart(this);
            }
            else
            {
                return new ReflectionComposablePart(this);
            }
        }

        internal override ComposablePartDefinition GetGenericPartDefinition()
        {
            GenericSpecializationPartCreationInfo genericCreationInfo = this._creationInfo as GenericSpecializationPartCreationInfo;
            if (genericCreationInfo != null)
            {
                return genericCreationInfo.OriginalPart;
            }

            return null;
        }

        internal override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            if (this.IsGeneric())
            {
                List<Tuple<ComposablePartDefinition, ExportDefinition>> exports = null;

                var genericParameters = (definition.Metadata.Count > 0) ? definition.Metadata.GetValue<IEnumerable<object>>(CompositionConstants.GenericParametersMetadataName) : null;
                // if and only if generic parameters have been supplied can we attempt to "close" the generic
                if (genericParameters != null)
                {
                    Type[] genericTypeParameters = null;
                    // we only undestand types
                    if (TryGetGenericTypeParameters(genericParameters, out genericTypeParameters))
                    {
                        // go through all orders of generic parameters that part exports allows
                        foreach (Type[] candidateParameters in this.GetCandidateParameters(genericTypeParameters))
                        {
                            ComposablePartDefinition candidatePart = null;
                            if (this.TryMakeGenericPartDefinition(candidateParameters, out candidatePart))
                            {
                                var candidatePartExports = candidatePart.GetExports(definition);
                                if (candidatePartExports != ComposablePartDefinition._EmptyExports)
                                {
                                    exports = exports.FastAppendToListAllowNulls(candidatePartExports);
                                }
                            }
                        }
                    }
                }
                return exports ?? _EmptyExports;
            }
            else
            {
                return base.GetExports(definition);
            }
        }

        private IEnumerable<Type[]> GetCandidateParameters(Type[] genericParameters)
        {
            // we iterate over all exports and find only generic ones. Assuming the arity matches, we reorder the original parameters
            foreach (ExportDefinition export in this.ExportDefinitions)
            {
                var genericParametersOrder = export.Metadata.GetValue<int[]>(CompositionConstants.GenericExportParametersOrderMetadataName);
                if ((genericParametersOrder != null) && (genericParametersOrder.Length == genericParameters.Length))
                {
                    yield return GenericServices.Reorder(genericParameters, genericParametersOrder);
                }
            }
        }

        private static bool TryGetGenericTypeParameters(IEnumerable<object> genericParameters, out Type[] genericTypeParameters)
        {
            genericTypeParameters = genericParameters as Type[];
            if (genericTypeParameters == null)
            {
                object[] genericParametersAsArray = genericParameters.AsArray();
                genericTypeParameters = new Type[genericParametersAsArray.Length];
                for (int i = 0; i < genericParametersAsArray.Length; i++)
                {
                    genericTypeParameters[i] = genericParametersAsArray[i] as Type;
                    if (genericTypeParameters[i] == null)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool TryMakeGenericPartDefinition(Type[] genericTypeParameters, out ComposablePartDefinition genericPartDefinition)
        {
            genericPartDefinition = null;

            if (!GenericSpecializationPartCreationInfo.CanSpecialize(this.Metadata, genericTypeParameters))
            {
                return false;
            }

            genericPartDefinition = new ReflectionComposablePartDefinition(new GenericSpecializationPartCreationInfo(this._creationInfo, this, genericTypeParameters));
            return true;
        }

        string ICompositionElement.DisplayName
        {
            get { return this._creationInfo.DisplayName; }
        }

        ICompositionElement ICompositionElement.Origin
        {
            get { return this._creationInfo.Origin; }
        }

        public override string ToString()
        {
            return this._creationInfo.DisplayName;
        }

        public override bool Equals(object obj)
        {
            ReflectionComposablePartDefinition that = obj as ReflectionComposablePartDefinition;
            if (that == null)
            {
                return false;
            }

            return this._creationInfo.Equals(that._creationInfo);
        }

        public override int GetHashCode()
        {
            return this._creationInfo.GetHashCode();
        }
    }
}
