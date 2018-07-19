// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Runtime.Serialization.Json
{
    using System.Collections.Generic;
    using System.Xml;

    /// <summary>
    /// Dummy documentation
    /// </summary>
    public class DataContractJsonSerializerSettings
    {
        private int maxItemsInObjectGraph = int.MaxValue;

        /// <summary>
        /// Gets or sets Dummy documentation
        /// </summary>
        public string RootName { get; set; }

        /// <summary>
        /// Gets or sets Dummy documentation
        /// </summary>
        public IEnumerable<Type> KnownTypes { get; set; }

        /// <summary>
        /// Gets or sets Dummy documentation
        /// </summary>
        public int MaxItemsInObjectGraph
        {
            get
            {
                return this.maxItemsInObjectGraph;
            }

            set
            {
                this.maxItemsInObjectGraph = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Dummy documentation
        /// </summary>
        public bool IgnoreExtensionDataObject { get; set; }

        /// <summary>
        /// Gets or sets Dummy documentation
        /// </summary>
        public IDataContractSurrogate DataContractSurrogate { get; set; }

        /// <summary>
        /// Gets or sets Dummy documentation
        /// </summary>
        public EmitTypeInformation EmitTypeInformation { get; set; }

        /// <summary>
        /// Gets or sets Dummy documentation
        /// </summary>
        public DateTimeFormat DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Dummy documentation
        /// </summary>
        public bool SerializeReadOnlyTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Dummy documentation
        /// </summary>
        public bool UseSimpleDictionaryFormat { get; set; }
    }
}
