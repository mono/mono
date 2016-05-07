//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ComponentModel;
    using Microsoft.VisualBasic.Activities;
    using System.Activities.Presentation.View;
    using System.Activities.Expressions;
    using System.Reflection;
    using System.Collections.ObjectModel;    

    class NamespaceListPropertyDescriptor : PropertyDescriptor
    {
        public const string ImportCollectionPropertyName = "Imports";
        public const string AvailableNamespacesPropertyName = "AvailableNamespaces";
        public const string NamespacePropertyName = "Namespace";        
        
        object instance;

        public NamespaceListPropertyDescriptor(object instance)
            : base(ImportCollectionPropertyName, null)
        {
            this.instance = instance;
        }

        public override Type ComponentType
        {
            get { return this.instance.GetType(); }
        }

        public override bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(NamespaceList);
            }
        }

        public override bool IsBrowsable
        {
            get
            {
                return false;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            VisualBasicSettings settings = VisualBasic.GetSettings(component);
            IList<AssemblyReference> references;
            IList<string> namespaces = NamespaceHelper.GetTextExpressionNamespaces(component, out references);
            if ((namespaces != null) && ((namespaces.Count > 0) || ((namespaces.Count == 0) && (settings == null))))
            {                                
                return new TextExpressionNamespaceList(namespaces, references);
            }
            else 
            {
                Fx.Assert(settings != null, "Either VB settings or new TextExpression attached properties should be set");
                return new VisualBasicNamespaceList(settings.ImportReferences);
            }            
        }

        public override void ResetValue(object component)
        {
            IList<AssemblyReference> references;
            IList<string> importedNamespaces = NamespaceHelper.GetTextExpressionNamespaces(component, out references);
            if (importedNamespaces != null)
            {
                NamespaceHelper.SetTextExpressionNamespaces(component, null, null);
            }
            else
            {
                NamespaceHelper.SetVisualBasicSettings(component, null);
            }
        }

        public override void SetValue(object component, object value)
        {
            NamespaceList namespaceList = value as NamespaceList;
            if (namespaceList != null)
            {
                if (namespaceList is VisualBasicNamespaceList)
                {
                    VisualBasicNamespaceList visualBasicNamespaces = namespaceList as VisualBasicNamespaceList;
                    VisualBasicSettings settings = new VisualBasicSettings();
                    settings.ImportReferences.UnionWith(visualBasicNamespaces.VisualBasicImports);
                    NamespaceHelper.SetVisualBasicSettings(component, settings);
                }
                else 
                {
                    Fx.Assert(namespaceList is TextExpressionNamespaceList, "The namespace list must be either of VisualBaiscSettings or TextExpression attached properties.");
                    TextExpressionNamespaceList textExpressionNamespace = namespaceList as TextExpressionNamespaceList;
                    NamespaceHelper.SetTextExpressionNamespaces(
                        component, 
                        textExpressionNamespace.TextExpressionNamespaces, 
                        textExpressionNamespace.TextExpressionReferences);
                }
            }
            else
            {
                this.ResetValue(component);
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        protected override void FillAttributes(IList attributeList)
        {
            attributeList.Add(new BrowsableAttribute(false));
            base.FillAttributes(attributeList);
        }
    }

    class NamespaceData
    {
        public string Namespace
        {
            get;
            set;
        }

        //Used by screen reader
        public override string ToString()
        {
            return this.Namespace;
        }
    }

    abstract class NamespaceList : IList
    {
        //list of uniqueNamespaces, the element is a tuple of the namespace and a arbitary data for consumer to use
        List<NamespaceData> uniqueNamespaces;
        Dictionary<string, List<string>> availableNamespaces;

        protected List<NamespaceData> UniqueNamespaces
        {
            get
            {
                if (this.uniqueNamespaces == null)
                {
                    this.uniqueNamespaces = new List<NamespaceData>();
                }
                return this.uniqueNamespaces;
            }
        }

        public Dictionary<string, List<string>> AvailableNamespaces
        {
            get
            {
                if (availableNamespaces == null)
                {
                    availableNamespaces = new Dictionary<string, List<string>>();
                }

                return availableNamespaces;
            }
        }

        internal int Lookup(string ns)
        {
            for (int i = 0; i < this.UniqueNamespaces.Count; i++)
            {
                if (this.UniqueNamespaces[i].Namespace == ns)
                {
                    return i;
                }
            }

            return -1;
        }

        public int Add(object value)
        {
            NamespaceData ns = value as NamespaceData;

            if (ns == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.NamespaceListArgumentMustBeNamespaceData, "value"));
            }

            if (Lookup(ns.Namespace) == -1)
            {
                this.AddCore(ns);
                return ((IList)this.UniqueNamespaces).Add(ns);
            }
            else
            {
                return -1;
            }
        }

        public void Clear()
        {
            this.ClearCore();
            this.UniqueNamespaces.Clear();
        }

        public bool Contains(object value)
        {
            return ((IList)this.UniqueNamespaces).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)this.UniqueNamespaces).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            NamespaceData ns = value as NamespaceData;
            if (ns == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.NamespaceListArgumentMustBeNamespaceData, "value"));
            }

            if (Lookup(ns.Namespace) == -1)
            {
                this.UniqueNamespaces.Insert(index, ns);
                this.InsertCore(index, ns);                
            }
            else
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException());
            }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            NamespaceData ns = value as NamespaceData;
            if (ns == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.NamespaceListArgumentMustBeNamespaceData, "value"));
            }

            int index = this.Lookup(ns.Namespace);
            if (index != -1)
            {
                RemoveAt(index);
            }
        }

        public void RemoveAt(int index)
        {
            NamespaceData ns = this.UniqueNamespaces[index];

            RemoveNamespaceFromSet(ns.Namespace);

            this.UniqueNamespaces.RemoveAt(index);
        }

        public object this[int index]
        {
            get
            {
                return this.UniqueNamespaces[index];
            }
            set
            {
                NamespaceData ns = value as NamespaceData;
                if (ns == null)
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.NamespaceListArgumentMustBeNamespaceData, "value"));
                }

                if (Lookup(ns.Namespace) == -1)
                {
                    this.SetValueAt(index, ns);
                    this.UniqueNamespaces[index] = ns;
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NamespaceListNoDuplicate));
                }
            }
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)this.UniqueNamespaces).CopyTo(array, index);
        }

        public int Count
        {
            get { return this.UniqueNamespaces.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return null; }
        }

        public IEnumerator GetEnumerator()
        {
            return this.UniqueNamespaces.GetEnumerator();
        }

        public void UpdateAssemblyInfo(string importedNamespace)
        {
            this.UpdateAssemblyInfoCore(importedNamespace);
        }

        protected abstract void AddCore(NamespaceData ns);
        protected abstract void ClearCore();
        protected abstract void InsertCore(int index, NamespaceData ns);
        protected abstract void RemoveNamespaceFromSet(string ns);
        protected abstract void SetValueAt(int index, NamespaceData ns);
        protected abstract void UpdateAssemblyInfoCore(string importedNamespace);
    }

    class VisualBasicNamespaceList : NamespaceList
    {
        //Since XAML generated by designer will almost always have some designer type in it, designer namespaces will appear in XAML.
        //And because Runtime design could not destinguish namespaces coming from XAML serialization between namespaces added by users,
        //designer namespaces will show up in import designer. This is bad user experience because most WF project won't reference designer
        //assembly. We could safely assume that customers won't use designer namespaces and type in their WF expressions, so we simply
        //---- designer namespaces out of the namespace list
        static readonly string[] BlackListedAssemblies = new string[] 
        {
            typeof(ViewStateService).Assembly.GetName().FullName,
            typeof(ViewStateService).Assembly.GetName().Name,
        };

        static readonly Collection<string> FrameworkAssemblies = new Collection<string>
        {
            typeof(Activity).Assembly.GetName().FullName,
            typeof(Activity).Assembly.GetName().Name,            
        };

        static readonly Collection<string> BlackListsedNamespaces = new Collection<string>
        {
            "System.Activities.Composition",
            "System.Activities.Debugger.Symbol"
        };

        public VisualBasicNamespaceList(ISet<VisualBasicImportReference> importReferences)
        {
            this.VisualBasicImports = importReferences;

            foreach (string blackListedAssembly in BlackListedAssemblies)
            {
                RemoveAssemblyFromSet(blackListedAssembly);
            }
            foreach (VisualBasicImportReference import in importReferences)
            {
                if (!(BlackListsedNamespaces.Contains(import.Import) && FrameworkAssemblies.Contains(import.Assembly)))
                {
                    if (Lookup(import.Import) == -1)
                    {
                        this.UniqueNamespaces.Add(new NamespaceData { Namespace = import.Import });
                    }
                }
            }
        }

        internal ISet<VisualBasicImportReference> VisualBasicImports
        {
            get;
            private set;
        }

        IEnumerable<VisualBasicImportReference> GetVisualBasicImportReferences(string importNamespace)
        {
            List<VisualBasicImportReference> imports = new List<VisualBasicImportReference>();
            List<string> assemblies;
            //in rehost cases or when some assembiles are not referenced, we may not find that namespace
            if (!this.AvailableNamespaces.TryGetValue(importNamespace, out assemblies))
            {
                return imports;
            }

            foreach (string assembly in assemblies)
            {
                imports.Add(new VisualBasicImportReference
                {
                    Import = importNamespace,
                    Assembly = assembly
                });
            }
            return imports;
        }

        protected override void UpdateAssemblyInfoCore(string importedNamespace)
        {
            if (this.VisualBasicImports != null)
            {
                if (this.Lookup(importedNamespace) != -1)
                {
                    this.VisualBasicImports.UnionWith(GetVisualBasicImportReferences(importedNamespace));
                }
                else
                {
                    Fx.Assert("UpdateAssemblyInfor should only be called for existed namespace");
                }
            }
        }

        protected override void RemoveNamespaceFromSet(string ns)
        {
            List<VisualBasicImportReference> toRemoves = new List<VisualBasicImportReference>();
            foreach (VisualBasicImportReference import in this.VisualBasicImports)
            {
                if (import.Import == ns)
                {
                    toRemoves.Add(import);
                }
            }

            foreach (VisualBasicImportReference toRemove in toRemoves)
            {
                this.VisualBasicImports.Remove(toRemove);
            }
        }

        private void RemoveAssemblyFromSet(string assembly)
        {
            List<VisualBasicImportReference> toRemoves = new List<VisualBasicImportReference>();
            foreach (VisualBasicImportReference import in this.VisualBasicImports)
            {
                if (import.Assembly == assembly)
                {
                    toRemoves.Add(import);
                }
            }

            foreach (VisualBasicImportReference toRemove in toRemoves)
            {
                this.VisualBasicImports.Remove(toRemove);
            }
        }

        protected override void AddCore(NamespaceData ns)
        {
            this.VisualBasicImports.UnionWith(GetVisualBasicImportReferences(ns.Namespace));
        }

        protected override void ClearCore()
        {
            this.VisualBasicImports.Clear();
        }

        protected override void InsertCore(int index, NamespaceData ns)
        {
            this.VisualBasicImports.UnionWith(GetVisualBasicImportReferences(ns.Namespace));
        }

        protected override void SetValueAt(int index, NamespaceData ns)
        {
            RemoveNamespaceFromSet(this.UniqueNamespaces[index].Namespace);
            this.VisualBasicImports.UnionWith(GetVisualBasicImportReferences(ns.Namespace));
        }
    }

    class TextExpressionNamespaceList : NamespaceList
    {
        public TextExpressionNamespaceList(IList<string> importedNamespaces, IList<AssemblyReference> references)
        {
            this.TextExpressionNamespaces = importedNamespaces;
            this.TextExpressionReferences = references;
            foreach (string importedNamespace in importedNamespaces)
            {
                if (Lookup(importedNamespace) == -1)
                {
                    this.UniqueNamespaces.Add(new NamespaceData { Namespace = importedNamespace });
                }
            }
        }

        internal IList<string> TextExpressionNamespaces
        {
            get;
            private set;
        }

        internal IList<AssemblyReference> TextExpressionReferences
        {
            get;
            private set;
        }


        protected override void RemoveNamespaceFromSet(string ns)
        {
            this.TextExpressionNamespaces.Remove(ns);
        }

        internal void RemoveAssemblyFromSet(string assembly)
        {
            AssemblyReference toRemove = null;
            foreach (AssemblyReference reference in this.TextExpressionReferences)
            {
                if (reference.AssemblyName.Name == assembly)
                {
                    toRemove = reference;
                    break;
                }
            }
            if (toRemove != null)
            {
                this.TextExpressionReferences.Remove(toRemove);
            }
        }

        private void AddAssemblyToSet(string assembly)
        {
            bool isExisted = false;
            foreach (AssemblyReference reference in this.TextExpressionReferences)
            {
                if (reference.AssemblyName.Name == assembly)
                {
                    isExisted = true;
                }
            }

            if (!isExisted)
            {
                this.TextExpressionReferences.Add(new AssemblyReference { AssemblyName = new AssemblyName(assembly) });
            }
        }

        protected override void AddCore(NamespaceData ns)
        {
            this.InsertCore(this.TextExpressionNamespaces.Count, ns);
        }

        protected override void ClearCore()
        {
            this.TextExpressionNamespaces.Clear();
        }

        protected override void InsertCore(int index, NamespaceData ns)
        {
            this.TextExpressionNamespaces.Insert(index, ns.Namespace);

            if (this.AvailableNamespaces.ContainsKey(ns.Namespace))
            {                
                foreach (string assembly in this.AvailableNamespaces[ns.Namespace])
                {
                    this.AddAssemblyToSet(assembly);
                }
            }
        }

        protected override void SetValueAt(int index, NamespaceData ns)
        {
            this.TextExpressionNamespaces[index] = ns.Namespace;
        }

        protected override void UpdateAssemblyInfoCore(string importedNamespace)
        {            
            foreach (string assembly in this.AvailableNamespaces[importedNamespace])
            {
                this.AddAssemblyToSet(assembly);
            }
        }

    }
}
