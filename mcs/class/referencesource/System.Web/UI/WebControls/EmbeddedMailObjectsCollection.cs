//------------------------------------------------------------------------------
// <copyright file="EmbeddedMailObjectsCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Web.Mail;
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using System.Drawing.Design;
    using System.Web;

    [Editor("System.Web.UI.Design.WebControls.EmbeddedMailObjectCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))]
    public sealed class EmbeddedMailObjectsCollection : CollectionBase {

        public EmbeddedMailObject this[int index] {
            get {
                return (EmbeddedMailObject)List[index];
            }
            set {
                List[index] = value;
            }
        }
        
        public int Add(EmbeddedMailObject value) {
            return List.Add(value);
        }

        public bool Contains(EmbeddedMailObject value) {
            return List.Contains(value);
        }

        public void CopyTo(EmbeddedMailObject[] array, int index) {
            List.CopyTo(array, index);
        }

        public int IndexOf(EmbeddedMailObject value) {
            return List.IndexOf(value);
        }

        public void Insert(int index, EmbeddedMailObject value) {
            List.Insert(index, value);
        }

        protected override void OnValidate(object value) {
            base.OnValidate(value);
            if (value == null) {
                throw new ArgumentNullException("value", SR.GetString(SR.Collection_CantAddNull));
            }
            if (!(value is EmbeddedMailObject)) {
                throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "EmbeddedMailObject"), "value");
            }
        }

        public void Remove(EmbeddedMailObject value) {
            List.Remove(value);
        }
    }
}
