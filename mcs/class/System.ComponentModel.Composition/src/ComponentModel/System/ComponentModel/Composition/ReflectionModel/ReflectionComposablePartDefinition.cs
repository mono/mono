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
    }
}
