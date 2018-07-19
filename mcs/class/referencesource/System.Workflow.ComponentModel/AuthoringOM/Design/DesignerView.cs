#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.IO;
    using System.Drawing;
    using System.CodeDom;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Design;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms.Design;
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Serialization;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Workflow.ComponentModel.Design;
    using System.Runtime.Serialization.Formatters.Binary;

    //

    #region Class DesignerView
    /// <summary>
    /// Holds information about the views supported by CompositeActivityDesigner
    /// </summary>
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class DesignerView
    {
        private static int MaxViewName = 150;
        private int viewId;
        private string text;
        private Image image;
        private IDictionary userData;
        private ActivityDesigner designer;

        /// <summary>
        /// Constructor for DesignerView
        /// </summary>
        /// <param name="id">Identifier which unqiuely identified the view</param>
        /// <param name="name">Name of the view</param>
        /// <param name="image">Image associated with the view</param>
        public DesignerView(int viewId, string text, Image image)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (image == null)
                throw new ArgumentNullException("image");

            this.viewId = viewId;
            this.text = ((text.Length > MaxViewName)) ? text.Substring(0, MaxViewName) + "..." : text;
            this.image = image;
        }

        public DesignerView(int viewId, string text, Image image, ActivityDesigner associatedDesigner)
            : this(viewId, text, image)
        {
            if (associatedDesigner == null)
                throw new ArgumentNullException("associatedDesigner");

            this.designer = associatedDesigner;
        }

        /// <summary>
        /// Gets the identifier associated with view
        /// </summary>
        public int ViewId
        {
            get
            {
                return this.viewId;
            }
        }

        /// <summary>
        /// Gets the name associated with the view
        /// </summary>
        public string Text
        {
            get
            {
                return this.text;
            }
        }

        /// <summary>
        /// Gets the image associated with the view
        /// </summary>
        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        /// <summary>
        /// Gets the userdata to be associated with the view
        /// </summary>
        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                    this.userData = new HybridDictionary();
                return this.userData;
            }
        }

        /// <summary>
        /// Gets the ActivityDesigner associated with the view
        /// </summary>
        public virtual ActivityDesigner AssociatedDesigner
        {
            get
            {
                return this.designer;
            }
        }

        /// <summary>
        /// Called when activating the view
        /// </summary>
        public virtual void OnActivate()
        {
        }

        /// <summary>
        /// Called when deactivating the view 
        /// </summary>
        public virtual void OnDeactivate()
        {
        }

        public override bool Equals(object obj)
        {
            DesignerView view = obj as DesignerView;
            if (view == null)
                return false;

            return (this.viewId == view.viewId);
        }

        public override int GetHashCode()
        {
            return this.viewId;
        }
    }
    #endregion

}
