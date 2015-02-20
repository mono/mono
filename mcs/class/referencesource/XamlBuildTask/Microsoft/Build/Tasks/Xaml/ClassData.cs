//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Linq;
    using System.Xaml.Schema;
    using System.Xaml;
    using System.Windows.Markup;
    using System.Runtime;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using System.Collections;
    
    public class ClassData
    {
        [Fx.Tag.Queue(typeof(NamedObject), Scope = Fx.Tag.Strings.DeclaringInstance)]
        private List<NamedObject> namedObjects;
        [Fx.Tag.Queue(typeof(string), Scope = Fx.Tag.Strings.DeclaringInstance)]
        private List<string> codeSnippets;
        List<AttributeData> attributes;
        PropertyDataCollection properties;

        public ClassData()
        {
            this.IsPublic = true;
        }

        public XamlType BaseType
        {
            get;
            set;
        }

        public IList<String> CodeSnippets
        {
            get
            {
                if (codeSnippets == null)
                {
                    codeSnippets = new List<String>();
                }
                return codeSnippets;
            }
        }

        public string EmbeddedResourceFileName
        { 
            get; 
            internal set; 
        }

        public KeyedCollection<string, PropertyData> Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new PropertyDataCollection();
                }
                return this.properties;
            }
        }

        public IList<AttributeData> Attributes
        {
            get
            {                
                if (attributes == null)
                {
                    attributes = new List<AttributeData>();
                }
                return attributes;
            }
        }

        public string Name
        { 
            get;
            internal set; 
        }
        
        public IList<NamedObject> NamedObjects
        {
            get
            {
                if (namedObjects == null)
                {
                    namedObjects = new List<NamedObject>();
                }
                return namedObjects;
            }
        }

        public String Namespace
        { 
            get;
            internal set;
        }             

        public XamlNodeList EmbeddedResourceXaml
        {
            get;
            set;
        }

        public bool IsPublic
        {
            get;
            set;
        }       
        
        public string FileName
        {
            get; 
            internal set;
        }

        public string HelperClassFullName
        {
            get;
            internal set;
        }

        internal String RootNamespace
        {
            get;
            set;
        }  

        internal bool SourceFileExists
        {
            get;
            set;
        }

        internal bool RequiresCompilationPass2
        {
            get;
            set;
        }
        
        class PropertyDataCollection : KeyedCollection<string, PropertyData>
        {
            protected override string GetKeyForItem(PropertyData item)
            {
                return item.Name;
            }
        }
    }
}
