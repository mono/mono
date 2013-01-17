namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc.Resources;
    using System.Xml;

    // Processes files with this format:
    //
    // <typeCache lastModified=... mvcVersionId=...>
    //   <assembly name=...>
    //     <module versionId=...>
    //       <type>...</type>
    //     </module>
    //   </assembly>
    // </typeCache>
    //
    // This is used to store caches of files between AppDomain resets, leading to improved cold boot time
    // and more efficient use of memory.

    internal sealed class TypeCacheSerializer {

        private static readonly Guid _mvcVersionId = typeof(TypeCacheSerializer).Module.ModuleVersionId;

        // used for unit testing

        private DateTime CurrentDate {
            get {
                return CurrentDateOverride ?? DateTime.Now;
            }
        }

        internal DateTime? CurrentDateOverride {
            get;
            set;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This is an instance method for consistency with the SerializeTypes() method.")]
        public List<Type> DeserializeTypes(TextReader input) {
            XmlDocument doc = new XmlDocument();
            doc.Load(input);
            XmlElement rootElement = doc.DocumentElement;

            Guid readMvcVersionId = new Guid(rootElement.Attributes["mvcVersionId"].Value);
            if (readMvcVersionId != _mvcVersionId) {
                // The cache is outdated because the cache file was produced by a different version
                // of MVC.
                return null;
            }

            List<Type> deserializedTypes = new List<Type>();
            foreach (XmlNode assemblyNode in rootElement.ChildNodes) {
                string assemblyName = assemblyNode.Attributes["name"].Value;
                Assembly assembly = Assembly.Load(assemblyName);

                foreach (XmlNode moduleNode in assemblyNode.ChildNodes) {
                    Guid moduleVersionId = new Guid(moduleNode.Attributes["versionId"].Value);

                    foreach (XmlNode typeNode in moduleNode.ChildNodes) {
                        string typeName = typeNode.InnerText;
                        Type type = assembly.GetType(typeName);
                        if (type == null || type.Module.ModuleVersionId != moduleVersionId) {
                            // The cache is outdated because we couldn't find a previously recorded
                            // type or the type's containing module was modified.
                            return null;
                        }
                        else {
                            deserializedTypes.Add(type);
                        }
                    }
                }
            }

            return deserializedTypes;
        }

        public void SerializeTypes(IEnumerable<Type> types, TextWriter output) {
            var groupedByAssembly = from type in types
                                    group type by type.Module into groupedByModule
                                    group groupedByModule by groupedByModule.Key.Assembly;

            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateComment(MvcResources.TypeCache_DoNotModify));

            XmlElement typeCacheElement = doc.CreateElement("typeCache");
            doc.AppendChild(typeCacheElement);
            typeCacheElement.SetAttribute("lastModified", CurrentDate.ToString());
            typeCacheElement.SetAttribute("mvcVersionId", _mvcVersionId.ToString());

            foreach (var assemblyGroup in groupedByAssembly) {
                XmlElement assemblyElement = doc.CreateElement("assembly");
                typeCacheElement.AppendChild(assemblyElement);
                assemblyElement.SetAttribute("name", assemblyGroup.Key.FullName);

                foreach (var moduleGroup in assemblyGroup) {
                    XmlElement moduleElement = doc.CreateElement("module");
                    assemblyElement.AppendChild(moduleElement);
                    moduleElement.SetAttribute("versionId", moduleGroup.Key.ModuleVersionId.ToString());

                    foreach (Type type in moduleGroup) {
                        XmlElement typeElement = doc.CreateElement("type");
                        moduleElement.AppendChild(typeElement);
                        typeElement.AppendChild(doc.CreateTextNode(type.FullName));
                    }
                }
            }

            doc.Save(output);
        }

    }
}
