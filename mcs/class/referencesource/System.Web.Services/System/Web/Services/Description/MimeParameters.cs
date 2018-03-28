//------------------------------------------------------------------------------
// <copyright file="MimeParameters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Services.Description {

    using System.Web.Services;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.Xml.Schema;
    using System.Collections;
    using System;
    using System.Reflection;

    internal class MimeParameterCollection : CollectionBase {
        Type writerType;

        internal Type WriterType {
            get { return writerType; }
            set { writerType = value; }
        }
        
        internal MimeParameter this[int index] {
            get { return (MimeParameter)List[index]; }
            set { List[index] = value; }
        }
        
        internal int Add(MimeParameter parameter) {
            return List.Add(parameter);
        }
        
        internal void Insert(int index, MimeParameter parameter) {
            List.Insert(index, parameter);
        }
        
        internal int IndexOf(MimeParameter parameter) {
            return List.IndexOf(parameter);
        }
        
        internal bool Contains(MimeParameter parameter) {
            return List.Contains(parameter);
        }
        
        internal void Remove(MimeParameter parameter) {
            List.Remove(parameter);
        }
        
        internal void CopyTo(MimeParameter[] array, int index) {
            List.CopyTo(array, index);
        }
        
    }
}
