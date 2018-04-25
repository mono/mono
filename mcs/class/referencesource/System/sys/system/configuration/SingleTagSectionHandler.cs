//------------------------------------------------------------------------------
// <copyright file="SingleTagSectionHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration {
    using System.Collections;
    using System.Xml;

    /**
     * Single-tag dictionary config factory
     *
     * Use for tags of the form: <MySingleTag key1="value1" ... keyN="valueN"/> 
     */
    /// <devdoc>
    /// </devdoc>
    public class SingleTagSectionHandler : IConfigurationSectionHandler {
        /**
         * Create
         *
         * Given a partially composed config object (possibly null)
         * and some input from the config system, return a
         * further partially composed config object
         */
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual object Create(Object parent, Object context, XmlNode section) {
            Hashtable result;

            // start result off as a shallow clone of the parent

            if (parent == null)
                result = new Hashtable();
            else
                result = new Hashtable((IDictionary)parent);

            // verify that there are no children

            HandlerBase.CheckForChildNodes(section);
            
            // iterate through each XML section in order and apply the directives

            foreach (XmlAttribute attribute in section.Attributes) {
                // handle name-value pairs
                result[attribute.Name] = attribute.Value;
            }

            return result;
        }
    }
}
