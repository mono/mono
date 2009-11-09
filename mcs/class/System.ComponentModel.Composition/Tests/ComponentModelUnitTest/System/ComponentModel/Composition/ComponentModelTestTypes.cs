// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;

namespace System.ComponentModel.Composition
{
    public class CallbackExecuteCodeDuringCompose
    {
        public CallbackExecuteCodeDuringCompose(Action callback)
        {
            this.callback = callback;
        }

        [Export("MyOwnCallbackContract")]
        public string ExportValue
        {
            get
            {
                callback();
                return string.Empty;
            }
        }

        [Import("MyOwnCallbackContract")]
        public string ImportValue { get; set; }
        private Action callback;
    }

    public class CallbackImportNotify : IPartImportsSatisfiedNotification
    {
        private Action callback;
        public CallbackImportNotify(Action callback)
        {
            this.callback = callback;
        }

        [Import(AllowDefault=true)]
        public ICompositionService ImportSomethingSoIGetImportCompletedCalled { get; set; }

        public void OnImportsSatisfied()
        {
            this.callback();
        }
    }

    public class ExportValueTypeFactory
    {
        [Export("{AssemblyCatalogResolver}FactoryValueType")]
        public int Value
        {
            get
            {
                return 18;
            }
        }
    }


    public class ExportValueTypeSingleton
    {
        [Export("{AssemblyCatalogResolver}SingletonValueType")]
        public int Value
        {
            get
            {
                return 17;
            }
        }
    }


    public class Int32CollectionImporter
    {

        public Int32CollectionImporter()
        {
            Values = new Collection<int>();
        }

        [ImportMany("Value")]
        public Collection<int> Values { get; private set; }
    }

    [PartNotDiscoverable]
    public class Int32Exporter
    {

        public Int32Exporter(int value) 
        {
            Value = value;
        }

        [Export("Value")]
        public int Value { get; set; }

    }

    [PartNotDiscoverable]
    public class Int32ExporterInternal
    {

        public Int32ExporterInternal(int value) 
        {
            Value = value;
        }

        [Export("Value")]
        public int Value { get; set; }

    }

    public class Int32Importer
    {

        public Int32Importer()
        {
        }

        [Import("Value", AllowRecomposition = true)]
        public int Value { get; set; }
    }

    public class Int32ImporterInternal
    {

        public Int32ImporterInternal()
        {
        }

        [Import("Value")]
        public int Value { get; set; }
    }


}