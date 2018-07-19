//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities.Design
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Windows.Forms;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.Workflow.ComponentModel.Design;


    internal class RichListBox : ListBox
    {
        ListItemViewControl activeItemDetailViewControl = null;
        ListItemViewControl activeItemViewControl = null;
        private bool editable;
        Dictionary<object, Point> itemLocations;
        Dictionary<string, Bitmap> listItemBitmapCache = null;
        Dictionary<string, ListItemViewControl> listItemViewRenderers = null;
        private IServiceProvider serviceProvider;

        public RichListBox()
        {
            this.DrawMode = DrawMode.OwnerDrawVariable;
            listItemBitmapCache = new Dictionary<string, Bitmap>();
            listItemViewRenderers = new Dictionary<string, ListItemViewControl>();
            itemLocations = new Dictionary<object, Point>();
            this.DoubleBuffered = true;
        }

        public bool Editable
        {
            get { return editable; }
            set { editable = value; }
        }


        public ListItemViewControl SelectedItemViewControl
        {
            get { return activeItemDetailViewControl; }
            set { activeItemDetailViewControl = value; }
        }


        public IServiceProvider ServiceProvider
        {
            set { serviceProvider = value; }
        }

        public static Type GetDetailViewType(Type editableListItemType)
        {
            if (editableListItemType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("editableListItemType");
            }
            if (!typeof(object).IsAssignableFrom(editableListItemType))
            {
                return null;
            }
            Type viewType = null;
            object[] attribs = editableListItemType.GetCustomAttributes(typeof(ListItemDetailViewAttribute), true);
            if ((attribs != null) && (attribs.Length > 0))
            {
                ListItemDetailViewAttribute viewAttribute = attribs[0] as ListItemDetailViewAttribute;
                if (viewAttribute != null)
                {
                    viewType = viewAttribute.ViewType;
                }
            }
            return viewType;
        }


        public static Type GetItemViewType(Type editableListItemType)
        {
            if (editableListItemType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("editableListItemType");
            }
            if (!typeof(object).IsAssignableFrom(editableListItemType))
            {
                return null;
            }
            Type viewType = null;
            object[] attribs = editableListItemType.GetCustomAttributes(typeof(ListItemViewAttribute), true);
            if ((attribs != null) && (attribs.Length > 0))
            {
                ListItemViewAttribute viewAttribute = attribs[0] as ListItemViewAttribute;
                if (viewAttribute != null)
                {
                    viewType = viewAttribute.ViewType;
                }
            }
            return viewType;
        }


        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0)
            {
                return;
            }
            if (Items.Count < 1)
            {
                return;
            }
            object itemToDraw = Items[e.Index];
            if (itemToDraw == null)
            {
                return;
            }
            ListItemViewControl listItemRenderer = GetRenderer(itemToDraw);
            listItemRenderer.DrawItemState = e.State;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                if (Editable)
                {
                    if (activeItemViewControl != null)
                    {
                        activeItemViewControl.Location = e.Bounds.Location;
                        activeItemViewControl.DrawItemState = e.State;
                    }
                }

            }
            listItemRenderer.Item = itemToDraw;
            listItemRenderer.UpdateView();
            listItemRenderer.TabStop = false;
            listItemRenderer.Parent = this;
            listItemRenderer.Top = -2000;
            Bitmap rendererBitmap = GetRendererBitmap(itemToDraw);
            itemLocations[itemToDraw] = e.Bounds.Location;
            listItemRenderer.DrawToBitmap(rendererBitmap, new Rectangle(new Point(0, 0), listItemRenderer.Size));
            e.Graphics.DrawImage(rendererBitmap, e.Bounds.Location);
        }

        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            base.OnMeasureItem(e);
            if (e.Index < 0)
            {
                return;
            }
            if (Items.Count == 0)
            {
                return;
            }
            object listItem = this.Items[e.Index];
            if (listItem == null)
            {
                return;
            }
            ListItemViewControl listItemRenderer = GetRenderer(listItem);
            listItemRenderer.Item = listItem;
            listItemRenderer.UpdateView();
            listItemRenderer.TabStop = false;
            listItemRenderer.Parent = this;
            listItemRenderer.Top = -2000;
            listItemRenderer.PerformAutoScale();
            e.ItemHeight = listItemRenderer.Height;
            e.ItemWidth = listItemRenderer.Width;
        }


        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            try
            {
                if (this.SelectedIndex < 0)
                {
                    return;
                }
                object selectedItem = Items[this.SelectedIndex];
                if (selectedItem == null)
                {
                    return;
                }
                SelectedItemViewControl = Activator.CreateInstance(GetDetailViewType(selectedItem.GetType())) as ListItemViewControl;
                SelectedItemViewControl.ServiceProvider = this.serviceProvider;
                SelectedItemViewControl.Item = selectedItem;
                SelectedItemViewControl.DrawItemState = DrawItemState.Selected;
                SelectedItemViewControl.ItemChanged += new EventHandler(SelectedItemDetailViewControlItemChanged);
                SelectedItemViewControl.UpdateView();
                if (Editable)
                {
                    if (activeItemViewControl != null)
                    {
                        this.Controls.Remove(activeItemViewControl);
                    }
                    if (selectedItem != null)
                    {

                        activeItemViewControl = Activator.CreateInstance(GetItemViewType(selectedItem.GetType())) as ListItemViewControl;
                        if (itemLocations.ContainsKey(selectedItem))
                        {
                            activeItemDetailViewControl.Location = itemLocations[selectedItem];
                        }
                        activeItemViewControl.DrawItemState = DrawItemState.Selected;
                        activeItemViewControl.UpdateView();
                        activeItemViewControl.Item = selectedItem;

                        this.Controls.Add(activeItemViewControl);
                    }
                }
            }
            catch (Exception exception)
            {
                DesignerHelpers.ShowMessage(serviceProvider, exception.Message, DR.GetString(DR.WorkflowDesignerTitle), MessageBoxButtons.OK,
                    MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
                throw;
            }
            base.OnSelectedIndexChanged(e);
        }

        private ListItemViewControl GetRenderer(object listItem)
        {
            Type viewType = GetItemViewType(listItem.GetType());

            if (!listItemViewRenderers.ContainsKey(viewType.Name))
            {
                listItemViewRenderers.Add(viewType.Name, Activator.CreateInstance(viewType) as ListItemViewControl);
            }
            ListItemViewControl listItemRenderer = listItemViewRenderers[viewType.Name];
            return listItemRenderer;
        }

        private Bitmap GetRendererBitmap(object listItem)
        {
            Type viewType = GetItemViewType(listItem.GetType());
            ListItemViewControl listItemRenderer = GetRenderer(listItem);
            if (listItemRenderer == null)
            {
                return null;
            }
            if (!listItemBitmapCache.ContainsKey(viewType.Name))
            {
                listItemBitmapCache.Add(viewType.Name, new Bitmap(listItemRenderer.Size.Width, listItemRenderer.Size.Height));
            }

            Bitmap rendererBitmap = listItemBitmapCache[viewType.Name];
            return rendererBitmap;
        }

        void SelectedItemDetailViewControlItemChanged(object sender, EventArgs e)
        {
            this.Refresh();
        }
    }
}
