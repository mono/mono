namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Collections;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.ComponentModel;
    using System.Globalization;
    using System.Drawing.Imaging;
    using System.Drawing.Drawing2D;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using NativeMethods = System.Workflow.Interop.NativeMethods;

    #region Enums And Structs
    internal enum AnchorAlignment { Near = 0, Far = 1 }
    #endregion

    #region ItemInfo Class
    internal class ItemInfo
    {
        private int commandID;
        private IDictionary userData;
        private Image image;
        private string text;

        public ItemInfo(int id)
        {
            this.commandID = id;
        }

        public ItemInfo(int id, Image image, string text)
            : this(id)
        {
            this.image = image;
            this.text = text;
        }

        public int Identifier
        {
            get
            {
                return this.commandID;
            }
        }

        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                    this.userData = new HybridDictionary();
                return this.userData;
            }
        }

        public Image Image
        {
            get
            {
                return this.image;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj is ItemInfo)
                return (((ItemInfo)obj).commandID == this.commandID);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ this.commandID.GetHashCode();
        }
    }
    #endregion

    #region SelectionChangeEventHandler Class
    internal delegate void SelectionChangeEventHandler<T>(object sender, T e);

    internal class SelectionChangeEventArgs : EventArgs
    {
        private ItemInfo previousItem;
        private ItemInfo currentItem;

        public SelectionChangeEventArgs(ItemInfo previousItem, ItemInfo currentItem)
        {
            this.previousItem = previousItem;
            this.currentItem = currentItem;
        }

        public ItemInfo CurrentItem
        {
            get
            {
                return this.currentItem;
            }
        }
    }
    #endregion

    #region Non Theme Enabled Controls

    #region Class PageStrip
    internal sealed class PageStrip : ScrollableItemStrip
    {
        private static Brush SelectionBrush = new SolidBrush(Color.FromArgb(255, 195, 107));
        private static Brush HighliteBrush = new SolidBrush(Color.FromArgb(100, 255, 195, 107));

        public PageStrip(IServiceProvider serviceProvider, Size itemSize)
            : base(serviceProvider, Orientation.Horizontal, itemSize, Size.Empty)
        {
        }

        protected override ItemStrip CreateItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin)
        {
            return new PageItemStrip(serviceProvider, orientation, itemSize, margin);
        }

        public override void Draw(Graphics graphics)
        {
            GraphicsContainer graphicsState = graphics.BeginContainer();

            Rectangle bounds = Bounds;
            using (Region clipRegion = new Region(new Rectangle(bounds.X, bounds.Y, bounds.Width + 1, bounds.Height + 1)))
            {
                graphics.Clip = clipRegion;

                base.itemStrip.Draw(graphics);

                if (base.itemStrip.ScrollPosition > 0)
                    DrawButton(graphics, (Orientation == Orientation.Horizontal) ? ScrollButton.Left : ScrollButton.Up);

                if (base.itemStrip.ScrollPosition + base.itemStrip.MaxVisibleItems < base.itemStrip.Items.Count)
                    DrawButton(graphics, (Orientation == Orientation.Horizontal) ? ScrollButton.Right : ScrollButton.Down);
            }

            graphics.EndContainer(graphicsState);
        }

        private void DrawButton(Graphics graphics, ScrollButton scrollButton)
        {
            Rectangle buttonBounds = GetButtonBounds(scrollButton);

            if (Orientation == Orientation.Horizontal)
                buttonBounds.Inflate(-base.itemStrip.ItemSize.Width / 6, -base.itemStrip.ItemSize.Height / 4);
            else
                buttonBounds.Inflate(-base.itemStrip.ItemSize.Width / 4, -base.itemStrip.ItemSize.Height / 6);

            if (ActiveButton == scrollButton)
            {
                buttonBounds.Offset(1, 1);

                Size inflateSize = (Orientation == Orientation.Horizontal) ? new Size(0, 2) : new Size(2, 0);
                buttonBounds.Inflate(inflateSize.Width, inflateSize.Height);

                graphics.FillRectangle(SelectionBrush, buttonBounds);
                graphics.DrawRectangle(Pens.Black, buttonBounds);

                buttonBounds.Inflate(-inflateSize.Width, -inflateSize.Height);
            }

            using (GraphicsPath graphicsPath = ActivityDesignerPaint.GetScrollIndicatorPath(buttonBounds, scrollButton))
            {
                graphics.FillPath(Brushes.Black, graphicsPath);
                graphics.DrawPath(Pens.Black, graphicsPath);
            }
        }

        private sealed class PageItemStrip : ItemStrip
        {
            public PageItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin)
                : base(serviceProvider, orientation, itemSize, margin)
            {
            }

            public override void Draw(Graphics graphics)
            {
                GraphicsContainer graphicsState = graphics.BeginContainer();

                Rectangle bounds = Bounds;
                using (Region clipRegion = new Region(new Rectangle(bounds.X, bounds.Y, bounds.Width + 1, bounds.Height + 1)))
                {
                    graphics.Clip = clipRegion;

                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    format.Trimming = StringTrimming.Character;
                    format.FormatFlags = StringFormatFlags.NoWrap;

                    int visibleItems = MaxVisibleItems;
                    int scrollPosition = ScrollPosition;
                    for (int itemIndex = scrollPosition; itemIndex < Items.Count && itemIndex < (scrollPosition + visibleItems); itemIndex++)
                    {
                        ItemInfo itemInfo = Items[itemIndex];
                        Rectangle itemRectangle = GetItemBounds(itemInfo);

                        int margin = itemRectangle.Width / 5;
                        GraphicsPath[] graphicsPath = ActivityDesignerPaint.GetPagePaths(itemRectangle, margin, DesignerContentAlignment.TopRight);
                        using (GraphicsPath pagePath = graphicsPath[0])
                        using (GraphicsPath pageFoldPath = graphicsPath[1])
                        {
                            Brush pageBrush = Brushes.White;
                            if (SelectedItem == itemInfo)
                                pageBrush = PageStrip.SelectionBrush;
                            else if (HighlitedItem == itemInfo)
                                pageBrush = PageStrip.HighliteBrush;

                            graphics.FillPath(pageBrush, pagePath);
                            graphics.DrawPath(Pens.DarkBlue, pagePath);
                            graphics.FillPath(Brushes.White, pageFoldPath);
                            graphics.DrawPath(Pens.DarkBlue, pageFoldPath);

                            if (itemInfo.Image == null)
                            {
                                itemRectangle.Y += margin;
                                itemRectangle.Height -= margin;
                                int index = itemIndex + 1;
                                graphics.DrawString(index.ToString(CultureInfo.CurrentCulture), Control.DefaultFont, SystemBrushes.ControlText, (RectangleF)itemRectangle, format);
                            }
                            else
                            {
                                itemRectangle.Y += margin; itemRectangle.Height -= margin;
                                itemRectangle.X += (itemRectangle.Width - itemRectangle.Height) / 2;
                                itemRectangle.Width = itemRectangle.Height;
                                itemRectangle.Inflate(-2, -2);
                                ActivityDesignerPaint.DrawImage(graphics, itemInfo.Image, itemRectangle, DesignerContentAlignment.Center);
                            }
                        }
                    }
                }

                graphics.EndContainer(graphicsState);
            }
        }
    }
    #endregion

    #region Class ScrollableItemStrip
    internal abstract class ScrollableItemStrip
    {
        private Rectangle bounds = Rectangle.Empty;
        protected IServiceProvider serviceProvider = null;
        protected ItemStrip itemStrip;
        private Orientation orientation;
        private Size margin;
        private Size buttonSize;
        private ScrollButton activeButton = ScrollButton.Min;

        public ScrollableItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin)
        {
            Debug.Assert(serviceProvider != null);
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;
            this.orientation = orientation;
            this.margin = margin;

            if (orientation == Orientation.Horizontal)
                this.buttonSize = new Size(itemSize.Width * 2 / 3, itemSize.Height);
            else
                this.buttonSize = new Size(itemSize.Width, itemSize.Height * 2 / 3);

            this.itemStrip = CreateItemStrip(serviceProvider, orientation, itemSize, margin);
            this.itemStrip.ScrollPositionChanged += new EventHandler(OnScrollPositionChanged);
        }

        #region Public Properties and Methods
        public IList<ItemInfo> Items
        {
            get
            {
                return this.itemStrip.Items;
            }
        }

        public ItemInfo SelectedItem
        {
            get
            {
                return this.itemStrip.SelectedItem;
            }

            set
            {
                this.itemStrip.SelectedItem = value;
            }
        }

        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged
        {
            add
            {
                this.itemStrip.SelectionChanged += value;
            }

            remove
            {
                this.itemStrip.SelectionChanged -= value;
            }
        }

        public Point Location
        {
            get
            {
                return this.bounds.Location;
            }

            set
            {
                if (this.bounds.Location != value)
                {
                    Invalidate();

                    this.bounds.Location = value;

                    Rectangle leftScrollButtonBounds = GetButtonBounds(ScrollButton.Left);
                    if (this.orientation == Orientation.Horizontal)
                        this.itemStrip.Location = new Point(leftScrollButtonBounds.Right, leftScrollButtonBounds.Top);
                    else
                        this.itemStrip.Location = new Point(leftScrollButtonBounds.Left, leftScrollButtonBounds.Bottom);

                    Invalidate();
                }
            }
        }

        public Size Size
        {
            get
            {
                return this.bounds.Size;
            }

            set
            {
                if (this.bounds.Size != value)
                {
                    Invalidate();

                    this.bounds.Size = value;

                    //Set item strip size
                    Size reqdSize = this.itemStrip.RequiredSize;

                    int availableSize = 0;
                    if (this.orientation == Orientation.Horizontal)
                    {
                        availableSize = this.bounds.Width - (2 * (2 * this.margin.Width + this.buttonSize.Width));
                        availableSize -= this.margin.Width;
                        if (this.margin.Width + this.itemStrip.ItemSize.Width > 0)
                            availableSize -= (availableSize % (this.margin.Width + this.itemStrip.ItemSize.Width));
                        this.itemStrip.Size = new Size(Math.Min(availableSize, reqdSize.Width), Math.Min(this.bounds.Height, reqdSize.Height));
                    }
                    else
                    {
                        availableSize = this.bounds.Height - (2 * (2 * this.margin.Height + this.buttonSize.Height));
                        availableSize -= this.margin.Height;
                        if (this.margin.Height + this.itemStrip.ItemSize.Height > 0)
                            availableSize -= (availableSize % (this.margin.Height + this.itemStrip.ItemSize.Height));
                        this.itemStrip.Size = new Size(Math.Min(this.bounds.Width, reqdSize.Width), Math.Min(availableSize, reqdSize.Height));
                    }

                    Invalidate();
                }
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public Orientation Orientation
        {
            get
            {
                return this.orientation;
            }
        }

        public abstract void Draw(Graphics graphics);
        #endregion

        #region Mouse Messages
        public virtual void OnMouseDragBegin(Point initialDragPoint, MouseEventArgs e)
        {
        }

        public virtual void OnMouseDragMove(MouseEventArgs e)
        {
        }

        public virtual void OnMouseDragEnd()
        {
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            this.itemStrip.OnMouseEnter(e);
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            Point mousePoint = new Point(e.X, e.Y);
            if (this.itemStrip.Bounds.Contains(mousePoint))
            {
                this.itemStrip.OnMouseDown(e);
            }
            else
            {
                ScrollButton scrollButton = HitTest(mousePoint);

                if (scrollButton != ScrollButton.Min)
                {
                    int incr = (scrollButton == ScrollButton.Left || scrollButton == ScrollButton.Up) ? -1 : 1;
                    this.itemStrip.ScrollPosition = this.itemStrip.ScrollPosition + incr;
                }

                if (e.Button == MouseButtons.Left)
                    ActiveButton = scrollButton;
            }
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            this.itemStrip.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
                ActiveButton = HitTest(new Point(e.X, e.Y));
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            Point mousePoint = new Point(e.X, e.Y);
            if (this.itemStrip.Bounds.Contains(mousePoint))
                this.itemStrip.OnMouseUp(e);

            ActiveButton = ScrollButton.Min;
        }

        public virtual void OnMouseLeave()
        {
            this.itemStrip.OnMouseLeave();

            ActiveButton = ScrollButton.Min;
        }
        #endregion

        #region Protected Properties and Methods
        protected abstract ItemStrip CreateItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin);

        protected Rectangle GetButtonBounds(ScrollButton scrollButton)
        {
            Rectangle buttonRectangle = Rectangle.Empty;
            buttonRectangle.Size = this.buttonSize;

            if (scrollButton == ScrollButton.Left || scrollButton == ScrollButton.Up)
            {
                buttonRectangle.X = this.bounds.X + this.margin.Width;
                buttonRectangle.Y = this.bounds.Y + this.margin.Height;
            }
            else if (scrollButton == ScrollButton.Right || scrollButton == ScrollButton.Down)
            {
                if (this.orientation == Orientation.Horizontal)
                {
                    buttonRectangle.X = this.bounds.X + this.margin.Width + buttonRectangle.Size.Width + this.itemStrip.Size.Width;
                    if (buttonRectangle.X >= this.bounds.Right)
                        buttonRectangle.X = this.bounds.Right - buttonRectangle.Size.Width;

                    buttonRectangle.Y = this.bounds.Y + this.margin.Height;
                }
                else
                {
                    buttonRectangle.X = this.bounds.X + this.margin.Width;

                    buttonRectangle.Y = this.bounds.Y + this.margin.Height + buttonRectangle.Size.Height + this.itemStrip.Size.Height;
                    if (buttonRectangle.Y >= this.bounds.Bottom)
                        buttonRectangle.Y = this.bounds.Bottom - buttonRectangle.Size.Height;
                }
            }

            return buttonRectangle;
        }

        protected ScrollButton HitTest(Point mousePoint)
        {
            if (this.itemStrip.ScrollPosition > 0)
            {
                ScrollButton scrollButton = (this.orientation == Orientation.Horizontal) ? ScrollButton.Left : ScrollButton.Up;
                Rectangle buttonBounds = GetButtonBounds(scrollButton);
                if (buttonBounds.Contains(mousePoint))
                    return scrollButton;
            }

            if (this.itemStrip.ScrollPosition + this.itemStrip.MaxVisibleItems < this.itemStrip.Items.Count)
            {
                ScrollButton scrollButton = (this.orientation == Orientation.Horizontal) ? ScrollButton.Right : ScrollButton.Down;
                Rectangle buttonBounds = GetButtonBounds(scrollButton);
                if (buttonBounds.Contains(mousePoint))
                    return scrollButton;
            }

            return ScrollButton.Min;
        }

        protected ScrollButton ActiveButton
        {
            get
            {
                return this.activeButton;
            }

            private set
            {
                if (this.activeButton != value)
                {
                    this.activeButton = value;
                    Invalidate();
                }
            }
        }

        protected void Invalidate()
        {
            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
                workflowView.InvalidateLogicalRectangle(this.bounds);
        }
        #endregion

        #region Private Methods and Events
        private void OnScrollPositionChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
        #endregion
    }
    #endregion

    #region Class ItemStrip
    //

    internal abstract class ItemStrip
    {
        #region Members and Constructors
        protected IServiceProvider serviceProvider = null;
        private ItemList<ItemInfo> items = null;
        private ItemInfo selectedItem = null;
        private ItemInfo highlitedItem = null;
        private int scrollPosition = 0;
        private Rectangle bounds = Rectangle.Empty;
        private Orientation orientation;
        private Size itemSize = new Size(16, 16);
        private Size margin = Size.Empty;

        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged;
        public event EventHandler ScrollPositionChanged;

        public ItemStrip(IServiceProvider serviceProvider, Orientation orientation, Size itemSize, Size margin)
        {
            Debug.Assert(serviceProvider != null);
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            this.serviceProvider = serviceProvider;
            this.orientation = orientation;
            this.itemSize = itemSize;
            this.margin = margin;

            this.items = new ItemList<ItemInfo>(this);
            this.items.ListChanging += new ItemListChangeEventHandler<ItemInfo>(OnItemsChanging);
            this.items.ListChanged += new ItemListChangeEventHandler<ItemInfo>(OnItemsChanged);
        }
        #endregion

        #region Public Properties and Methods
        public IList<ItemInfo> Items
        {
            get
            {
                return this.items;
            }
        }

        public ItemInfo SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                if (this.selectedItem == value)
                    return;

                ItemInfo previousSelection = this.selectedItem;
                this.selectedItem = value;

                EnsureScrollPositionAndSelection();
                Invalidate();

                if (SelectionChanged != null)
                    SelectionChanged(this, new SelectionChangeEventArgs(previousSelection, this.selectedItem));
            }
        }

        public Point Location
        {
            get
            {
                return this.bounds.Location;
            }

            set
            {
                if (this.bounds.Location != value)
                {
                    Invalidate();
                    this.bounds.Location = value;
                    Invalidate();
                }
            }
        }

        public Size Size
        {
            get
            {
                return this.bounds.Size;
            }

            set
            {
                if (this.bounds.Size != value)
                {
                    Invalidate();
                    this.bounds.Size = value;
                    EnsureScrollPositionAndSelection();
                    Invalidate();
                }
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public int ScrollPosition
        {
            get
            {
                return this.scrollPosition;
            }

            set
            {
                if (value < 0)
                    return;

                int newPosition = value;
                int visibleItems = MaxVisibleItems;

                //If there are more items in the strip than displayed then we need to display what ever we can
                if (this.items.Count >= visibleItems && ((this.items.Count - newPosition) < visibleItems))
                    newPosition = this.items.Count - visibleItems;

                if (newPosition >= 0 && newPosition <= Math.Max(this.items.Count - visibleItems + 1, 0))
                {
                    this.scrollPosition = newPosition;
                    Invalidate();

                    if (this.ScrollPositionChanged != null)
                        ScrollPositionChanged(this, EventArgs.Empty);
                }
            }
        }

        public Rectangle GetItemBounds(ItemInfo itemInfo)
        {
            int itemIndex = this.items.IndexOf(itemInfo);
            if (itemIndex < 0 || itemIndex < this.scrollPosition || itemIndex >= this.scrollPosition + MaxVisibleItems)
                return Rectangle.Empty;

            Rectangle itemRectangle = Rectangle.Empty;
            itemIndex = itemIndex - this.scrollPosition;

            if (this.orientation == Orientation.Horizontal)
            {
                itemRectangle.X = bounds.Left + (itemIndex * this.itemSize.Width) + ((itemIndex + 1) * this.margin.Width);
                itemRectangle.Y = bounds.Top + this.margin.Height;
            }
            else
            {
                itemRectangle.X = bounds.Left + this.margin.Width;
                itemRectangle.Y = bounds.Top + (itemIndex * this.itemSize.Height) + ((itemIndex + 1) * this.margin.Height);
            }

            itemRectangle.Size = this.itemSize;
            return itemRectangle;
        }

        public abstract void Draw(Graphics graphics);

        public ItemInfo HitTest(Point point)
        {
            ItemInfo itemHit = null;

            foreach (ItemInfo item in this.items)
            {
                if (GetItemBounds(item).Contains(point))
                {
                    itemHit = item;
                    break;
                }
            }

            return itemHit;
        }

        public Size RequiredSize
        {
            get
            {
                Size reqdSize = Size.Empty;

                if (this.orientation == Orientation.Horizontal)
                {
                    reqdSize.Width = (this.items.Count * this.itemSize.Width) + ((this.items.Count + 1) * this.margin.Width);
                    reqdSize.Height = this.itemSize.Height + 2 * this.margin.Height;
                }
                else
                {
                    reqdSize.Width = this.itemSize.Width + 2 * this.margin.Width;
                    reqdSize.Height = (this.items.Count * this.itemSize.Height) + ((this.items.Count + 1) * this.margin.Height);
                }

                return reqdSize;
            }
        }

        public Size ItemSize
        {
            get
            {
                return this.itemSize;
            }
        }
        #endregion

        #region Mouse Messages
        public virtual void OnMouseDragBegin(Point initialDragPoint, MouseEventArgs e)
        {
        }

        public virtual void OnMouseDragMove(MouseEventArgs e)
        {
        }

        public virtual void OnMouseDragEnd()
        {
        }

        public virtual void OnMouseEnter(MouseEventArgs e)
        {
            ItemInfo itemHit = HitTest(new Point(e.X, e.Y));
            if (itemHit != null && itemHit.Text != null && itemHit.Text.Length > 0)
                ShowInfoTip(itemHit.Text);

            HighlitedItem = itemHit;
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            ItemInfo itemHit = HitTest(new Point(e.X, e.Y));
            if (itemHit != null)
            {
                SelectedItem = itemHit;

                if (itemHit.Text != null && itemHit.Text.Length > 0)
                    ShowInfoTip(itemHit.Text);
            }

            HighlitedItem = itemHit;
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            ItemInfo itemHit = HitTest(new Point(e.X, e.Y));
            if (itemHit != null && itemHit.Text != null && itemHit.Text.Length > 0)
                ShowInfoTip(itemHit.Text);

            HighlitedItem = itemHit;
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public virtual void OnMouseLeave()
        {
            ShowInfoTip(String.Empty);
            HighlitedItem = null;
        }
        #endregion

        #region Protected Properties and Methods
        protected internal int MaxVisibleItems
        {
            get
            {
                int visibleItemCount = 0;

                if (this.orientation == Orientation.Horizontal)
                {
                    int totalStripSize = this.bounds.Width - this.margin.Width;
                    visibleItemCount = totalStripSize / Math.Max((this.itemSize.Width + this.margin.Width), 1);
                }
                else
                {
                    int totalStripSize = this.bounds.Height - this.margin.Height;
                    visibleItemCount = totalStripSize / Math.Max((this.itemSize.Height + this.margin.Height), 1);
                }

                return Math.Max(visibleItemCount, 1);
            }
        }

        protected ItemInfo HighlitedItem
        {
            get
            {
                return this.highlitedItem;
            }

            private set
            {
                if (this.highlitedItem != value)
                {
                    this.highlitedItem = value;
                    Invalidate();
                }
            }
        }

        protected void Invalidate()
        {
            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
                workflowView.InvalidateLogicalRectangle(this.bounds);
        }
        #endregion

        #region Helpers
        private void ShowInfoTip(string infoTip)
        {
            WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
            if (workflowView != null)
                workflowView.ShowInfoTip(String.Empty, infoTip);
        }

        private void EnsureScrollPositionAndSelection()
        {
            int newPosition = this.scrollPosition;
            if (this.selectedItem != null)
            {
                //The logic used for the ensuring the selected item is that there needs to be atleast one 
                //If marker falls outside the range then ensure it to a visible point
                int index = this.items.IndexOf(this.selectedItem);
                if (index >= 0)
                {
                    if (index <= this.scrollPosition)
                        newPosition = Math.Max(index - 1, 0);

                    int visibleItems = MaxVisibleItems;
                    if (index >= (this.scrollPosition + visibleItems - 1))
                        newPosition = index - visibleItems + 2;
                }
            }

            ScrollPosition = newPosition;
        }

        private void OnItemsChanging(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Remove && e.RemovedItems.Count > 0 && this.selectedItem == e.RemovedItems[0])
            {
                int nextIndex = this.items.IndexOf(e.RemovedItems[0]);
                nextIndex += (nextIndex < this.items.Count - 1) ? 1 : -1;
                SelectedItem = (nextIndex >= 0 && nextIndex < this.items.Count) ? this.items[nextIndex] : null;
            }
        }

        private void OnItemsChanged(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Add)
            {
                if (e.AddedItems.Count > 0)
                    SelectedItem = e.AddedItems[0];
            }
            else if (e.Action == ItemListChangeAction.Remove)
            {
                EnsureScrollPositionAndSelection();
            }

            Invalidate();
        }
        #endregion
    }
    #endregion

    #region Class ItemPalette
    //
    internal sealed class ItemPalette
    {
        #region Members and Constructor
        private ItemList<ItemInfo> items = null;
        private ItemInfo selectedItem = null;
        private Palette palette = null;
        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged;
        public event EventHandler Closed;
        private Font font = null;

        public ItemPalette()
        {
            this.items = new ItemList<ItemInfo>(this);
        }
        #endregion

        #region Public Properties and Methods
        public IList<ItemInfo> Items
        {
            get
            {
                return this.items;
            }
        }

        public ItemInfo SelectedItem
        {
            get
            {
                return this.selectedItem;
            }

            set
            {
                if (this.selectedItem == value)
                    return;

                ItemInfo previousItem = this.selectedItem;
                this.selectedItem = value;
                if (this.SelectionChanged != null)
                {
                    this.SelectionChanged(this, new SelectionChangeEventArgs(previousItem, this.selectedItem));
                    if (this.palette != null)
                        this.palette.Invalidate();
                }
            }
        }

        public void SetFont(Font font)
        {
            this.font = font;
        }

        public bool IsVisible
        {
            get
            {
                return (this.palette != null && this.palette.Visible);
            }
        }

        public void Show(Point location)
        {
            if (this.palette != null)
                DestroyPalette(this.palette);

            //Sometimes due to the way events are fired palette needs to be destroyed soon after creation
            this.palette = new Palette(this, location);
            this.palette.Font = this.font;
            this.palette.Show();
            this.palette.Focus();
            this.palette.LostFocus += new System.EventHandler(OnPaletteLostFocus);
        }
        #endregion

        #region Helpers
        private void OnPaletteLostFocus(object sender, EventArgs e)
        {
            DestroyPalette(sender as Palette);
        }

        private void DestroyPalette(Palette palette)
        {
            if (palette != null)
            {
                if (this.Closed != null)
                    this.Closed(this, EventArgs.Empty);

                palette.LostFocus -= new System.EventHandler(OnPaletteLostFocus);
                palette.Close();
                palette.Dispose();
                this.palette = null;
            }
        }
        #endregion

        #region Class Palette
        private sealed class Palette : Form
        {
            private Size imageRectangle = new Size(20, 20);
            private Size imageSize = new Size(16, 16);
            private Size selectionItemMargin = new Size(1, 1);

            private int leftTextMargin = 5;
            private int rightTextMargin = 20;

            private List<Rectangle> itemRectangles = new List<Rectangle>();
            private int menuItemCount = 0;

            private Rectangle workingRectangle = Rectangle.Empty; //this rectangle we would be fitting the form into

            private static readonly int DropShadowWidth = AmbientTheme.DropShadowWidth;

            private int maximumTextWidth = 500;

            private Rectangle formRectangle;
            private Rectangle leftGradientRectangle;

            private int itemHeight = 0;
            private int itemWidth = 0;
            private int maxTextHeight = 0;

            private int activeIndex = -1;

            private ItemPalette parent = null;
            private PaletteShadow paletteShadow;

            private ItemList<ItemInfo> enabledItems;

            public Palette(ItemPalette parent, Point location)
            {
                this.parent = parent;

                //copy over only items with enabled commands
                this.enabledItems = new ItemList<ItemInfo>(this);
                foreach (ItemInfo item in this.parent.items)
                {
                    ActivityDesignerVerb smartVerb = item.UserData[DesignerUserDataKeys.DesignerVerb] as ActivityDesignerVerb;
                    if (smartVerb == null || smartVerb.Enabled)
                        this.enabledItems.Add(item);
                }

                this.menuItemCount = this.enabledItems.Count;

                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                            ControlStyles.UserPaint |
                            ControlStyles.SupportsTransparentBackColor |
                            ControlStyles.AllPaintingInWmPaint, true);

                FormBorderStyle = FormBorderStyle.None;
                BackColor = Color.White;
                ShowInTaskbar = false;
                MaximizeBox = false;
                ControlBox = false;
                StartPosition = FormStartPosition.Manual;

                Screen closestScreen = Screen.FromPoint(location);
                this.workingRectangle = closestScreen.WorkingArea;

                PreparePalette(location);

                this.paletteShadow = new PaletteShadow(this);
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                DestroyShadow();
            }

            protected override void OnVisibleChanged(EventArgs e)
            {
                base.OnVisibleChanged(e);

                if (Visible)
                {
                    this.paletteShadow.Show();
                    BringToFront();
                    Focus();
                }
                else
                {
                    this.paletteShadow.Hide();
                }
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (e.KeyCode == Keys.Enter)
                {
                    if (ActiveItem != null)
                    {
                        try
                        {
                            this.parent.SelectedItem = ActiveItem;
                        }
                        finally
                        {
                            this.parent.DestroyPalette(this);
                        }
                    }
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    this.parent.DestroyPalette(this);
                }
                else if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                {
                    int index = -1;
                    if (this.activeIndex != -1)
                        index = this.activeIndex;

                    int oldIndex = index;

                    if (index >= 0)
                    {
                        if (e.KeyCode == Keys.Up)
                            index--;
                        else if (e.KeyCode == Keys.Down)
                            index++;
                    }
                    else
                    {
                        index = 0;
                    }

                    if (index >= 0 && index < this.enabledItems.Count)
                        SetActiveItem(index);
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                ItemInfo selectedItem = null;
                Point mousePoint = new Point(e.X, e.Y);
                for (int i = 0; i < this.enabledItems.Count; i++)
                {
                    if (GetItemBounds(i).Contains(mousePoint))
                    {
                        selectedItem = this.enabledItems[i];
                        break;
                    }
                }

                if (selectedItem != null)
                {
                    try
                    {
                        this.parent.SelectedItem = selectedItem;
                    }
                    finally
                    {
                        this.parent.DestroyPalette(this);
                    }
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                Point mousePoint = new Point(e.X, e.Y);
                for (int i = 0; i < this.enabledItems.Count; i++)
                {
                    if (GetItemBounds(i).Contains(mousePoint))
                    {
                        SetActiveItem(i);
                        break;
                    }
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                SetActiveItem(-1);
            }

            protected override void OnPaint(PaintEventArgs paintArgs)
            {
                Graphics graphics = paintArgs.Graphics;

                graphics.FillRectangle(SystemBrushes.Window, this.formRectangle);
                graphics.DrawRectangle(SystemPens.ControlDarkDark, this.formRectangle.X, this.formRectangle.Y, this.formRectangle.Width - 1, this.formRectangle.Height - 1);

                using (Brush gradientBrush = new LinearGradientBrush(new Point(this.leftGradientRectangle.Left, this.leftGradientRectangle.Top), new Point(this.leftGradientRectangle.Right, this.leftGradientRectangle.Top), SystemColors.Window, SystemColors.ScrollBar))
                {
                    graphics.FillRectangle(gradientBrush, this.leftGradientRectangle);
                }

                for (int i = 0; i < this.enabledItems.Count; i++)
                {
                    Rectangle itemBounds = GetItemBounds(i);

                    if (this.activeIndex == i)
                    {
                        graphics.FillRectangle(SystemBrushes.InactiveCaptionText, itemBounds.X, itemBounds.Y, itemBounds.Width - 1, itemBounds.Height - 1);
                        graphics.DrawRectangle(SystemPens.ActiveCaption, itemBounds.X, itemBounds.Y, itemBounds.Width - 1, itemBounds.Height - 1);
                    }

                    if (this.enabledItems[i].Image != null)
                    {
                        Point imagePoint = new Point(itemBounds.Left + 3, itemBounds.Top + 3);
                        Size imageSize = this.enabledItems[i].Image.Size;

                        //this code is to support a border around currently supported item, which unfortunatly is not currently available
                        //if (this.enabledItems[i] == this.parent.SelectedItem)
                        //{
                        //    Rectangle hotTrack = new Rectangle(imagePoint, imageSize);
                        //    hotTrack.Inflate(2, 2);
                        //    graphics.FillRectangle(SystemBrushes.InactiveCaptionText, hotTrack);//ActiveCaption
                        //    graphics.DrawRectangle(SystemPens.HotTrack, hotTrack);
                        //}

                        graphics.DrawImage(this.enabledItems[i].Image, new Rectangle(imagePoint, imageSize), new Rectangle(Point.Empty, imageSize), GraphicsUnit.Pixel);
                    }

                    Rectangle textRectangle = new Rectangle(itemBounds.Left + 20 + 5 + 2, itemBounds.Top + 1, this.itemWidth - (20 + 5 + 4), this.itemHeight - 3);

                    int textVerticalOffset = textRectangle.Height - this.maxTextHeight;
                    textVerticalOffset = (textVerticalOffset > 0) ? textVerticalOffset / 2 : 0;

                    textRectangle.Height = Math.Min(textRectangle.Height, this.maxTextHeight);
                    textRectangle.Y += textVerticalOffset;

                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    string descriptionString = this.enabledItems[i].Text;
                    descriptionString = descriptionString.Replace("&", "");
                    ActivityDesignerPaint.DrawText(graphics, Font, descriptionString, textRectangle, StringAlignment.Near, TextQuality.Aliased, SystemBrushes.ControlText);
                    //graphics.DrawRectangle(Pens.DarkBlue, textRectangle);//uncomment for debugging purposes
                }
            }

            private void DestroyShadow()
            {
                if (this.paletteShadow != null)
                {
                    this.paletteShadow.Close();
                    this.paletteShadow.Dispose();
                    this.paletteShadow = null;
                }
            }

            private void PreparePalette(Point location)
            {
                LayoutPalette();

                //*****Adjust the location before setting it
                Point point = location;
                Rectangle bounds = this.formRectangle;
                bounds.Offset(point);
                Size fittingOffset = Size.Empty;

                //Also make sure that we add the drop shadow offset
                bounds.Width += DropShadowWidth;
                bounds.Height += DropShadowWidth;

                //Check if we are outside screen
                Rectangle screenRect = Rectangle.Empty;
                foreach (Screen screen in Screen.AllScreens)
                {
                    screenRect = Rectangle.Union(screenRect, screen.Bounds);
                }

                if (this.workingRectangle.Top > bounds.Top)
                    fittingOffset.Height += this.workingRectangle.Top - bounds.Top;
                else if (this.workingRectangle.Bottom < bounds.Bottom)
                    fittingOffset.Height -= bounds.Bottom - this.workingRectangle.Bottom;

                if (this.workingRectangle.Left > bounds.Left)
                    fittingOffset.Width += this.workingRectangle.Left - bounds.Left;
                else if (this.workingRectangle.Right < bounds.Right)
                    fittingOffset.Width -= bounds.Right - this.workingRectangle.Right;

                point += fittingOffset;
                Location = point;

                //Create the region for the window and set it
                GraphicsPath graphicsPath = new GraphicsPath();
                graphicsPath.AddRectangle(this.formRectangle);

                base.Size = this.formRectangle.Size;
                base.Region = new Region(graphicsPath);
            }

            private void LayoutPalette()
            {
                this.itemRectangles.Clear();

                this.leftGradientRectangle = Rectangle.Empty;

                //Take into account the max description size
                using (Graphics paletteGraphics = CreateGraphics())
                {
                    Size maxTextSize = Size.Empty;

                    foreach (ItemInfo itemInfo in this.enabledItems)
                    {
                        SizeF size = paletteGraphics.MeasureString(itemInfo.Text, Font);
                        maxTextSize.Width = Math.Max(Convert.ToInt32(Math.Ceiling(size.Width)), maxTextSize.Width);
                        maxTextSize.Height = Math.Max(Convert.ToInt32(Math.Ceiling(size.Height)), maxTextSize.Height);
                    }

                    maxTextSize.Width = Math.Min(maxTextSize.Width, this.maximumTextWidth);
                    this.maxTextHeight = maxTextSize.Height;
                    this.itemHeight = Math.Max(imageRectangle.Height, maxTextSize.Height + 2) + 3;
                    this.itemWidth = this.imageRectangle.Width + 2 * this.selectionItemMargin.Width + this.leftTextMargin + maxTextSize.Width + this.rightTextMargin;
                }

                int yOffset = 2; //there is a 1 pixel white space between items and the outer form border
                foreach (ItemInfo itemInfo in this.enabledItems)
                {
                    this.itemRectangles.Add(new Rectangle(2, yOffset, this.itemWidth, this.itemHeight));
                    yOffset += this.itemHeight + 2 * this.selectionItemMargin.Height;
                }

                this.leftGradientRectangle = new Rectangle(2, 2, 24, yOffset - 4);
                this.formRectangle = new Rectangle(0, 0, this.itemWidth + 4, yOffset);
            }

            private ItemInfo ActiveItem
            {
                get { return (this.activeIndex > -1) ? this.enabledItems[this.activeIndex] : null; }
            }

            private void SetActiveItem(int index)
            {
                if (this.activeIndex == index)
                    return;

                if (this.activeIndex != -1)
                    Invalidate(GetItemBounds(this.activeIndex));

                this.activeIndex = index;

                if (this.activeIndex != -1)
                    Invalidate(GetItemBounds(this.activeIndex));
            }

            private Rectangle GetItemBounds(int index)
            {
                if (index < 0 || index >= this.itemRectangles.Count)
                    return Rectangle.Empty;

                return this.itemRectangles[index];
            }

            #region Class PaletteShadow
            private sealed class PaletteShadow : Form
            {
                private Palette parent = null;

                public PaletteShadow(Palette parent)
                {
                    this.parent = parent;

                    SetStyle(ControlStyles.OptimizedDoubleBuffer |
                                ControlStyles.UserPaint |
                                ControlStyles.SupportsTransparentBackColor |
                                ControlStyles.AllPaintingInWmPaint, true);

                    FormBorderStyle = FormBorderStyle.None;
                    BackColor = Color.White;
                    ShowInTaskbar = false;
                    MaximizeBox = false;
                    ControlBox = false;
                    Opacity = 0.5d;
                    StartPosition = FormStartPosition.Manual;
                    Enabled = false;

                    Region = parent.Region;
                    Location = new Point(this.parent.Location.X + Palette.DropShadowWidth, this.parent.Location.Y + Palette.DropShadowWidth);
                }

                protected override void OnPaint(PaintEventArgs e)
                {
                    base.OnPaint(e);

                    Rectangle rectangle = this.parent.formRectangle;
                    rectangle.Offset(-Palette.DropShadowWidth, -Palette.DropShadowWidth);
                    ActivityDesignerPaint.DrawDropShadow(e.Graphics, rectangle, Color.Black, AmbientTheme.DropShadowWidth, LightSourcePosition.Left | LightSourcePosition.Top, 0.2f, false);
                }
            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region ScrollableTabControl and Related Classes

    #region TabControl Class
    [ToolboxItem(false)]
    internal sealed class TabControl : Control
    {
        #region Members and Constructor
        private const int SplitterSize = 6;
        private TabStrip tabStrip;
        private ScrollBar scrollBar;
        private AnchorAlignment stripAnchor;
        private bool allowDockChange = true;
        private Splitter splitter;
        private EventHandler idleHandler;
        private bool itemsMinimized = true;

        public TabControl(DockStyle dockStyle, AnchorAlignment stripAnchor)
        {
            if (dockStyle == DockStyle.Fill || dockStyle == DockStyle.None)
                throw new ArgumentException(DR.GetString(DR.InvalidDockingStyle, "dockStyle"));

            this.SuspendLayout();

            this.stripAnchor = stripAnchor;
            Dock = dockStyle;
            this.allowDockChange = false;

            if (Dock == DockStyle.Left || Dock == DockStyle.Right)
            {
                Width = SystemInformation.VerticalScrollBarWidth + 2;

                this.splitter = new Splitter();
                this.tabStrip = new TabStrip(Orientation.Vertical, SystemInformation.VerticalScrollBarWidth);
                this.scrollBar = new VScrollBar();

                if (this.stripAnchor == AnchorAlignment.Near)
                {
                    this.tabStrip.Dock = DockStyle.Top;
                    this.splitter.Dock = DockStyle.Top;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
                else
                {
                    this.tabStrip.Dock = DockStyle.Bottom;
                    this.splitter.Dock = DockStyle.Bottom;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
            }
            else
            //Top, Bottom
            {
                Height = SystemInformation.HorizontalScrollBarHeight + 2;

                this.splitter = new Splitter();
                this.tabStrip = new TabStrip(Orientation.Horizontal, SystemInformation.HorizontalScrollBarHeight);
                this.scrollBar = new HScrollBar();

                if (this.stripAnchor == AnchorAlignment.Near)
                {
                    this.tabStrip.Dock = DockStyle.Left;
                    this.splitter.Dock = DockStyle.Left;
                    this.scrollBar.Dock = DockStyle.Fill;

                }
                else
                {
                    this.tabStrip.Dock = DockStyle.Right;
                    this.splitter.Dock = DockStyle.Right;
                    this.scrollBar.Dock = DockStyle.Fill;
                }
            }

            Controls.AddRange(new Control[] { this.scrollBar, this.splitter, this.tabStrip });

            this.splitter.Size = new Size(SplitterSize, SplitterSize);
            this.splitter.Paint += new PaintEventHandler(OnSplitterPaint);
            this.splitter.DoubleClick += new EventHandler(OnSplitterDoubleClick);
            ((ItemList<ItemInfo>)this.TabStrip.Tabs).ListChanged += new ItemListChangeEventHandler<ItemInfo>(OnTabsChanged);

            BackColor = SystemColors.Control;
            this.ResumeLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (this.idleHandler != null)
            {
                Application.Idle -= this.idleHandler;
                this.idleHandler = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Public Functions
        public ScrollBar ScrollBar
        {
            get
            {
                return this.scrollBar;
            }
        }

        public TabStrip TabStrip
        {
            get
            {
                return this.tabStrip;
            }
        }
        #endregion

        #region Protected Members and Overrides
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            bool updateSplitterPosition = (this.splitter.SplitPosition < this.splitter.MinSize);
            if (this.splitter.Dock == DockStyle.Left || this.splitter.Dock == DockStyle.Right)
            {
                int minExtra = Math.Max(this.splitter.MinSize, Width - this.tabStrip.MaximumRequiredSize - this.splitter.Width);
                if (this.splitter.MinExtra != minExtra)
                    this.splitter.MinExtra = minExtra;
                updateSplitterPosition |= (this.itemsMinimized) ? /*minimized*/(this.splitter.SplitPosition != this.splitter.MinSize) : /*maximized*/(this.splitter.SplitPosition != Width - this.splitter.MinExtra);
            }
            else
            { //top bottom
                int minExtra = Math.Max(this.splitter.MinSize, Height - this.tabStrip.MaximumRequiredSize - this.splitter.Height);
                if (this.splitter.MinExtra != minExtra)
                    this.splitter.MinExtra = minExtra;
                updateSplitterPosition |= (this.itemsMinimized) ? /*minimized*/(this.splitter.SplitPosition != this.splitter.MinSize) : /*maximized*/(this.splitter.SplitPosition != Height - this.splitter.MinExtra);
            }

            if (updateSplitterPosition && this.idleHandler == null)
            {
                this.idleHandler = new EventHandler(OnIdle);
                Application.Idle += this.idleHandler;
            }
        }

        protected override void OnDockChanged(EventArgs e)
        {
            if (!this.allowDockChange)
                throw new InvalidOperationException(SR.GetString(SR.Error_ChangingDock));
        }

        private void OnIdle(object sender, EventArgs e)
        {
            Application.Idle -= this.idleHandler;
            this.idleHandler = null;

            if (this.splitter.Dock == DockStyle.Left || this.splitter.Dock == DockStyle.Right)
            {
                if (!this.itemsMinimized && this.splitter.SplitPosition != Width - this.splitter.MinExtra)
                    this.splitter.SplitPosition = Width - this.splitter.MinExtra;
            }
            else
            {
                if (!this.itemsMinimized && this.splitter.SplitPosition != Height - this.splitter.MinExtra)
                    this.splitter.SplitPosition = Height - this.splitter.MinExtra;
            }

            if (this.itemsMinimized && this.splitter.SplitPosition > this.splitter.MinSize)
                this.splitter.SplitPosition = this.splitter.MinSize;

            if (this.splitter.SplitPosition < this.splitter.MinSize)
                this.splitter.SplitPosition = this.splitter.MinSize;
        }

        private void OnSplitterDoubleClick(object sender, EventArgs e)
        {
            this.itemsMinimized = !this.itemsMinimized;

            if (!this.itemsMinimized)
                //maximized
                this.splitter.SplitPosition = ((this.splitter.Dock == DockStyle.Left || this.splitter.Dock == DockStyle.Right) ? Width : Height) - this.splitter.MinExtra;
            else
                //minimized
                this.splitter.SplitPosition = this.splitter.MinSize;
        }

        private void OnSplitterPaint(object sender, PaintEventArgs e)
        {
            Rectangle rectangle = ClientRectangle;
            if (this.splitter.Dock == DockStyle.Left || this.splitter.Dock == DockStyle.Right)
            {
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 0, 0, this.splitter.Height);
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 0, SplitterSize - 1, 0);

                e.Graphics.DrawLine(SystemPens.ControlDark, SplitterSize - 2, 0, SplitterSize - 2, this.splitter.Height - 1);
                e.Graphics.DrawLine(SystemPens.ControlDark, SplitterSize - 2, this.splitter.Height - 1, 0, this.splitter.Height - 1);

                e.Graphics.DrawLine(SystemPens.ControlText, SplitterSize - 1, 0, SplitterSize - 1, this.splitter.Height);
            }
            else
            {
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 1, this.splitter.Width, 1);
                e.Graphics.DrawLine(SystemPens.ControlLightLight, 0, 1, 0, SplitterSize - 1);

                e.Graphics.DrawLine(SystemPens.ControlDark, 0, SplitterSize - 2, this.splitter.Width, SplitterSize - 2);
                e.Graphics.DrawLine(SystemPens.ControlDark, this.splitter.Width - 1, SplitterSize - 2, this.splitter.Width - 1, 1);

                e.Graphics.DrawLine(SystemPens.ControlText, 0, SplitterSize - 1, this.splitter.Width, SplitterSize - 1);
            }
        }

        private void OnTabsChanged(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (this.splitter.Dock == DockStyle.Left || this.splitter.Dock == DockStyle.Right)
            {
                this.splitter.MinExtra = (Width - this.tabStrip.MaximumRequiredSize - this.splitter.Width);
                this.splitter.MinSize = this.tabStrip.MinimumRequiredSize;
            }
            else if (this.splitter.Dock == DockStyle.Top || this.splitter.Dock == DockStyle.Bottom)
            {
                this.splitter.MinExtra = (Height - this.tabStrip.MaximumRequiredSize - this.splitter.Height);
                this.splitter.MinSize = this.tabStrip.MinimumRequiredSize;
            }
        }
        #endregion
    }
    #endregion

    #region TabStrip Class
    #region Class TabSelectionChangeEventArgs
    internal sealed class TabSelectionChangeEventArgs : SelectionChangeEventArgs
    {
        private Rectangle selectedTabBounds = Rectangle.Empty;

        public TabSelectionChangeEventArgs(ItemInfo previousItem, ItemInfo currentItem, Rectangle selectedTabBounds)
            : base(previousItem, currentItem)
        {
            this.selectedTabBounds = selectedTabBounds;
        }

        public Rectangle SelectedTabBounds
        {
            get
            {
                return this.selectedTabBounds;
            }
        }
    }
    #endregion

    [ToolboxItem(false)]
    internal sealed class TabStrip : Control
    {
        #region Members and Constructor
        private const int MinSize = 18;
        private const int TabMargin = 1; //bitmap size is always 16x16 so to avoid scaling margin should be (MinSize - 16) / 2 = 1

        public event SelectionChangeEventHandler<TabSelectionChangeEventArgs> TabChange;

        private Orientation orientation = Orientation.Horizontal;
        private int reqTabItemSize = 0;
        private int selectedTab = -1;
        private ItemList<ItemInfo> tabItemList = null;
        private ToolTip buttonTips;

        private DrawTabItemStruct[] drawItems = null;

        public TabStrip(Orientation orientation, int tabSize)
        {
            this.SuspendLayout();

            this.orientation = orientation;
            this.reqTabItemSize = Math.Max(tabSize, TabStrip.MinSize);

            Font = new Font(Font.FontFamily, this.reqTabItemSize * 2 / 3, GraphicsUnit.Pixel);

            this.tabItemList = new ItemList<ItemInfo>(this);
            this.tabItemList.ListChanging += new ItemListChangeEventHandler<ItemInfo>(OnItemsChanging);
            this.tabItemList.ListChanged += new ItemListChangeEventHandler<ItemInfo>(OnItemsChanged);

            this.buttonTips = new ToolTip();
            this.buttonTips.ShowAlways = true;
            this.buttonTips.SetToolTip(this, string.Empty);

            BackColor = SystemColors.Control;

            SetStyle(ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.SupportsTransparentBackColor, true);

            this.ResumeLayout();

            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Microsoft.Win32.SystemEvents.UserPreferenceChanged -= new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);

            base.Dispose(disposing);
        }

        private void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            this.buttonTips.BackColor = SystemColors.Info;
            this.buttonTips.ForeColor = SystemColors.InfoText;
        }

        #endregion

        #region Public Properties and Methods
        public IList<ItemInfo> Tabs
        {
            get
            {
                return this.tabItemList;
            }
        }

        public int SelectedTab
        {
            get
            {
                return this.selectedTab;
            }

            set
            {
                if (value < 0 || value > this.tabItemList.Count)
                    return;

                ItemInfo previousTab = (this.selectedTab >= 0 && this.selectedTab < this.tabItemList.Count) ? this.tabItemList[this.selectedTab] : null;
                ItemInfo currentTab = this.tabItemList[value];

                this.selectedTab = value;
                Invalidate();

                if (TabChange != null)
                {
                    Rectangle tabItemBounds = GetTabItemRectangle(currentTab);
                    TabChange(this, new TabSelectionChangeEventArgs(previousTab, currentTab, new Rectangle(PointToScreen(tabItemBounds.Location), tabItemBounds.Size)));
                }
            }
        }

        public int MinimumRequiredSize
        {
            get
            {
                int tabstripSize = 0;
                for (int i = 0; i < this.tabItemList.Count; i++)
                    tabstripSize += TabMargin + this.reqTabItemSize;

                return tabstripSize;
            }
        }

        public int MaximumRequiredSize
        {
            get
            {
                int tabstripSize = 0;

                if (this.tabItemList.Count == this.drawItems.Length)
                {
                    for (int i = 0; i < this.tabItemList.Count; i++)
                    {
                        ItemInfo tabInfo = this.tabItemList[i];
                        int tabItemSize = 0;
                        if (tabInfo.Image != null)
                        {
                            tabItemSize += TabMargin;
                            tabItemSize += this.reqTabItemSize;
                        }

                        if (tabInfo.Text != null && tabInfo.Text.Length > 0)
                        {
                            tabItemSize += TabMargin;
                            tabItemSize += this.drawItems[i].TextSize.Width;
                        }

                        tabItemSize += (tabItemSize == 0) ? this.reqTabItemSize : TabMargin;

                        tabstripSize += tabItemSize;
                    }
                }

                return tabstripSize;
            }
        }
        #endregion

        #region Protected Method and Overrides
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int index = 0;
            foreach (ItemInfo tabItemInfo in this.tabItemList)
            {
                Rectangle buttonRectangle = GetTabItemRectangle(tabItemInfo);
                if (buttonRectangle.Contains(new Point(e.X, e.Y)))
                {
                    SelectedTab = index;
                    break;
                }

                index += 1;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            foreach (ItemInfo tabItemInfo in this.tabItemList)
            {
                Rectangle buttonRectangle = GetTabItemRectangle(tabItemInfo);
                if (buttonRectangle.Contains(new Point(e.X, e.Y)) && tabItemInfo.Text != this.buttonTips.GetToolTip(this))
                {
                    this.buttonTips.Active = false;
                    this.buttonTips.SetToolTip(this, tabItemInfo.Text);
                    this.buttonTips.Active = true;
                    break;
                }
            }

            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.buttonTips.SetToolTip(this, string.Empty);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.drawItems.Length != this.tabItemList.Count)
                return;

            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Color hottrackColor = Color.FromArgb(255, 238, 194);
            Color selectionColor = Color.FromArgb(255, 192, 111);
            if (SystemInformation.HighContrast)
            { //invert the values
                hottrackColor = Color.FromArgb(255 - hottrackColor.R, 255 - hottrackColor.G, 255 - hottrackColor.B);
                selectionColor = Color.FromArgb(255 - selectionColor.R, 255 - selectionColor.G, 255 - selectionColor.B);
            }
            using (Brush hottrackBrush = new SolidBrush(hottrackColor))
            using (Brush selectionBrush = new SolidBrush(selectionColor))
            {
                for (int tabItemIndex = 0; tabItemIndex < this.drawItems.Length; tabItemIndex++)
                {
                    ItemInfo tabItem = this.tabItemList[tabItemIndex];
                    DrawTabItemStruct drawTabItem = this.drawItems[tabItemIndex];

                    Brush backgroundBrush = SystemBrushes.Control;

                    Rectangle tabItemRectangle = drawTabItem.TabItemRectangle;
                    if (this.selectedTab == tabItemIndex)
                    {
                        backgroundBrush = selectionBrush;
                        e.Graphics.FillRectangle(backgroundBrush, tabItemRectangle);
                        e.Graphics.DrawRectangle(SystemPens.Highlight, tabItemRectangle);
                    }
                    else
                    {
                        Point mousePosition = PointToClient(Control.MousePosition);
                        if (tabItemRectangle.Contains(mousePosition))
                        {
                            backgroundBrush = hottrackBrush;
                            e.Graphics.FillRectangle(backgroundBrush, tabItemRectangle);
                            e.Graphics.DrawRectangle(SystemPens.ControlDarkDark, tabItemRectangle);
                        }
                    }

                    Rectangle tabImageRectangle = GetTabImageRectangle(tabItem);
                    if (!tabImageRectangle.IsEmpty)
                        e.Graphics.DrawImage(tabItem.Image, tabImageRectangle);

                    Rectangle tabTextRectangle = GetTabTextRectangle(tabItem);
                    if (!tabTextRectangle.IsEmpty)
                    {
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;
                        stringFormat.Trimming = StringTrimming.EllipsisCharacter;

                        if (this.orientation == Orientation.Horizontal)
                        {
                            RectangleF tabTextRectangleF = new RectangleF(tabTextRectangle.X, tabTextRectangle.Y, tabTextRectangle.Width, tabTextRectangle.Height);
                            e.Graphics.DrawString(tabItem.Text, Font, SystemBrushes.ControlText, tabTextRectangleF, stringFormat);
                        }
                        else
                        {
                            using (Bitmap bitmap = new Bitmap(tabTextRectangle.Height, tabTextRectangle.Width, e.Graphics))
                            using (Graphics graphics = Graphics.FromImage(bitmap))
                            {
                                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                                graphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                                graphics.DrawString(this.tabItemList[tabItemIndex].Text, Font, SystemBrushes.ControlText, new Rectangle(0, 0, bitmap.Width, bitmap.Height), stringFormat);
                                bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                                e.Graphics.DrawImage(bitmap, tabTextRectangle);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            using (Graphics graphics = CreateGraphics())
            {
                this.drawItems = new DrawTabItemStruct[this.tabItemList.Count];
                int maxTotalTabItemSize = ((this.orientation == Orientation.Horizontal) ? Width : Height);
                bool iconsOnly = false;

                //when it is minimum size, dont count the text in
                if (maxTotalTabItemSize <= MinimumRequiredSize)
                    iconsOnly = true;

                //Go through all the tabs and calculate the ImageRectangle and TextRectangle
                //as if we had enough space for everything
                //if it is not the case we'll decrease lenght of every item by the same length
                int offset = 0;
                for (int i = 0; i < this.tabItemList.Count; i++)
                {
                    int tabItemSize = 0;
                    ItemInfo itemInfo = this.tabItemList[i];
                    if (itemInfo.Image != null)
                    {
                        tabItemSize += TabMargin;
                        tabItemSize += this.reqTabItemSize - 2 * TabMargin;
                    }

                    if (itemInfo.Text != null && itemInfo.Text.Length > 0)
                    {
                        SizeF sizef = graphics.MeasureString(itemInfo.Text, Font);
                        this.drawItems[i].TextSize = new Size(Convert.ToInt32(Math.Ceiling(sizef.Width)), Convert.ToInt32(Math.Ceiling(sizef.Height)));

                        if (!iconsOnly)
                            tabItemSize += this.drawItems[i].TextSize.Width + TabMargin;
                    }

                    tabItemSize += (tabItemSize == 0) ? this.reqTabItemSize : TabMargin;

                    this.drawItems[i].TabItemRectangle = Rectangle.Empty;
                    if (this.orientation == Orientation.Horizontal)
                    {
                        this.drawItems[i].TabItemRectangle.X = offset;
                        this.drawItems[i].TabItemRectangle.Y = 0;
                        this.drawItems[i].TabItemRectangle.Width = tabItemSize;
                        this.drawItems[i].TabItemRectangle.Height = this.reqTabItemSize;
                    }
                    else
                    {
                        this.drawItems[i].TabItemRectangle.X = 0;
                        this.drawItems[i].TabItemRectangle.Y = offset;
                        this.drawItems[i].TabItemRectangle.Width = this.reqTabItemSize;
                        this.drawItems[i].TabItemRectangle.Height = tabItemSize;
                    }

                    offset += tabItemSize + 1;
                }

                offset--;
                //now calculate how much space we really consumed and if we need to make items smaller
                if (offset > maxTotalTabItemSize)
                {
                    int itemSizeDecrease = (int)Math.Ceiling(((double)(offset - maxTotalTabItemSize)) / (double)Math.Max(1, this.tabItemList.Count));
                    offset = 0;

                    //make sure the last icon is not over the edge
                    DrawTabItemStruct lastItemStruct = this.drawItems[this.tabItemList.Count - 1];
                    int lastItemWidth = (this.orientation == Orientation.Horizontal) ? lastItemStruct.TabItemRectangle.Width - itemSizeDecrease : lastItemStruct.TabItemRectangle.Height - itemSizeDecrease;
                    if (lastItemWidth < this.reqTabItemSize)
                        itemSizeDecrease += (int)Math.Ceiling(((double)(this.reqTabItemSize - lastItemWidth)) / (double)Math.Max(1, this.tabItemList.Count));

                    for (int i = 0; i < this.tabItemList.Count; i++)
                    {
                        if (this.orientation == Orientation.Horizontal)
                        {
                            this.drawItems[i].TabItemRectangle.X -= offset;
                            this.drawItems[i].TabItemRectangle.Width -= itemSizeDecrease;
                            if ((i == this.tabItemList.Count - 1) && this.drawItems[i].TabItemRectangle.Width < this.reqTabItemSize)
                                this.drawItems[i].TabItemRectangle.Width = this.reqTabItemSize;
                        }
                        else
                        {
                            this.drawItems[i].TabItemRectangle.Y -= offset;
                            this.drawItems[i].TabItemRectangle.Height -= itemSizeDecrease;
                            if ((i == this.tabItemList.Count - 1) && this.drawItems[i].TabItemRectangle.Height < this.reqTabItemSize)
                                this.drawItems[i].TabItemRectangle.Height = this.reqTabItemSize;
                        }

                        offset += itemSizeDecrease;
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        private void OnItemsChanging(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Add)
            {
                foreach (ItemInfo item in e.AddedItems)
                {
                    if (this.tabItemList.Contains(item))
                        throw new ArgumentException(DR.GetString(DR.Error_TabExistsWithSameId));
                }
            }
        }

        private void OnItemsChanged(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (this.tabItemList.Count == 0)
                this.selectedTab = -1;
            else if (this.selectedTab > this.tabItemList.Count - 1)
                SelectedTab = this.tabItemList.Count - 1;

            if (Parent != null)
                Parent.PerformLayout();
        }

        private Rectangle GetTabItemRectangle(ItemInfo tabItemInfo)
        {
            int index = this.tabItemList.IndexOf(tabItemInfo);
            if (index < 0)
                throw new ArgumentException(DR.GetString(DR.ButtonInformationMissing));

            if (this.drawItems.Length == this.tabItemList.Count)
                return this.drawItems[index].TabItemRectangle;
            else
                return Rectangle.Empty;
        }

        private Rectangle GetTabImageRectangle(ItemInfo tabItemInfo)
        {
            int index = this.tabItemList.IndexOf(tabItemInfo);
            if (index < 0)
                throw new ArgumentException(DR.GetString(DR.ButtonInformationMissing));

            Rectangle imageRectangle = Rectangle.Empty;
            if (tabItemInfo.Image != null && this.drawItems.Length == this.tabItemList.Count)
            {
                imageRectangle = this.drawItems[index].TabItemRectangle;
                imageRectangle.Inflate(-TabMargin, -TabMargin);
                imageRectangle.Size = new Size(this.reqTabItemSize - 2 * TabMargin, this.reqTabItemSize - 2 * TabMargin);
            }

            return imageRectangle;
        }

        private Rectangle GetTabTextRectangle(ItemInfo tabItemInfo)
        {
            int index = this.tabItemList.IndexOf(tabItemInfo);
            if (index < 0)
                throw new ArgumentException(DR.GetString(DR.ButtonInformationMissing));

            Rectangle textRectangle = Rectangle.Empty;
            if (tabItemInfo.Text != null && this.drawItems.Length == this.tabItemList.Count)
            {
                textRectangle = this.drawItems[index].TabItemRectangle;
                textRectangle.Inflate(-TabMargin, -TabMargin);

                Rectangle imageRectangle = GetTabImageRectangle(tabItemInfo);
                if (!imageRectangle.IsEmpty)
                {
                    if (this.orientation == Orientation.Horizontal)
                    {
                        textRectangle.X += imageRectangle.Width + TabMargin;
                        textRectangle.Width -= (imageRectangle.Width + TabMargin);
                    }
                    else
                    {
                        textRectangle.Y += imageRectangle.Height + TabMargin;
                        textRectangle.Height -= (imageRectangle.Height + TabMargin);
                    }
                }

                if (textRectangle.Width <= 0 || textRectangle.Height <= 0)
                    textRectangle = Rectangle.Empty;
            }

            return textRectangle;
        }
        #endregion

        #region Struct DrawItemStruct
        private struct DrawTabItemStruct
        {
            public Rectangle TabItemRectangle;
            public Size TextSize;
        }
        #endregion
    }
    #endregion

    #endregion

    #region Class WorkflowToolTip
    internal sealed class WorkflowToolTip : IDisposable
    {
        private Control parentControl;
        private NativeToolTip infoTip;
        private NativeToolTip inplaceTip;

        private string infoTipTitle = String.Empty;
        private string infoTipText = String.Empty;

        private string inplaceTipText = String.Empty;
        private Rectangle inplaceTipRectangle;

        internal WorkflowToolTip(Control parentControl)
        {
            this.parentControl = parentControl;

            this.infoTip = new NativeToolTip(this.parentControl.Handle);
            this.infoTip.SetDelay(NativeMethods.TTDT_INITIAL, 1000);
            this.infoTip.SetDelay(NativeMethods.TTDT_RESHOW, 1000);
            this.infoTip.SetDelay(NativeMethods.TTDT_AUTOPOP, 1000000);

            using (Graphics graphics = this.parentControl.CreateGraphics())
            {
                SizeF textSize = graphics.MeasureString(SR.GetString(SR.ToolTipString), this.parentControl.Font);
                int width = Convert.ToInt32((Math.Ceiling(textSize.Width) / 3)) * 30;
                this.infoTip.SetMaxTipWidth(width);
            }

            this.inplaceTip = new NativeToolTip(this.parentControl.Handle);
            this.inplaceTip.SetDelay(NativeMethods.TTDT_INITIAL, 50);
            this.inplaceTip.SetDelay(NativeMethods.TTDT_RESHOW, 50);
            this.inplaceTip.SetDelay(NativeMethods.TTDT_AUTOPOP, 1000000);

            this.parentControl.Layout += new LayoutEventHandler(OnParentLayoutChanged);
        }

        void IDisposable.Dispose()
        {
            if (this.parentControl != null)
            {
                if (this.infoTip != null)
                {
                    ((IDisposable)this.infoTip).Dispose();
                    this.infoTip = null;
                }

                if (this.inplaceTip != null)
                {
                    ((IDisposable)this.inplaceTip).Dispose();
                    this.inplaceTip = null;
                }

                this.parentControl.Layout -= new LayoutEventHandler(OnParentLayoutChanged);
                this.parentControl = null;
            }
        }

        public void SetText(string title, string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                this.infoTip.Pop();
                this.infoTip.Activate(false);
            }
            else
            {
                this.inplaceTip.Activate(false);
                this.infoTip.Activate(true);
            }

            bool needsUpdated = (this.infoTipTitle != title);
            needsUpdated |= (this.infoTipText != text);
            if (needsUpdated)
            {
                if (NativeMethods.IsWindowVisible(this.infoTip.Handle))
                    this.infoTip.Pop();

                this.infoTipTitle = title;
                this.infoTip.UpdateTitle(this.infoTipTitle);

                this.infoTipText = text;
                this.infoTip.UpdateToolTipText(this.infoTipText);
            }
        }

        public void SetText(string text, Rectangle rectangle)
        {
            if (String.IsNullOrEmpty(text))
            {
                this.inplaceTip.Pop();
                this.inplaceTip.Activate(false);
            }
            else
            {
                this.infoTip.Activate(false);
                this.inplaceTip.Activate(true);
            }

            bool needsUpdated = (this.inplaceTipText != text);
            needsUpdated |= (this.inplaceTipRectangle != rectangle);
            if (needsUpdated)
            {
                if (NativeMethods.IsWindowVisible(this.inplaceTip.Handle))
                    this.inplaceTip.Pop();

                this.inplaceTipText = text;
                this.inplaceTip.UpdateToolTipText(this.inplaceTipText);
                this.inplaceTipRectangle = rectangle;
            }
        }

        public void RelayParentNotify(ref System.Windows.Forms.Message msg)
        {
            if (msg.Msg == NativeMethods.WM_NOTIFY && msg.LParam != IntPtr.Zero && !this.inplaceTipRectangle.IsEmpty)
            {
                NativeMethods.NMHDR notifyHeader = Marshal.PtrToStructure(msg.LParam, typeof(NativeMethods.NMHDR)) as NativeMethods.NMHDR;
                if (notifyHeader != null && notifyHeader.hwndFrom == this.inplaceTip.Handle && notifyHeader.code == NativeMethods.TTN_SHOW)
                {
                    Point screenCoOrd = this.parentControl.PointToScreen(new Point(this.inplaceTipRectangle.Left, this.inplaceTipRectangle.Top));
                    int result = NativeMethods.SetWindowPos(this.inplaceTip.Handle, IntPtr.Zero, screenCoOrd.X, screenCoOrd.Y, 0, 0, NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
                    msg.Result = new IntPtr(1);
                }
            }
        }

        private void OnParentLayoutChanged(object sender, LayoutEventArgs e)
        {
            this.infoTip.UpdateToolTipRectangle(this.parentControl.ClientRectangle);
            this.inplaceTip.UpdateToolTipRectangle(this.parentControl.ClientRectangle);
        }

        #region Class NativeToolTip
        private sealed class NativeToolTip : NativeWindow, IDisposable
        {
            private const string ToolTipClass = "tooltips_class32";
            private IntPtr parentHandle;
            private bool activate = true;

            internal NativeToolTip(IntPtr parentHandle)
            {
                this.parentHandle = parentHandle;

                CreateParams createParams = new CreateParams();
                createParams.ClassName = NativeToolTip.ToolTipClass;
                createParams.Style = NativeMethods.WS_POPUP | NativeMethods.TTS_ALWAYSTIP | NativeMethods.TTS_NOPREFIX;
                createParams.ExStyle = NativeMethods.WS_EX_TOPMOST;
                createParams.Parent = this.parentHandle;
                CreateHandle(createParams);
                if (IntPtr.Zero == Handle)
                    throw new NullReferenceException(SR.GetString(SR.Error_CreatingToolTip));

                NativeMethods.TOOLINFO toolInfo = GetToolInfo();
                toolInfo.flags = NativeMethods.TTF_TRANSPARENT | NativeMethods.TTF_SUBCLASS;
                toolInfo.hwnd = this.parentHandle;
                AddTool(toolInfo);
                Activate(false);
            }

            void IDisposable.Dispose()
            {
                if (this.parentHandle != IntPtr.Zero)
                {
                    NativeMethods.TOOLINFO toolInfo = GetToolInfo();
                    toolInfo.hwnd = this.parentHandle;
                    DelTool(toolInfo);
                    DestroyHandle();
                    this.parentHandle = IntPtr.Zero;
                }
            }

            public void Activate(bool activateToolTip)
            {
                if (this.activate != activateToolTip)
                {
                    this.activate = activateToolTip;
                    IntPtr activateValue = (this.activate) ? new IntPtr(1) : new IntPtr(0);
                    IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_ACTIVATE, activateValue, IntPtr.Zero);
                }
            }

            public void Pop()
            {
                IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_POP, IntPtr.Zero, IntPtr.Zero);
            }

            public void SetMaxTipWidth(int tipWidth)
            {
                IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_SETMAXTIPWIDTH, IntPtr.Zero, new IntPtr(tipWidth));
            }

            public void SetDelay(int time, int delay)
            {
                IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_SETDELAYTIME, new IntPtr(time), new IntPtr(delay));
            }

            public void UpdateTitle(string title)
            {
                IntPtr titleStr = IntPtr.Zero;
                try
                {
                    titleStr = Marshal.StringToBSTR(title);
                    IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_SETTITLE, new IntPtr((int)ToolTipIcon.None), titleStr);
                }
                finally
                {
                    Marshal.FreeBSTR(titleStr);
                }
            }

            public void UpdateToolTipText(string toolTipText)
            {
                NativeMethods.TOOLINFO toolInfo = GetToolInfo();
                toolInfo.hwnd = this.parentHandle;
                try
                {
                    toolInfo.text = Marshal.StringToBSTR(toolTipText);
                    IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_UPDATETIPTEXT, IntPtr.Zero, ref toolInfo);
                }
                finally
                {
                    Marshal.FreeBSTR(toolInfo.text);
                }
            }

            public void UpdateToolTipRectangle(Rectangle rectangle)
            {
                NativeMethods.TOOLINFO toolInfo = GetToolInfo();
                toolInfo.hwnd = this.parentHandle;
                toolInfo.rect.left = rectangle.Left;
                toolInfo.rect.top = rectangle.Top;
                toolInfo.rect.right = rectangle.Right;
                toolInfo.rect.bottom = rectangle.Bottom;
                IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_NEWTOOLRECT, IntPtr.Zero, ref toolInfo);
            }

            private bool AddTool(NativeMethods.TOOLINFO toolInfo)
            {
                IntPtr retVal = NativeMethods.SendMessage(Handle, NativeMethods.TTM_ADDTOOL, IntPtr.Zero, ref toolInfo);
                return (retVal != IntPtr.Zero);
            }

            private void DelTool(NativeMethods.TOOLINFO toolInfo)
            {
                IntPtr lresult = NativeMethods.SendMessage(Handle, NativeMethods.TTM_DELTOOL, IntPtr.Zero, ref toolInfo);
            }

            private NativeMethods.TOOLINFO GetToolInfo()
            {
                NativeMethods.TOOLINFO toolInfo = new NativeMethods.TOOLINFO();
                toolInfo.size = Marshal.SizeOf(toolInfo);
                toolInfo.flags = 0;
                toolInfo.hwnd = IntPtr.Zero;
                toolInfo.id = IntPtr.Zero;
                toolInfo.rect.left = toolInfo.rect.right = toolInfo.rect.top = toolInfo.rect.bottom = 0;
                toolInfo.hinst = IntPtr.Zero;
                toolInfo.text = new IntPtr(-1);
                toolInfo.lParam = IntPtr.Zero;
                return toolInfo;
            }
        }
        #endregion
    }
    #endregion

    #endregion

    #region Theme Enabled Controls

    #region Class PreviewItemStrip
    //
    internal sealed class PreviewItemStrip
    {
        #region Members and Constuctor
        private ActivityPreviewDesigner parentDesigner = null;
        private ItemList<ItemInfo> items = null;

        private ScrollButton activeScrollButton = ScrollButton.Min;
        private string helpText = String.Empty;
        private int scrollMarker = 0;
        private int activeDropTarget = -1;

        private Rectangle bounds = Rectangle.Empty;
        private ItemInfo activeItem = null;
        public event SelectionChangeEventHandler<SelectionChangeEventArgs> SelectionChanged;

        private List<ItemStripAccessibleObject> accessibilityObjects = null;

        public PreviewItemStrip(ActivityPreviewDesigner parentDesigner)
        {
            Debug.Assert(parentDesigner != null);
            if (parentDesigner == null)
                throw new ArgumentNullException("parentDesigner");

            this.parentDesigner = parentDesigner;
            this.items = new ItemList<ItemInfo>(this);
            this.items.ListChanging += new ItemListChangeEventHandler<ItemInfo>(OnItemsChanging);
            this.items.ListChanged += new ItemListChangeEventHandler<ItemInfo>(OnItemsChanged);
        }
        #endregion

        #region Public Properties
        public IList<ItemInfo> Items
        {
            get
            {
                return this.items;
            }
        }

        public AccessibleObject[] AccessibilityObjects
        {
            get
            {
                if (this.accessibilityObjects == null)
                {
                    this.accessibilityObjects = new List<ItemStripAccessibleObject>();
                    this.accessibilityObjects.Add(new ItemStripAccessibleObject(ItemStripAccessibleObject.AccessibleObjectType.LeftScroll, this));
                    for (int i = 0; (i < VisibleItemCount) && ((this.scrollMarker + i) < Items.Count); i++)
                        this.accessibilityObjects.Add(new ItemStripAccessibleObject(ItemStripAccessibleObject.AccessibleObjectType.Item, this, i));
                    this.accessibilityObjects.Add(new ItemStripAccessibleObject(ItemStripAccessibleObject.AccessibleObjectType.RightScroll, this));
                }
                return accessibilityObjects.ToArray();
            }
        }

        public ItemInfo ActiveItem
        {
            get
            {
                return this.activeItem;
            }

            set
            {
                if (this.activeItem == value)
                    return;

                ItemInfo previousSelection = this.activeItem;
                this.activeItem = value;

                EnsureScrollMarker();

                if (SelectionChanged != null)
                    SelectionChanged(this, new SelectionChangeEventArgs(previousSelection, this.activeItem));
            }
        }

        public int ActiveDropTarget
        {
            get
            {
                return this.activeDropTarget;
            }

            set
            {
                if (this.activeDropTarget == value)
                    return;

                this.activeDropTarget = value;
                Invalidate();
            }
        }

        public string HelpText
        {
            get
            {
                return this.helpText;
            }

            set
            {
                this.helpText = value;
                if (this.items.Count == 0 && this.parentDesigner.Activity != null)
                    Invalidate();
            }
        }

        public Rectangle[] DropTargets
        {
            get
            {
                Size itemMargin = ItemMargin;
                Size itemSize = ItemSize;

                Rectangle stripRectangle = StripRectangle;
                Rectangle[] rectangles = new Rectangle[this.items.Count + 1];

                int j = 0;
                int maxItems = Math.Min(this.items.Count - this.scrollMarker, VisibleItemCount) + 1;
                for (int i = 0; i < maxItems; i++)
                {
                    j = i + this.scrollMarker;
                    rectangles[j].X = stripRectangle.Left + (i * (itemSize.Width + itemMargin.Width));
                    rectangles[j].Y = stripRectangle.Top + itemMargin.Height / 2;
                    rectangles[j].Size = new Size(itemMargin.Width, itemSize.Height + itemMargin.Height);
                }

                //Make sure that final drop target occupies the entire empty area on the right
                rectangles[j] = new Rectangle(rectangles[j].Left, rectangles[j].Top, stripRectangle.Right - rectangles[j].Left, rectangles[j].Height);
                return rectangles;
            }
        }

        public Rectangle GetItemBounds(ItemInfo itemInfo)
        {
            int itemIndex = this.items.IndexOf(itemInfo);
            if (itemIndex < 0)
                return Rectangle.Empty;

            if (itemIndex < this.scrollMarker || itemIndex >= this.scrollMarker + VisibleItemCount)
                return Rectangle.Empty;

            Rectangle stripRectangle = StripRectangle;
            Rectangle itemRectangle = Rectangle.Empty;

            Size itemMargin = ItemMargin;
            Size itemSize = ItemSize;

            itemIndex = itemIndex - this.scrollMarker;
            itemRectangle.X = stripRectangle.Left + (itemIndex * itemSize.Width) + ((itemIndex + 1) * itemMargin.Width);
            itemRectangle.Y = stripRectangle.Top + itemMargin.Height;
            itemRectangle.Size = itemSize;
            return itemRectangle;
        }
        #endregion

        #region Members similar to ActivityDesigner.
        public Point Location
        {
            get
            {
                return this.bounds.Location;
            }

            set
            {
                if (this.bounds.Location == value)
                    return;
                this.bounds.Location = value;
            }
        }

        public Size Size
        {
            get
            {
                return this.bounds.Size;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public ItemInfo HitTest(Point point)
        {
            //Go thru the buttons and if any of them is selected then return appropriate HitTestData
            for (int itemIndex = this.scrollMarker; itemIndex < this.items.Count; itemIndex++)
            {
                if (GetItemBounds(this.items[itemIndex]).Contains(point))
                    return this.items[itemIndex];
            }

            return null;
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            Point point = new Point(e.X, e.Y);

            int incr = 0;
            if (GetButtonBounds(ScrollButton.Left).Contains(point))
            {
                ActiveScrollButton = ScrollButton.Left;
                incr = -1;
            }
            else if (GetButtonBounds(ScrollButton.Right).Contains(point))
            {
                ActiveScrollButton = ScrollButton.Right;
                incr = 1;
            }

            if (incr != 0)
            {
                if (ActiveItem != null)
                {
                    int index = this.items.IndexOf(ActiveItem) + incr;
                    index = (index >= this.items.Count) ? 0 : (index < 0) ? this.items.Count - 1 : index;
                    ActiveItem = this.items[index];
                }
            }
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            ActiveScrollButton = ScrollButton.Min;
        }

        public void OnMouseLeave()
        {
            ActiveScrollButton = ScrollButton.Min;
        }

        public void OnLayoutSize(Graphics graphics)
        {
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;

            //Be sure to call this atleast once
            Size itemMargin = ItemMargin;
            Size itemSize = ItemSize;
            this.bounds.Width = 2 * WorkflowTheme.CurrentTheme.AmbientTheme.Margin.Width;
            this.bounds.Width += (itemSize.Width * ((designerTheme != null) ? designerTheme.PreviewItemCount : 0));
            this.bounds.Width += (itemMargin.Width * (((designerTheme != null) ? designerTheme.PreviewItemCount : 0) + 1));
            this.bounds.Width += GetButtonBounds(ScrollButton.Left).Size.Width;
            this.bounds.Width += GetButtonBounds(ScrollButton.Right).Size.Width;
            this.bounds.Height = itemSize.Height + (2 * itemMargin.Height);

            EnsureScrollMarker();
        }

        public void Draw(Graphics graphics)
        {
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            if (designerTheme != null)
            {
                //First draw the strip
                Rectangle stripRectangle = StripRectangle;
                GraphicsPath stripPath = new GraphicsPath();
                if (designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle)
                    stripPath.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(stripRectangle, 4), false);
                else
                    stripPath.AddRectangle(stripRectangle);
                stripPath.CloseFigure();

                graphics.FillPath(designerTheme.PreviewBackgroundBrush, stripPath);
                graphics.DrawPath(designerTheme.PreviewBorderPen, stripPath);
                stripPath.Dispose();

                //Now draw the images for scroll buttons
                Image scrollButtonImage = ActivityPreviewDesignerTheme.LeftScrollImageUp;
                Rectangle scrollbuttonRectangle = GetButtonBounds(ScrollButton.Left);
                if (ActiveScrollButton == ScrollButton.Left)
                {
                    scrollButtonImage = ActivityPreviewDesignerTheme.LeftScrollImage;
                    scrollbuttonRectangle.Offset(1, 1);
                }
                if (scrollButtonImage != null)
                    ActivityDesignerPaint.DrawImage(graphics, scrollButtonImage, scrollbuttonRectangle, DesignerContentAlignment.Center);

                scrollButtonImage = ActivityPreviewDesignerTheme.RightScrollImageUp;
                scrollbuttonRectangle = GetButtonBounds(ScrollButton.Right);
                if (ActiveScrollButton == ScrollButton.Right)
                {
                    scrollButtonImage = ActivityPreviewDesignerTheme.RightScrollImage;
                    scrollbuttonRectangle.Offset(1, 1);
                }
                if (scrollButtonImage != null)
                    ActivityDesignerPaint.DrawImage(graphics, scrollButtonImage, scrollbuttonRectangle, DesignerContentAlignment.Center);

                //Draw previwed designers
                Size itemMargin = ItemMargin;
                int selectionSize = Math.Max(Math.Min(itemMargin.Width / 4, itemMargin.Height / 2), 1);
                for (int itemIndex = this.scrollMarker; itemIndex < this.items.Count && itemIndex < (this.scrollMarker + VisibleItemCount); itemIndex++)
                {
                    Rectangle itemRectangle = GetItemBounds(this.items[itemIndex]);
                    if (itemRectangle.IsEmpty)
                        continue;

                    GraphicsPath itemPath = new GraphicsPath();
                    if (designerTheme.DesignerGeometry == DesignerGeometry.RoundedRectangle)
                        itemPath.AddPath(ActivityDesignerPaint.GetRoundedRectanglePath(itemRectangle, 4), true);
                    else
                        itemPath.AddRectangle(itemRectangle);

                    graphics.FillPath(designerTheme.PreviewForegroundBrush, itemPath);
                    graphics.DrawPath(designerTheme.PreviewBorderPen, itemPath);
                    itemPath.Dispose();

                    Image itemImage = this.items[itemIndex].Image;
                    if (itemImage == null)
                    {
                        Activity activity = this.items[itemIndex].UserData[DesignerUserDataKeys.Activity] as Activity;
                        ActivityDesigner activityDesigner = ActivityDesigner.GetDesigner(activity);
                        if (activityDesigner != null)
                            itemImage = activityDesigner.Image;
                    }

                    if (itemImage != null)
                    {
                        Rectangle imageRectangle = Rectangle.Empty;
                        imageRectangle.X = itemRectangle.Left + 2;
                        imageRectangle.Y = itemRectangle.Top + 2;
                        imageRectangle.Size = new Size(itemRectangle.Width - 4, itemRectangle.Height - 4);
                        ActivityDesignerPaint.DrawImage(graphics, itemImage, imageRectangle, DesignerContentAlignment.Center);
                    }

                    if (itemIndex == this.items.IndexOf(ActiveItem))
                    {
                        itemRectangle.Inflate(selectionSize, selectionSize);
                        graphics.DrawRectangle(ambientTheme.SelectionForegroundPen, itemRectangle);
                    }
                }

                //Draw the drop target
                Rectangle[] dropTargets = DropTargets;
                int activeDropTarget = ActiveDropTarget;
                if (activeDropTarget >= 0 && activeDropTarget < dropTargets.GetLength(0))
                {
                    dropTargets[activeDropTarget].Width = itemMargin.Width;
                    graphics.DrawLine(ambientTheme.DropIndicatorPen, dropTargets[activeDropTarget].Left + dropTargets[activeDropTarget].Width / 2, dropTargets[activeDropTarget].Top, dropTargets[activeDropTarget].Left + dropTargets[activeDropTarget].Width / 2, dropTargets[activeDropTarget].Bottom);
                }
                else if (this.items.Count == 0 && this.helpText.Length > 0)
                {
                    stripRectangle.Inflate(-2, -2);

                    Brush textBrush = (ActiveDropTarget != -1) ? ambientTheme.DropIndicatorBrush : designerTheme.ForegroundBrush;
                    ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, this.helpText, stripRectangle, StringAlignment.Center, WorkflowTheme.CurrentTheme.AmbientTheme.TextQuality, textBrush);
                }
            }
        }
        #endregion

        #region Helpers
        private void EnsureScrollMarker()
        {
            if (ActiveItem == null || VisibleItemCount == 0)
                return;

            int newMarker = -1;

            //If marker falls outside the range then ensure it to a visible point
            int index = this.items.IndexOf(ActiveItem);
            if (index >= 0)
                newMarker = (index < this.scrollMarker) ? index : (index >= this.scrollMarker + VisibleItemCount) ? index - VisibleItemCount + 1 : newMarker;

            //If there are more items in the strip than displayed then we need to display what ever we can
            if (this.items.Count >= VisibleItemCount && ((this.items.Count - this.scrollMarker) < VisibleItemCount))
                newMarker = this.items.Count - VisibleItemCount;

            if (newMarker >= 0 && newMarker <= Math.Max(this.items.Count - VisibleItemCount + 1, 0))
                this.scrollMarker = newMarker;

            Invalidate();
        }

        private ScrollButton ActiveScrollButton
        {
            get
            {
                return this.activeScrollButton;
            }

            set
            {
                if (this.activeScrollButton == value)
                    return;

                this.activeScrollButton = value;

                Invalidate();
            }
        }

        private int VisibleItemCount
        {
            get
            {
                ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
                return ((designerTheme != null) ? designerTheme.PreviewItemCount : 1);
            }
        }

        private Rectangle StripRectangle
        {
            get
            {
                Rectangle stripRectangle = Rectangle.Empty;
                Rectangle scrollLeftButton = GetButtonBounds(ScrollButton.Left);
                Rectangle scrollRightButton = GetButtonBounds(ScrollButton.Right);

                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                stripRectangle.X = scrollLeftButton.Right + margin.Width;
                stripRectangle.Y = this.bounds.Y;
                stripRectangle.Width = (scrollRightButton.Left - margin.Width) - (scrollLeftButton.Right + margin.Width);
                stripRectangle.Height = this.bounds.Height;
                return stripRectangle;
            }
        }

        private Rectangle GetButtonBounds(ScrollButton scrollButton)
        {
            Image scrollButtonImage = ActivityPreviewDesignerTheme.LeftScrollImage;
            if (scrollButton == ScrollButton.Min || scrollButtonImage == null)
                return Rectangle.Empty;

            Size scrollButtonSize = scrollButtonImage.Size;
            scrollButtonSize.Height = Math.Min(scrollButtonSize.Width, Math.Min(scrollButtonSize.Height, ItemSize.Height));
            scrollButtonSize.Width = Math.Min(scrollButtonSize.Width, scrollButtonSize.Height);

            int startLocation = (scrollButton == ScrollButton.Left) ? this.bounds.X : this.bounds.Right - scrollButtonSize.Width;
            Rectangle scrollRectangle = Rectangle.Empty;
            scrollRectangle.X = startLocation;
            scrollRectangle.Y = this.bounds.Y + this.bounds.Size.Height / 2 - scrollButtonSize.Height / 2;
            scrollRectangle.Size = scrollButtonSize;
            return scrollRectangle;
        }

        private void OnItemsChanging(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Remove && e.RemovedItems.Count > 0 && ActiveItem == e.RemovedItems[0])
            {
                int nextIndex = this.items.IndexOf(e.RemovedItems[0]);
                nextIndex += (nextIndex < this.items.Count - 1) ? 1 : -1;
                ActiveItem = (nextIndex >= 0 && nextIndex < this.items.Count) ? this.items[nextIndex] : null;
            }
        }

        private void OnItemsChanged(object sender, ItemListChangeEventArgs<ItemInfo> e)
        {
            if (e.Action == ItemListChangeAction.Add && e.AddedItems.Count > 0)
                ActiveItem = e.AddedItems[0];
            if (e.Action == ItemListChangeAction.Remove)
                EnsureScrollMarker();

            this.accessibilityObjects = null;

            Invalidate();
        }

        private Size ItemSize
        {
            get
            {
                ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
                return ((designerTheme != null) ? designerTheme.PreviewItemSize : Size.Empty);
            }
        }

        private Size ItemMargin
        {
            get
            {
                Size itemSize = ItemSize;
                return new Size(itemSize.Width / 2, itemSize.Height / 4);
            }
        }

        private void Invalidate()
        {
            if (this.parentDesigner != null && parentDesigner.Activity.Site != null)
            {
                WorkflowView workflowView = parentDesigner.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                    workflowView.InvalidateLogicalRectangle(this.bounds);
            }
        }
        #endregion

        #region Class ItemStripAccessibleObject
        private sealed class ItemStripAccessibleObject : AccessibleObject
        {
            internal enum AccessibleObjectType { LeftScroll = 1, Item = 2, RightScroll = 3 }

            private AccessibleObjectType accessibleObjectType;
            private PreviewItemStrip itemStrip;
            private int itemIndex = -1;

            internal ItemStripAccessibleObject(AccessibleObjectType type, PreviewItemStrip itemStrip)
            {
                this.accessibleObjectType = type;
                this.itemStrip = itemStrip;
            }

            internal ItemStripAccessibleObject(AccessibleObjectType type, PreviewItemStrip itemStrip, int itemIndex)
            {
                this.accessibleObjectType = type;
                this.itemStrip = itemStrip;
                this.itemIndex = itemIndex;
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = Rectangle.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        bounds = this.itemStrip.GetButtonBounds(ScrollButton.Left);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        bounds = this.itemStrip.GetButtonBounds(ScrollButton.Right);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        int index = this.itemStrip.scrollMarker + this.itemIndex;
                        bounds = (index >= 0 && index < this.itemStrip.Items.Count) ? this.itemStrip.GetItemBounds(this.itemStrip.Items[index]) : Rectangle.Empty;
                    }

                    if (!bounds.IsEmpty)
                    {
                        WorkflowView workflowView = this.itemStrip.parentDesigner.Activity.Site.GetService(typeof(WorkflowView)) as WorkflowView;
                        if (workflowView != null)
                            bounds = new Rectangle(workflowView.LogicalPointToScreen(bounds.Location), workflowView.LogicalSizeToClient(bounds.Size));
                    }

                    return bounds;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return DR.GetString(DR.AccessibleAction);
                }
            }

            public override string Description
            {
                get
                {
                    string description = String.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        description = DR.GetString(DR.LeftScrollButtonAccessibleDescription);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        description = DR.GetString(DR.RightScrollButtonAccessibleDescription);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner activityDesigner = AssociatedDesigner;
                        if (activityDesigner != null)
                            description = DR.GetString(DR.ActivityDesignerAccessibleDescription, activityDesigner.Activity.GetType().Name);
                    }

                    return description;
                }
            }

            public override string Help
            {
                get
                {
                    string help = String.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        help = DR.GetString(DR.LeftScrollButtonAccessibleHelp);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        help = DR.GetString(DR.RightScrollButtonAccessibleHelp);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner activityDesigner = AssociatedDesigner;
                        if (activityDesigner != null)
                            help = DR.GetString(DR.ActivityDesignerAccessibleHelp, activityDesigner.Activity.GetType().Name);
                    }

                    return help;
                }
            }

            public override string Name
            {
                get
                {
                    string name = String.Empty;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                    {
                        name = DR.GetString(DR.LeftScrollButtonName);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                    {
                        name = DR.GetString(DR.RightScrollButtonName);
                    }
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner activityDesigner = AssociatedDesigner;
                        if (activityDesigner != null)
                        {
                            Activity activity = activityDesigner.Activity;
                            name = (activity != null) ? activity.QualifiedName : base.Name;
                        }
                    }

                    return name;
                }

                set
                {
                    //We do not allow setting ID programatically
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.itemStrip.parentDesigner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Diagram;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = AccessibleStates.None;

                    if (this.accessibleObjectType == AccessibleObjectType.Item)
                    {
                        ActivityDesigner activityDesigner = AssociatedDesigner;
                        ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                        if (selectionService != null && activityDesigner != null)
                        {
                            state = (activityDesigner.IsSelected) ? AccessibleStates.Selected : AccessibleStates.Selectable;
                            state |= AccessibleStates.MultiSelectable;
                            state |= (activityDesigner.IsLocked) ? AccessibleStates.ReadOnly : AccessibleStates.Moveable;
                            state |= (activityDesigner.IsPrimarySelection) ? AccessibleStates.Focused : AccessibleStates.Focusable;
                        }
                    }

                    return state;
                }
            }

            public override void DoDefaultAction()
            {
                if (this.accessibleObjectType == AccessibleObjectType.Item)
                {
                    ActivityDesigner activityDesigner = AssociatedDesigner;
                    if (activityDesigner != null)
                    {
                        ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                        if (selectionService != null)
                            selectionService.SetSelectedComponents(new object[] { activityDesigner.Activity }, SelectionTypes.Replace);
                    }
                }
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (navdir == AccessibleNavigation.Left || navdir == AccessibleNavigation.Right)
                {
                    AccessibleObject[] accessibleObjects = this.itemStrip.AccessibilityObjects;
                    int index = -1;
                    if (this.accessibleObjectType == AccessibleObjectType.LeftScroll)
                        index = 0;
                    else if (this.accessibleObjectType == AccessibleObjectType.RightScroll)
                        index = accessibleObjects.Length - 1;
                    else if (this.accessibleObjectType == AccessibleObjectType.Item)
                        index = this.itemIndex + ((navdir == AccessibleNavigation.Left) ? -1 : 1);

                    index = Math.Max(Math.Min(accessibleObjects.Length - 1, index), 0);
                    return accessibleObjects[index];
                }
                else if (navdir == AccessibleNavigation.Previous)
                {
                    return this.itemStrip.parentDesigner.AccessibilityObject;
                }
                else if (navdir == AccessibleNavigation.Next)
                {
                    int accessibilityObjectCount = this.itemStrip.AccessibilityObjects.Length;
                    int childCount = this.itemStrip.parentDesigner.AccessibilityObject.GetChildCount();
                    if (childCount > accessibilityObjectCount)
                        return this.itemStrip.parentDesigner.AccessibilityObject.GetChild(accessibilityObjectCount);
                    else
                        return this.itemStrip.parentDesigner.AccessibilityObject.Navigate(navdir);
                }

                return base.Navigate(navdir);
            }

            public override void Select(AccessibleSelection flags)
            {
                if (this.accessibleObjectType == AccessibleObjectType.Item)
                {
                    ISelectionService selectionService = GetService(typeof(ISelectionService)) as ISelectionService;
                    ActivityDesigner activityDesigner = AssociatedDesigner;
                    if (selectionService != null && activityDesigner != null)
                    {
                        if (((flags & AccessibleSelection.TakeFocus) > 0) || ((flags & AccessibleSelection.TakeSelection) > 0))
                            selectionService.SetSelectedComponents(new object[] { activityDesigner.Activity }, SelectionTypes.Replace);
                        else if ((flags & AccessibleSelection.AddSelection) > 0)
                            selectionService.SetSelectedComponents(new object[] { activityDesigner.Activity }, SelectionTypes.Add);
                        else if ((flags & AccessibleSelection.RemoveSelection) > 0)
                            selectionService.SetSelectedComponents(new object[] { activityDesigner.Activity }, SelectionTypes.Remove);
                    }
                }
                else
                {
                    base.Select(flags);
                }
            }

            private ActivityDesigner AssociatedDesigner
            {
                get
                {
                    if (this.accessibleObjectType != AccessibleObjectType.Item)
                        return null;

                    int index = this.itemStrip.scrollMarker + this.itemIndex;
                    ItemInfo itemInfo = (index >= 0 && index < this.itemStrip.Items.Count) ? this.itemStrip.Items[index] : null;
                    if (itemInfo != null)
                        return ActivityDesigner.GetDesigner(itemInfo.UserData[DesignerUserDataKeys.Activity] as Activity);
                    else
                        return null;
                }
            }

            private object GetService(Type serviceType)
            {
                if (this.itemStrip.parentDesigner.Activity != null || this.itemStrip.parentDesigner.Activity.Site != null)
                    return this.itemStrip.parentDesigner.Activity.Site.GetService(serviceType);
                else
                    return null;
            }
        }
        #endregion
    }
    #endregion

    #region Class PreviewWindow
    internal sealed class PreviewWindow
    {
        #region Members and Constructor
        private ActivityPreviewDesigner parentDesigner = null;
        private IServiceProvider serviceProvider = null;
        private Activity previewedActivity = null;
        private Image previewedActivityImage = null;
        private bool previewMode = true;
        private PreviewWindowAccessibleObject accessibilityObject;
        public event System.EventHandler PreviewModeChanged;

        //Temporary members
        private Rectangle bounds = Rectangle.Empty;
        private Rectangle canvasBounds = Rectangle.Empty;
        private Rectangle previewModeButtonRectangle = Rectangle.Empty;
        private Rectangle previewModeDescRectangle = Rectangle.Empty;
        private Size previewDescTextSize = Size.Empty;

        public PreviewWindow(ActivityPreviewDesigner parent)
        {
            this.parentDesigner = parent;
            this.serviceProvider = this.parentDesigner.Activity.Site;
        }
        #endregion

        #region Public Properties
        public AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                    this.accessibilityObject = new PreviewWindowAccessibleObject(this);
                return this.accessibilityObject;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
        }

        public Point Location
        {
            get
            {
                return this.bounds.Location;
            }

            set
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                this.bounds.Location = value;

                int maxDescHeight = Math.Max(this.previewModeDescRectangle.Height, this.previewModeButtonRectangle.Height);

                Point descRectanglePos = Point.Empty;
                descRectanglePos.X = this.bounds.Left + this.bounds.Width / 2 - this.previewModeDescRectangle.Width / 2 + this.previewModeButtonRectangle.Width + margin.Width;
                descRectanglePos.Y = this.bounds.Top + maxDescHeight / 2 - this.previewModeDescRectangle.Height / 2;
                this.previewModeDescRectangle.Location = descRectanglePos;

                Point previewModeBitmapPos = Point.Empty;
                previewModeBitmapPos.X = descRectanglePos.X - (this.previewModeButtonRectangle.Width + margin.Width);
                previewModeBitmapPos.Y = this.bounds.Top + maxDescHeight / 2 - this.previewModeButtonRectangle.Height / 2;
                this.previewModeButtonRectangle.Location = previewModeBitmapPos;

                this.canvasBounds.Location = new Point(value.X + this.bounds.Width / 2 - this.canvasBounds.Width / 2, this.previewModeDescRectangle.Bottom + margin.Height);

                //Adjust the location of the activity which is previewed
                if (PreviewDesigner != null)
                {
                    Point location = Point.Empty;
                    location.X = this.canvasBounds.Left + this.canvasBounds.Width / 2 - PreviewDesigner.Size.Width / 2;
                    location.Y = this.canvasBounds.Top + this.canvasBounds.Height / 2 - PreviewDesigner.Size.Height / 2;
                    PreviewDesigner.Location = location;
                }
            }
        }

        public Size Size
        {
            get
            {
                return this.bounds.Size;
            }
        }

        public Activity PreviewedActivity
        {
            get
            {
                return this.previewedActivity;
            }

            set
            {
                if (this.previewedActivity == value)
                    return;

                this.previewedActivity = value;

                if (this.previewedActivityImage != null)
                {
                    this.previewedActivityImage.Dispose();
                    this.previewedActivityImage = null;
                }

                if (this.serviceProvider != null)
                {
                    WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                    if (workflowView != null)
                        workflowView.PerformLayout(false);
                }
            }
        }

        public void Refresh()
        {
            if (this.previewedActivityImage != null)
            {
                this.previewedActivityImage.Dispose();
                this.previewedActivityImage = null;
            }

            if (this.serviceProvider != null)
            {
                WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                if (workflowView != null)
                    workflowView.InvalidateLogicalRectangle(this.bounds);
            }
        }

        public bool PreviewMode
        {
            get
            {
                return this.previewMode;
            }

            set
            {
                if (this.previewMode == value)
                    return;

                this.previewMode = value;

                if (this.previewMode)
                {
                    EnsureValidDesignerPreview(PreviewDesigner);

                    if (this.previewedActivityImage != null)
                    {
                        this.previewedActivityImage.Dispose();
                        this.previewedActivityImage = null;
                    }
                }

                if (PreviewModeChanged != null)
                    PreviewModeChanged(this, EventArgs.Empty);

                if (this.serviceProvider != null)
                {
                    WorkflowView workflowView = this.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                    if (workflowView != null)
                        workflowView.PerformLayout(false);
                }
            }
        }
        #endregion

        #region Public Methods
        public void OnMouseDown(MouseEventArgs e)
        {
            if (PreviewModeButtonRectangle.Contains(new Point(e.X, e.Y)))
                PreviewMode = !PreviewMode;
        }

        public void OnLayoutSize(Graphics graphics, int minWidth)
        {
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            if (designerTheme == null)
                return;

            Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
            if (!PreviewMode && PreviewDesigner != null)
            {
                this.canvasBounds.Size = PreviewDesigner.Bounds.Size;
                this.canvasBounds.Inflate(margin.Width * 2, margin.Height * 2);
                this.canvasBounds.Size = new Size(Math.Max(this.canvasBounds.Width, designerTheme.PreviewWindowSize.Width), Math.Max(this.canvasBounds.Height, designerTheme.PreviewWindowSize.Height));
            }
            else
            {
                this.canvasBounds.Size = designerTheme.PreviewWindowSize;
            }

            this.canvasBounds.Width = Math.Max(this.canvasBounds.Width, minWidth);

            SizeF stringSize = graphics.MeasureString(PreviewModeDescription, designerTheme.Font);
            this.previewDescTextSize = new Size(Convert.ToInt32(Math.Ceiling(stringSize.Width)), Convert.ToInt32(Math.Ceiling(stringSize.Height)));
            this.previewDescTextSize.Width = Math.Min(this.canvasBounds.Size.Width - margin.Width - this.previewModeButtonRectangle.Size.Width, this.previewDescTextSize.Width);
            this.previewModeDescRectangle.Size = this.previewDescTextSize;

            this.previewModeButtonRectangle.Height = Math.Min(designerTheme.PreviewButtonSize.Height, this.previewDescTextSize.Height);
            this.previewModeButtonRectangle.Width = this.previewModeButtonRectangle.Size.Height;

            Size totalSize = Size.Empty;
            totalSize.Width = this.canvasBounds.Width + 2 * margin.Width;
            totalSize.Height = Math.Max(this.previewModeButtonRectangle.Size.Height, this.previewDescTextSize.Height);
            totalSize.Height += margin.Height;
            totalSize.Height += this.canvasBounds.Height;
            this.bounds.Size = totalSize;
        }

        public void Draw(Graphics graphics, Rectangle viewPort)
        {
            ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
            if (designerTheme != null)
            {
                //todo: check if can still draw something
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;

                //Draw description for preview mode
                ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, PreviewModeDescription, this.previewModeDescRectangle, StringAlignment.Center, WorkflowTheme.CurrentTheme.AmbientTheme.TextQuality, designerTheme.ForegroundBrush);

                //Draw the button
                graphics.DrawRectangle(Pens.Black, this.previewModeButtonRectangle.Left - 1, this.previewModeButtonRectangle.Top - 1, this.previewModeButtonRectangle.Width + 1, this.previewModeButtonRectangle.Height + 1);
                ActivityDesignerPaint.Draw3DButton(graphics, null, this.previewModeButtonRectangle, 1.0f, (!PreviewMode) ? ButtonState.Pushed : ButtonState.Normal);

                Image previewModeImage = (PreviewMode) ? ActivityPreviewDesignerTheme.PreviewButtonImage : ActivityPreviewDesignerTheme.EditButtonImage;
                ActivityDesignerPaint.DrawImage(graphics, previewModeImage, new Rectangle(this.previewModeButtonRectangle.Left + 2, this.previewModeButtonRectangle.Top + 2, this.previewModeButtonRectangle.Width - 4, this.previewModeButtonRectangle.Height - 4), DesignerContentAlignment.Center);

                graphics.FillRectangle(designerTheme.PreviewBackgroundBrush, this.canvasBounds);
                if (PreviewMode)
                {
                    graphics.DrawRectangle(designerTheme.PreviewBorderPen, this.canvasBounds);
                }
                else
                {
                    Rectangle canvasRect = this.canvasBounds;
                    canvasRect.Inflate(2, 2);
                    graphics.DrawRectangle(SystemPens.ControlDark, canvasRect);
                    canvasRect.Inflate(-1, -1);
                    graphics.DrawLine(SystemPens.ControlDarkDark, canvasRect.Left, canvasRect.Top, canvasRect.Left, canvasRect.Bottom);
                    graphics.DrawLine(SystemPens.ControlDarkDark, canvasRect.Left, canvasRect.Top, canvasRect.Right, canvasRect.Top);
                    graphics.DrawLine(SystemPens.ControlLight, canvasRect.Right, canvasRect.Top, canvasRect.Right, canvasRect.Bottom);
                    graphics.DrawLine(SystemPens.ControlLight, canvasRect.Left, canvasRect.Bottom, canvasRect.Right, canvasRect.Bottom);
                    canvasRect.Inflate(-1, -1);
                    graphics.DrawLine(SystemPens.ControlLight, canvasRect.Left, canvasRect.Top, canvasRect.Left, canvasRect.Bottom);
                    graphics.DrawLine(SystemPens.ControlLight, canvasRect.Left, canvasRect.Top, canvasRect.Right, canvasRect.Top);
                    graphics.FillRectangle(designerTheme.PreviewBackgroundBrush, canvasRect);
                }

                //Draw the helptext
                if (PreviewDesigner == null)
                {
                    Rectangle descriptionRectangle = this.canvasBounds;
                    descriptionRectangle.Inflate(-margin.Width, -margin.Height);
                    string previewDescription = DR.GetString(DR.SelectActivityDesc);
                    ActivityDesignerPaint.DrawText(graphics, designerTheme.Font, previewDescription, descriptionRectangle, StringAlignment.Center, WorkflowTheme.CurrentTheme.AmbientTheme.TextQuality, designerTheme.ForegroundBrush);
                }

                //Draw the preview
                if (PreviewMode)
                {
                    //We want to make sure that if the previewd designer is too large then we collapse it before
                    //generating the preview so that user is able to see the preview
                    Image previewedActivityImage = GeneratePreview(graphics);
                    if (previewedActivityImage != null)
                    {
                        Rectangle destnRectangle = Rectangle.Empty;

                        Size maxBitmapSize = new Size(this.canvasBounds.Width - 2 * margin.Width, this.canvasBounds.Height - 2 * margin.Height);
                        double stretchFactor = ((double)previewedActivityImage.Width) / maxBitmapSize.Width;
                        stretchFactor = Math.Max(stretchFactor, ((double)previewedActivityImage.Height) / maxBitmapSize.Height);
                        stretchFactor = Math.Max(stretchFactor, 1.3f);

                        destnRectangle.Width = Convert.ToInt32(Math.Ceiling((float)previewedActivityImage.Width / stretchFactor));
                        destnRectangle.Height = Convert.ToInt32(Math.Ceiling((float)previewedActivityImage.Height / stretchFactor));
                        destnRectangle.X = this.canvasBounds.Left + this.canvasBounds.Width / 2 - destnRectangle.Width / 2;
                        destnRectangle.Y = this.canvasBounds.Top + this.canvasBounds.Height / 2 - destnRectangle.Height / 2;

                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(previewedActivityImage, destnRectangle, new Rectangle(Point.Empty, previewedActivityImage.Size), GraphicsUnit.Pixel);
                    }

                    Rectangle indicatorBounds = this.canvasBounds;
                    indicatorBounds.Inflate(-margin.Width, -margin.Height);
                    ActivityDesignerPaint.DrawImage(graphics, ActivityPreviewDesignerTheme.PreviewImage, indicatorBounds, DesignerContentAlignment.TopLeft);
                }
                else
                {
                    if (PreviewDesigner != null)
                    {
                        Rectangle previewDesignerViewPort = PreviewDesigner.Bounds;
                        previewDesignerViewPort.Inflate(margin.Width, margin.Height);
                        using (PaintEventArgs paintEventArgs = new PaintEventArgs(graphics, previewDesignerViewPort))
                        {
                            ((IWorkflowDesignerMessageSink)PreviewDesigner).OnPaint(paintEventArgs, previewDesignerViewPort);
                        }
                    }
                }
            }
        }
        #endregion

        #region Helpers
        private ActivityDesigner PreviewDesigner
        {
            get
            {
                return ActivityDesigner.GetDesigner(this.previewedActivity);
            }
        }

        private Rectangle PreviewModeButtonRectangle
        {
            get
            {
                return this.previewModeButtonRectangle;
            }
        }

        private string PreviewModeDescription
        {
            get
            {
                string previewModeDescription = (PreviewMode) ? DR.GetString(DR.PreviewMode) : DR.GetString(DR.EditMode);

                CompositeActivity compositeActivity = (this.parentDesigner != null) ? this.parentDesigner.Activity as CompositeActivity : null;
                if (compositeActivity == null)
                    return previewModeDescription;

                IComponent component = (PreviewDesigner != null) ? PreviewDesigner.Activity : null;
                if (component == null)
                    return previewModeDescription;

                List<Activity> previewedActivities = new List<Activity>();

                foreach (Activity containedActivity in compositeActivity.Activities)
                {
                    if (!Helpers.IsAlternateFlowActivity(containedActivity))
                        previewedActivities.Add(containedActivity);
                }

                //
                int index = previewedActivities.IndexOf(component as Activity) + 1;
                previewModeDescription += " [" + index.ToString(CultureInfo.CurrentCulture) + "/" + previewedActivities.Count.ToString(CultureInfo.CurrentCulture) + "]";
                return previewModeDescription;
            }
        }

        private Image GeneratePreview(Graphics graphics)
        {
            if (this.previewedActivityImage == null)
            {
                ActivityDesigner previewDesigner = PreviewDesigner;
                if (previewDesigner != null && this.parentDesigner != null)
                    this.previewedActivityImage = previewDesigner.GetPreviewImage(graphics);
            }
            return this.previewedActivityImage;
        }

        private void EnsureValidDesignerPreview(ActivityDesigner designer)
        {
            //We introduce this logic as beyond a point we can not show the preview as the designer size can become too large to show
            //So if we go beyond a point then we always collapse the activity before rendering the preview
            CompositeActivityDesigner previewDesigner = designer as CompositeActivityDesigner;
            if (previewDesigner != null && previewDesigner.Expanded)
            {
                ActivityPreviewDesignerTheme designerTheme = this.parentDesigner.DesignerTheme as ActivityPreviewDesignerTheme;
                if (designerTheme == null)
                    return;

                Size previewSize = designerTheme.PreviewWindowSize;
                Size previewDesignerSize = previewDesigner.Size;

                float stretchFactor = ((float)previewSize.Width) / previewDesignerSize.Width;
                stretchFactor = Math.Min(stretchFactor, ((float)previewSize.Height) / previewDesignerSize.Height);
                if (stretchFactor < 0.1f) //If we are shrinking less than 10% then we collapse
                {
                    if (!previewDesigner.CanExpandCollapse)
                    {
                        if (previewDesigner.ContainedDesigners.Count > 0)
                            previewDesigner = previewDesigner.ContainedDesigners[0] as CompositeActivityDesigner;
                    }

                    if (previewDesigner != null)
                        previewDesigner.Expanded = false;
                }
            }
        }
        #endregion

        #region Class PreviewWindowAccessibleObject
        private sealed class PreviewWindowAccessibleObject : AccessibleObject
        {
            private PreviewWindow previewWindow;

            internal PreviewWindowAccessibleObject(PreviewWindow previewWindow)
            {
                this.previewWindow = previewWindow;
            }

            public override Rectangle Bounds
            {
                get
                {
                    Rectangle bounds = this.previewWindow.PreviewModeButtonRectangle;
                    WorkflowView workflowView = this.previewWindow.serviceProvider.GetService(typeof(WorkflowView)) as WorkflowView;
                    if (workflowView != null)
                        bounds = new Rectangle(workflowView.LogicalPointToScreen(bounds.Location), workflowView.LogicalSizeToClient(bounds.Size));
                    return bounds;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return DR.GetString(DR.AccessibleAction);
                }
            }

            public override string Description
            {
                get
                {
                    return DR.GetString(DR.PreviewButtonAccessibleDescription);
                }
            }

            public override string Help
            {
                get
                {
                    return DR.GetString(DR.PreviewButtonAccessibleHelp);
                }
            }

            public override string Name
            {
                get
                {
                    return DR.GetString(DR.PreviewButtonName);
                }

                set
                {
                    //We do not allow setting ID programatically
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.previewWindow.parentDesigner.AccessibilityObject;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return AccessibleRole.Diagram;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    return base.State;
                }
            }

            public override void DoDefaultAction()
            {
                this.previewWindow.PreviewMode = !this.previewWindow.PreviewMode;
            }

            public override AccessibleObject Navigate(AccessibleNavigation navdir)
            {
                if (navdir == AccessibleNavigation.Previous)
                {
                    int childCount = this.previewWindow.parentDesigner.AccessibilityObject.GetChildCount();
                    if ((childCount - 3) >= 0)
                        return this.previewWindow.parentDesigner.AccessibilityObject.GetChild(childCount - 3);
                }
                else if (navdir == AccessibleNavigation.Next)
                {
                    if (!this.previewWindow.PreviewMode)
                    {
                        int childCount = this.previewWindow.parentDesigner.AccessibilityObject.GetChildCount();
                        if ((childCount - 1) >= 0)
                            return this.previewWindow.parentDesigner.AccessibilityObject.GetChild(childCount - 1);
                    }
                    else
                    {
                        return this.previewWindow.parentDesigner.AccessibilityObject.Navigate(navdir);
                    }
                }

                return base.Navigate(navdir);
            }

            public override void Select(AccessibleSelection flags)
            {
                base.Select(flags);
            }
        }
        #endregion
    }
    #endregion

    #endregion
}
