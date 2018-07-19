//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.DurableInstancing;
    using System.Threading;
    using System.Xml.Linq;

    class PersistencePipeline
    {
        readonly IEnumerable<IPersistencePipelineModule> modules;

        Stage expectedStage;

        IDictionary<XName, InstanceValue> values;
        ReadOnlyDictionaryInternal<XName, InstanceValue> readOnlyView;
        ValueDictionaryView readWriteView;
        ValueDictionaryView writeOnlyView;

        // Used for the save pipeline.
        public PersistencePipeline(IEnumerable<IPersistencePipelineModule> modules, Dictionary<XName, InstanceValue> initialValues)
        {
            Fx.Assert(modules != null, "Null modules collection provided to persistence pipeline.");

            this.expectedStage = Stage.Collect;
            this.modules = modules;
            this.values = initialValues;
            this.readOnlyView = new ReadOnlyDictionaryInternal<XName, InstanceValue>(this.values);
            this.readWriteView = new ValueDictionaryView(this.values, false);
            this.writeOnlyView = new ValueDictionaryView(this.values, true);
        }

        // Used for the load pipeline.
        public PersistencePipeline(IEnumerable<IPersistencePipelineModule> modules)
        {
            Fx.Assert(modules != null, "Null modules collection provided to persistence pipeline.");

            this.expectedStage = Stage.Load;
            this.modules = modules;
        }

        public ReadOnlyDictionaryInternal<XName, InstanceValue> Values
        {
            get
            {
                return this.readOnlyView;
            }
        }

        public bool IsSaveTransactionRequired
        {
            get
            {
                return this.modules.FirstOrDefault(value => value.IsSaveTransactionRequired) != null;
            }
        }

        public bool IsLoadTransactionRequired
        {
            get
            {
                return this.modules.FirstOrDefault(value => value.IsLoadTransactionRequired) != null;
            }
        }

        public void Collect()
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Collect, "Collect called at the wrong time.");
            this.expectedStage = Stage.None;

            foreach (IPersistencePipelineModule module in modules)
            {
                IDictionary<XName, object> readWriteValues;
                IDictionary<XName, object> writeOnlyValues;

                module.CollectValues(out readWriteValues, out writeOnlyValues);
                if (readWriteValues != null)
                {
                    foreach (KeyValuePair<XName, object> value in readWriteValues)
                    {
                        try
                        {
                            this.values.Add(value.Key, new InstanceValue(value.Value));
                        }
                        catch (ArgumentException exception)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.NameCollisionOnCollect(value.Key, module.GetType().Name), exception));
                        }
                    }
                }
                if (writeOnlyValues != null)
                {
                    foreach (KeyValuePair<XName, object> value in writeOnlyValues)
                    {
                        try
                        {
                            this.values.Add(value.Key, new InstanceValue(value.Value, InstanceValueOptions.Optional | InstanceValueOptions.WriteOnly));
                        }
                        catch (ArgumentException exception)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.NameCollisionOnCollect(value.Key, module.GetType().Name), exception));
                        }
                    }
                }
            }

            this.expectedStage = Stage.Map;
        }

        public void Map()
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Map, "Map called at the wrong time.");
            this.expectedStage = Stage.None;

            List<Tuple<IPersistencePipelineModule, IDictionary<XName, object>>> pendingValues = null;

            foreach (IPersistencePipelineModule module in modules)
            {
                IDictionary<XName, object> mappedValues = module.MapValues(this.readWriteView, this.writeOnlyView);
                if (mappedValues != null)
                {
                    if (pendingValues == null)
                    {
                        pendingValues = new List<Tuple<IPersistencePipelineModule, IDictionary<XName, object>>>();
                    }
                    pendingValues.Add(new Tuple<IPersistencePipelineModule, IDictionary<XName, object>>(module, mappedValues));
                }
            }

            if (pendingValues != null)
            {
                foreach (Tuple<IPersistencePipelineModule, IDictionary<XName, object>> writeOnlyValues in pendingValues)
                {
                    foreach (KeyValuePair<XName, object> value in writeOnlyValues.Item2)
                    {
                        try
                        {
                            this.values.Add(value.Key, new InstanceValue(value.Value, InstanceValueOptions.Optional | InstanceValueOptions.WriteOnly));
                        }
                        catch (ArgumentException exception)
                        {
                            throw Fx.Exception.AsError(new InvalidOperationException(SRCore.NameCollisionOnMap(value.Key, writeOnlyValues.Item1.GetType().Name), exception));
                        }
                    }
                }

                this.writeOnlyView.ResetCaches();
            }

            this.expectedStage = Stage.Save;
        }

        public IAsyncResult BeginSave(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Save, "Save called at the wrong time.");
            this.expectedStage = Stage.None;

            return new IOAsyncResult(this, false, timeout, callback, state);
        }

        public void EndSave(IAsyncResult result)
        {
            IOAsyncResult.End(result);
        }

        public void SetLoadedValues(IDictionary<XName, InstanceValue> values)
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Load, "SetLoadedValues called at the wrong time.");
            Fx.Assert(values != null, "Null values collection provided to SetLoadedValues.");

            this.values = values;
            this.readOnlyView = values as ReadOnlyDictionaryInternal<XName, InstanceValue> ?? new ReadOnlyDictionaryInternal<XName, InstanceValue>(values);
            this.readWriteView = new ValueDictionaryView(this.values, false);
        }

        public IAsyncResult BeginLoad(TimeSpan timeout, AsyncCallback callback, object state)
        {
            Fx.Assert(this.values != null, "SetLoadedValues not called.");
            Fx.AssertAndThrow(this.expectedStage == Stage.Load, "Load called at the wrong time.");
            this.expectedStage = Stage.None;

            return new IOAsyncResult(this, true, timeout, callback, state);
        }

        public void EndLoad(IAsyncResult result)
        {
            IOAsyncResult.End(result);
            this.expectedStage = Stage.Publish;
        }

        public void Publish()
        {
            Fx.AssertAndThrow(this.expectedStage == Stage.Publish || this.expectedStage == Stage.Load, "Publish called at the wrong time.");
            this.expectedStage = Stage.None;

            foreach (IPersistencePipelineModule module in modules)
            {
                module.PublishValues(this.readWriteView);
            }
        }

        public void Abort()
        {
            foreach (IPersistencePipelineModule module in modules)
            {
                try
                {
                    module.Abort();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    throw Fx.Exception.AsError(new CallbackException(SRCore.PersistencePipelineAbortThrew(module.GetType().Name), exception));
                }
            }
        }

        enum Stage
        {
            None,
            Collect,
            Map,
            Save,
            Load,
            Publish,
        }

        class ValueDictionaryView : IDictionary<XName, object>
        {
            IDictionary<XName, InstanceValue> basis;
            bool writeOnly;

            List<XName> keys;
            List<object> values;

            public ValueDictionaryView(IDictionary<XName, InstanceValue> basis, bool writeOnly)
            {
                this.basis = basis;
                this.writeOnly = writeOnly;
            }

            public ICollection<XName> Keys
            {
                get
                {
                    if (this.keys == null)
                    {
                        this.keys = new List<XName>(this.basis.Where(value => value.Value.IsWriteOnly() == this.writeOnly).Select(value => value.Key));
                    }
                    return this.keys;
                }
            }

            public ICollection<object> Values
            {
                get
                {
                    if (this.values == null)
                    {
                        this.values = new List<object>(this.basis.Where(value => value.Value.IsWriteOnly() == this.writeOnly).Select(value => value.Value.Value));
                    }
                    return this.values;
                }
            }

            public object this[XName key]
            {
                get
                {
                    object value;
                    if (TryGetValue(key, out value))
                    {
                        return value;
                    }
                    throw Fx.Exception.AsError(new KeyNotFoundException());
                }

                set
                {
                    throw Fx.Exception.AsError(CreateReadOnlyException());
                }
            }

            public int Count
            {
                get
                {
                    return Keys.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public void Add(XName key, object value)
            {
                throw Fx.Exception.AsError(CreateReadOnlyException());
            }

            public bool ContainsKey(XName key)
            {
                object dummy;
                return TryGetValue(key, out dummy);
            }

            public bool Remove(XName key)
            {
                throw Fx.Exception.AsError(CreateReadOnlyException());
            }

            public bool TryGetValue(XName key, out object value)
            {
                InstanceValue realValue;
                if (!this.basis.TryGetValue(key, out realValue) || realValue.IsWriteOnly() != this.writeOnly)
                {
                    value = null;
                    return false;
                }

                value = realValue.Value;
                return true;
            }

            public void Add(KeyValuePair<XName, object> item)
            {
                throw Fx.Exception.AsError(CreateReadOnlyException());
            }

            public void Clear()
            {
                throw Fx.Exception.AsError(CreateReadOnlyException());
            }

            public bool Contains(KeyValuePair<XName, object> item)
            {
                object value;
                if (!TryGetValue(item.Key, out value))
                {
                    return false;
                }
                return EqualityComparer<object>.Default.Equals(value, item.Value);
            }

            public void CopyTo(KeyValuePair<XName, object>[] array, int arrayIndex)
            {
                foreach (KeyValuePair<XName, object> entry in this)
                {
                    array[arrayIndex++] = entry;
                }
            }

            public bool Remove(KeyValuePair<XName, object> item)
            {
                throw Fx.Exception.AsError(CreateReadOnlyException());
            }

            public IEnumerator<KeyValuePair<XName, object>> GetEnumerator()
            {
                return this.basis.Where(value => value.Value.IsWriteOnly() == this.writeOnly).Select(value => new KeyValuePair<XName, object>(value.Key, value.Value.Value)).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal void ResetCaches()
            {
                this.keys = null;
                this.values = null;
            }

            Exception CreateReadOnlyException()
            {
                return new InvalidOperationException(InternalSR.DictionaryIsReadOnly);
            }
        }

        class IOAsyncResult : AsyncResult
        {
            PersistencePipeline pipeline;
            bool isLoad;
            IPersistencePipelineModule[] pendingModules;
            int remainingModules;
            Exception exception;

            public IOAsyncResult(PersistencePipeline pipeline, bool isLoad, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                this.pipeline = pipeline;
                this.isLoad = isLoad;
                this.pendingModules = this.pipeline.modules.Where(value => value.IsIOParticipant).ToArray();
                this.remainingModules = this.pendingModules.Length;

                bool completeSelf = false;
                if (this.pendingModules.Length == 0)
                {
                    completeSelf = true;
                }
                else
                {
                    for (int i = 0; i < this.pendingModules.Length; i++)
                    {
                        Fx.Assert(!completeSelf, "Shouldn't have been completed yet.");

                        IPersistencePipelineModule module = this.pendingModules[i];
                        IAsyncResult result = null;
                        try
                        {
                            if (this.isLoad)
                            {
                                result = module.BeginOnLoad(this.pipeline.readWriteView, timeout, Fx.ThunkCallback(new AsyncCallback(OnIOComplete)), i);
                            }
                            else
                            {
                                result = module.BeginOnSave(this.pipeline.readWriteView, this.pipeline.writeOnlyView, timeout, Fx.ThunkCallback(new AsyncCallback(OnIOComplete)), i);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }

                            this.pendingModules[i] = null;
                            ProcessException(exception);
                        }
                        if (result == null)
                        {
                            if (CompleteOne())
                            {
                                completeSelf = true;
                            }
                        }
                        else if (result.CompletedSynchronously)
                        {
                            this.pendingModules[i] = null;
                            if (IOComplete(result, module))
                            {
                                completeSelf = true;
                            }
                        }
                    }
                }

                if (completeSelf)
                {
                    Complete(true, this.exception);
                }
            }

            void OnIOComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                int i = (int)result.AsyncState;

                IPersistencePipelineModule module = this.pendingModules[i];
                Fx.Assert(module != null, "There should be a pending result for this result");
                this.pendingModules[i] = null;

                if (IOComplete(result, module))
                {
                    Complete(false, this.exception);
                }
            }

            bool IOComplete(IAsyncResult result, IPersistencePipelineModule module)
            {
                try
                {
                    if (this.isLoad)
                    {
                        module.EndOnLoad(result);
                    }
                    else
                    {
                        module.EndOnSave(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    ProcessException(exception);
                }
                return CompleteOne();
            }

            void ProcessException(Exception exception)
            {
                if (exception != null)
                {
                    bool abortNeeded = false;
                    lock (this.pendingModules)
                    {
                        if (this.exception == null)
                        {
                            this.exception = exception;
                            abortNeeded = true;
                        }
                    }

                    if (abortNeeded)
                    {
                        Abort();
                    }
                }
            }

            bool CompleteOne()
            {
                return Interlocked.Decrement(ref this.remainingModules) == 0;
            }

            void Abort()
            {
                for (int j = 0; j < this.pendingModules.Length; j++)
                {
                    IPersistencePipelineModule module = this.pendingModules[j];
                    if (module != null)
                    {
                        try
                        {
                            module.Abort();
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            throw Fx.Exception.AsError(new CallbackException(SRCore.PersistencePipelineAbortThrew(module.GetType().Name), exception));
                        }
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<IOAsyncResult>(result);
            }
        }
    }
}
