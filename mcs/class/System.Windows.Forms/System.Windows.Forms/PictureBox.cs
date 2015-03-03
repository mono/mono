// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//

// COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;

namespace System.Windows.Forms {
	[DefaultProperty("Image")]
	[Designer("System.Windows.Forms.Design.PictureBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[Docking (DockingBehavior.Ask)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[DefaultBindingProperty ("Image")]
	public class PictureBox : Control, ISupportInitialize
	{
		#region Fields
		private Image	image;
		private PictureBoxSizeMode size_mode;
		private Image	error_image;
		private string	image_location;
		private Image	initial_image;
		private bool	wait_on_load;
		private WebClient image_download;
		private bool image_from_url;
		private int	no_update;
		#endregion	// Fields

		private EventHandler frame_handler;

		#region Public Constructor
		public PictureBox ()
		{
			//recalc = true;
			no_update = 0;

			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle (ControlStyles.Opaque, false);
			SetStyle (ControlStyles.Selectable, false);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			HandleCreated += new EventHandler(PictureBox_HandleCreated);
			initial_image = ResourceImageLoader.Get ("image-x-generic.png");
			error_image = ResourceImageLoader.Get ("image-missing.png");
		}
		#endregion	// Public Constructor

		#region Public Properties
		[DefaultValue(PictureBoxSizeMode.Normal)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public PictureBoxSizeMode SizeMode {
			get { return size_mode; }
			set {
				if (size_mode == value)
					return;
				size_mode = value;
				
				if (size_mode == PictureBoxSizeMode.AutoSize) {
					AutoSize = true;
					SetAutoSizeMode (AutoSizeMode.GrowAndShrink);
				} else {
					AutoSize = false;
					SetAutoSizeMode (AutoSizeMode.GrowOnly);
				}

				UpdateSize ();
				if (no_update == 0) {
					Invalidate ();
				}

				OnSizeModeChanged (EventArgs.Empty);
			}
		}

		[Bindable (true)]
		[Localizable(true)]
		public Image Image {
			get { return image; }
			set { ChangeImage (value, false); }
		}

		[DefaultValue(BorderStyle.None)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}

		[Localizable (true)]
		[RefreshProperties (RefreshProperties.All)]
		public Image ErrorImage {
			get { return error_image; }
			set { error_image = value; }
		}
		
		[RefreshProperties (RefreshProperties.All)]
		[Localizable(true)]
		public Image InitialImage {
			get { return initial_image; }
			set { initial_image = value; }
		}
		
		[Localizable (true)]
		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.All)]
		public string ImageLocation {
			get { return image_location; }
			set {
				image_location = value;

				if (!string.IsNullOrEmpty (value)) {
					if (WaitOnLoad)
						Load (value);
					else
						LoadAsync (value);
				} else if (image_from_url)
					ChangeImage (null, true);
			}
		}
		
		[Localizable (true)]
		[DefaultValue (false)]
		public bool WaitOnLoad {
			get { return wait_on_load; }
			set { wait_on_load = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value;	}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int TabIndex	{
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Bindable(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get { return base.DefaultImeMode; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AllowDrop {
			get { return base.AllowDrop; }
			set { base.AllowDrop = value; }
		}
		#endregion	// Public Properties

		#region	Protected Instance Methods
		protected override Size DefaultSize {
			get { return ThemeEngine.Current.PictureBoxDefaultSize; }
		}

		protected override void Dispose (bool disposing)
		{
			if (image != null) {
				StopAnimation ();
				image = null;
			}
			initial_image = null;

			base.Dispose (disposing);
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			ThemeEngine.Current.DrawPictureBox (pe.Graphics, pe.ClipRectangle, this);
			base.OnPaint (pe);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		protected virtual void OnSizeModeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SizeModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}
		
		protected virtual void OnLoadCompleted (AsyncCompletedEventArgs e)
		{
			AsyncCompletedEventHandler eh = (AsyncCompletedEventHandler)(Events[LoadCompletedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnLoadProgressChanged (ProgressChangedEventArgs e)
		{
			ProgressChangedEventHandler eh = (ProgressChangedEventHandler)(Events[LoadProgressChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			
			Invalidate ();
		}

		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			if (image == null)
				return base.GetPreferredSizeCore (proposedSize);
			else
				return image.Size;
		}
		#endregion	// Protected Instance Methods

		#region ISupportInitialize Interface
		void System.ComponentModel.ISupportInitialize.BeginInit() {
			no_update++;
		}

		void System.ComponentModel.ISupportInitialize.EndInit() {
			if (no_update > 0) {
				no_update--;
			}
			if (no_update == 0) {
				Invalidate ();
			}
		}
		#endregion	// ISupportInitialize Interface

		#region Private Properties
		private WebClient ImageDownload {
			get { 
				if (image_download == null)
					image_download = new WebClient ();
					
				return image_download;
			}
		}
		#endregion
		
		#region Private Methods

		private void ChangeImage (Image value, bool from_url)
		{
			StopAnimation ();

			image_from_url = from_url;
			image = value;

			if (IsHandleCreated) {
				UpdateSize ();
				if (image != null && ImageAnimator.CanAnimate (image)) {
					frame_handler = new EventHandler (OnAnimateImage);
					ImageAnimator.Animate (image, frame_handler);
				}
				if (no_update == 0) {
					Invalidate ();
				}
			}
		}

		private void StopAnimation ()
		{
			if (frame_handler == null)
				return;
			ImageAnimator.StopAnimate (image, frame_handler);
			frame_handler = null;
		}

		private void UpdateSize ()
		{
			if (image == null)
				return;

			if (Parent != null)
				Parent.PerformLayout (this, "AutoSize");
		}

		private void OnAnimateImage (object sender, EventArgs e)
		{
			// This is called from a worker thread,BeginInvoke is used
			// so the control is updated from the correct thread
			
			// Check if we have a handle again, since it may have gotten
			// destroyed since the last time we checked.
			if (!IsHandleCreated)
				return;
			
			BeginInvoke (new EventHandler (UpdateAnimatedImage), new object [] { this, e });
		}

		private void UpdateAnimatedImage (object sender, EventArgs e)
		{
			// Check if we have a handle again, since it may have gotten
			// destroyed since the last time we checked.
			if (!IsHandleCreated)
				return;
				
			ImageAnimator.UpdateFrames (image);
			Refresh ();
		}

		private void PictureBox_HandleCreated(object sender, EventArgs e) {
			UpdateSize ();
			if (image != null && ImageAnimator.CanAnimate (image)) {
				frame_handler = new EventHandler (OnAnimateImage);
				ImageAnimator.Animate (image, frame_handler);
			}
			if (no_update == 0) {
				Invalidate ();
			}
		}

		void ImageDownload_DownloadDataCompleted (object sender, DownloadDataCompletedEventArgs e)
		{
			if (e.Error != null && !e.Cancelled)
				Image = error_image;
			else if (e.Error == null && !e.Cancelled)
				using (MemoryStream ms = new MemoryStream (e.Result))
					Image = Image.FromStream (ms);
					
			ImageDownload.DownloadProgressChanged -= new DownloadProgressChangedEventHandler (ImageDownload_DownloadProgressChanged);
			ImageDownload.DownloadDataCompleted -= new DownloadDataCompletedEventHandler (ImageDownload_DownloadDataCompleted);
			image_download = null;
			
			OnLoadCompleted (e);
		}

		private void ImageDownload_DownloadProgressChanged (object sender, DownloadProgressChangedEventArgs e)
		{
			OnLoadProgressChanged (new ProgressChangedEventArgs (e.ProgressPercentage, e.UserState));
		}
		#endregion	// Private Methods

		#region Public Instance Methods
		public void CancelAsync ()
		{
			if (image_download != null)
				image_download.CancelAsync ();
		}
		
		public void Load ()
		{
			Load (image_location);
		}
		
		public void Load (string url)
		{
			if (string.IsNullOrEmpty (url))
				throw new InvalidOperationException ("ImageLocation not specified.");
			
			image_location = url;
			
			if (url.Contains ("://"))
				using (Stream s = ImageDownload.OpenRead (url))
					ChangeImage (Image.FromStream (s), true);
			else
				ChangeImage (Image.FromFile (url), true);
		}
		
		public void LoadAsync ()
		{
			LoadAsync (image_location);
		}
		
		public void LoadAsync (string url)
		{
			// If WaitOnLoad is true, do not do async
			if (wait_on_load) {
				Load (url);
				return;
			}

			if (string.IsNullOrEmpty (url))
				throw new InvalidOperationException ("ImageLocation not specified.");

			image_location = url;
			ChangeImage (InitialImage, true);
			
			if (ImageDownload.IsBusy)
				ImageDownload.CancelAsync ();

			Uri uri = null;
			try {
				uri = new Uri (url);
			} catch (UriFormatException) {
				uri = new Uri (Path.GetFullPath (url));
			}

			ImageDownload.DownloadProgressChanged += new DownloadProgressChangedEventHandler (ImageDownload_DownloadProgressChanged);
			ImageDownload.DownloadDataCompleted += new DownloadDataCompletedEventHandler (ImageDownload_DownloadDataCompleted);
			ImageDownload.DownloadDataAsync (uri);
		}

		public override string ToString() {
			return String.Format("{0}, SizeMode: {1}", base.ToString (), SizeMode);
		}
		#endregion

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		static object LoadCompletedEvent = new object ();
		static object LoadProgressChangedEvent = new object ();

		public event AsyncCompletedEventHandler LoadCompleted {
			add { Events.AddHandler (LoadCompletedEvent, value); }
			remove { Events.RemoveHandler (LoadCompletedEvent, value); }
		}

		public event ProgressChangedEventHandler LoadProgressChanged {
			add { Events.AddHandler (LoadProgressChangedEvent, value); }
			remove { Events.RemoveHandler (LoadProgressChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		static object SizeModeChangedEvent = new object ();
		public event EventHandler SizeModeChanged {
			add { Events.AddHandler (SizeModeChangedEvent, value); }
			remove { Events.RemoveHandler (SizeModeChangedEvent, value); }
		}

		#endregion	// Events
	}
}

