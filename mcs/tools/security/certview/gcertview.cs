using System;
using System.IO;
using System.Reflection;

using Mono.Security.X509;

using Gdk;
using Gtk;
using Glade;
using GLib;
using GtkSharp;

[assembly: AssemblyTitle("Mono Certificate Viewer")]
[assembly: AssemblyDescription("X.509 Certificate Viewer for GTK#")]

namespace Mono.Tools.CertView {

        public class GtkCertificateViewer {

		public static void Main (string[] args)
		{
			string filename = ((args.Length > 0) ? args[0] : null);
			if ((filename != null) && (File.Exists (filename)))
				new GtkCertificateViewer (filename);
			else {
				Console.WriteLine ("Usage: gcertview.exe certificate.cer");
			}
		}

		[Glade.Widget] Button issuerStatementButton;
		[Glade.Widget] Gtk.Image brokenSealImage;
		[Glade.Widget] Gtk.Image sealImage;
		[Glade.Widget] Entry issuedToEntry;
		[Glade.Widget] Entry issuedByEntry;
		[Glade.Widget] Label subjectAltNameLabel;
		[Glade.Widget] TextView certInfoTextview;
		[Glade.Widget] TextView certStatusTextview;
		[Glade.Widget] Entry notBeforeEntry;
		[Glade.Widget] Entry notAfterEntry;
		[Glade.Widget] TreeView detailsTreeview;
		[Glade.Widget] TextView detailsTextview;
		[Glade.Widget] Entry showComboEntry;

		// non widget stuff
		private static Pixbuf[] version;
		private static TreeCellDataFunc dataFunc = null;
		private ListStore allStore;
		private ListStore v1Store;
		private ListStore extensionsStore;
		private ListStore criticalStore;
		private ListStore propertiesStore;
		private ListStore emptyStore;

		// non-glade stuff
		private CertificateFormatter cf;

		public GtkCertificateViewer (string filename) 
		{
			Application.Init();

			Glade.XML gxml = new Glade.XML ("certview.glade", "CertificateViewer", null);
			gxml.Autoconnect (this);

			cf = new CertificateFormatter (filename);

			// init UI
			Tooltips tt = new Tooltips ();
			issuedToEntry.Text = cf.Issuer (false);
			tt.SetTip (issuedToEntry, issuedToEntry.Text, issuedToEntry.Text);
			issuedByEntry.Text = cf.Subject (false);
			tt.SetTip (issuedByEntry, issuedByEntry.Text, issuedByEntry.Text);

			subjectAltNameLabel.Text = cf.SubjectAltName (false);
//			subjectAltNameLabel.Visible = (subjectAltNameLabel.Text != String.Empty);

			notBeforeEntry.Text = cf.Certificate.ValidFrom.ToString ("yyyy-MM-dd");
			notAfterEntry.Text = cf.Certificate.ValidUntil.ToString ("yyyy-MM-dd");

			TextBuffer tb = new TextBuffer (null);
			if (cf.Status != null)
				tb.SetText (cf.Status);
			certStatusTextview.Buffer = tb;
			if (cf.Status != null) {
				certInfoTextview.Buffer = tb;
				certInfoTextview.ModifyText (StateType.Normal, new Gdk.Color (0xff, 0x00, 0x00));
			}

			sealImage.Visible = (cf.Status == null);
			brokenSealImage.Visible = !sealImage.Visible;

			allStore = new ListStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeInt);
			v1Store = new ListStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeInt);
			extensionsStore = new ListStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeInt);
			criticalStore = new ListStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeInt);
			propertiesStore = new ListStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeInt);
			emptyStore = new ListStore ((int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeString, (int)TypeFundamentals.TypeInt);

			AddToStores (CertificateFormatter.FieldNames.Version, cf.Version (false), cf.Version (true), 1);
			AddToStores (CertificateFormatter.FieldNames.SerialNumber, cf.SerialNumber (false), cf.SerialNumber (true), 0);
			AddToStores (CertificateFormatter.FieldNames.SignatureAlgorithm, cf.SignatureAlgorithm (false), cf.SignatureAlgorithm (true), 0);
			AddToStores (CertificateFormatter.FieldNames.Issuer, cf.Issuer (false), cf.Issuer (true), 0);
			AddToStores (CertificateFormatter.FieldNames.ValidFrom, cf.ValidFrom (false), cf.ValidFrom (true), 0);
			AddToStores (CertificateFormatter.FieldNames.ValidUntil, cf.ValidUntil (false), cf.ValidUntil (true), 0);
			AddToStores (CertificateFormatter.FieldNames.Subject, cf.Subject (false), cf.Subject (true), 0);
			AddToStores (CertificateFormatter.FieldNames.PublicKey, cf.PublicKey (false), cf.PublicKey (true), 0);
			for (int i=0; i < cf.Certificate.Extensions.Count; i++) {
				X509Extension xe = cf.GetExtension (i);
				string name = xe.Name;
				int icon = 2;
				if (xe.Critical)
					icon = 3;
				string exts = xe.ToString ();
				string details;
				if (xe.Name == xe.OID) {
					exts = cf.Extension (i, false);
					details = cf.Extension (i, true);
				}
				else {
					details = xe.ToString ();
					exts = CertificateFormatter.OneLine (details);
				}

				AddToStores (name, exts, details, icon);
			}
			AddToStores (CertificateFormatter.PropertyNames.ThumbprintAlgorithm, cf.ThumbprintAlgorithm, cf.ThumbprintAlgorithm, 4);
			string ftb = CertificateFormatter.Array2Word (cf.Thumbprint);
			AddToStores (CertificateFormatter.PropertyNames.Thumbprint, ftb, ftb, 4);

			// select appropriate store to show
			OnShowComboChanged (null, null);

			TreeViewColumn fieldColumn = new TreeViewColumn ();
			CellRendererPixbuf pr = new CellRendererPixbuf ();
			CellRenderer fieldRenderer = new CellRendererText ();
			fieldColumn.PackStart (pr, false);
			fieldColumn.SetCellDataFunc (pr, CellDataFunc, IntPtr.Zero, null);
			fieldColumn.Title = "Field";
			fieldColumn.PackStart (fieldRenderer, false);
			fieldColumn.AddAttribute (fieldRenderer, "text", 0);
			detailsTreeview.AppendColumn (fieldColumn);

			TreeViewColumn valueColumn = new TreeViewColumn ();
			CellRenderer valueRenderer = new CellRendererText ();
			valueColumn.Title = "Value";
			valueColumn.PackStart (valueRenderer, true);
			valueColumn.AddAttribute (valueRenderer, "text", 1);
			detailsTreeview.AppendColumn (valueColumn);

//			detailsTreeview.ModifyText (StateType.Selected, new Gdk.Color (0x33, 0xff, 0x33));

			Application.Run();
		}

		static void SetCellData (TreeViewColumn col, CellRenderer cell, TreeModel model, TreeIter iter)
		{
			GLib.Value icon;
			model.GetValue (iter, 3, out icon);
			CellRendererPixbuf cr = (CellRendererPixbuf) cell;
			cr.Pixbuf = version [(int)icon];
		}

		public static Gtk.TreeCellDataFunc CellDataFunc {
			get {
				if (dataFunc == null) {
					dataFunc = new TreeCellDataFunc (SetCellData);
					version = new Pixbuf [5];
					version [0] = new Pixbuf ("v1.bmp");
					version [1] = new Pixbuf ("v2.bmp");
					version [2] = new Pixbuf ("v3.bmp");
					version [3] = new Pixbuf ("v3critical.bmp");
					version [4] = new Pixbuf ("app.ico");
				}

				return dataFunc;
			}
		}

		private void AddToStores (string fieldName, string fieldValue, string detailedValue, int iconValue) 
		{
			GLib.Value fv = new GLib.Value (fieldName);
			GLib.Value vv = new GLib.Value (fieldValue);
			GLib.Value dv = new GLib.Value (detailedValue);
			GLib.Value iv = new GLib.Value (iconValue);
			TreeIter iter = new TreeIter ();
			switch (iconValue) {
				case 0: // X.509 version 1 fields
					AddToStore (v1Store, iter, fv, vv, dv, iv);
					break;
				case 2: // extensions
					AddToStore (extensionsStore, iter, fv, vv, dv, iv);
					break;
				case 3: // critical extensions
					AddToStore (extensionsStore, iter, fv, vv, dv, iv);
					AddToStore (criticalStore, iter, fv, vv, dv, iv);
					break;
				case 4: // properties
					AddToStore (propertiesStore, iter, fv, vv, dv, iv);
					break;
			}
			// and we always add to allStore
			AddToStore (allStore, iter, fv, vv, dv, iv);
		}

		private void AddToStore (ListStore store, TreeIter iter, GLib.Value field, GLib.Value value, GLib.Value details, GLib.Value icon)
		{
			store.Append (out iter);
			store.SetValue (iter, 0, field);
			store.SetValue (iter, 1, value);
			store.SetValue (iter, 2, details);
			store.SetValue (iter, 3, icon);
		}

		private void OnCursorChanged (object o, EventArgs args)
		{
			TreeModel model;
			TreeIter iter = new TreeIter ();
			TreeSelection ts = detailsTreeview.Selection;
			ts.GetSelected (out model, ref iter);
			GLib.Value val = new GLib.Value ();
			detailsTreeview.Model.GetValue (iter, 2, out val);

			TextBuffer tb = new TextBuffer (null);
			tb.SetText ((string)val);
			detailsTextview.Buffer = tb;
		}

		private void OnShowComboChanged (object o, EventArgs e) 
		{
			// FIXME: yuck - how can I get an index ?
			switch (showComboEntry.Text) {
				case "<All>":
					detailsTreeview.Model = allStore;
					break;
				case "Version 1 Fields Only":
					detailsTreeview.Model = v1Store;
					break;
				case "Extensions Only":
					detailsTreeview.Model = extensionsStore;
					break;
				case "Critical Extensions Only":
					detailsTreeview.Model = criticalStore;
					break;
				case "Properties Only":
					detailsTreeview.Model = propertiesStore;
					break;
				default:
					detailsTreeview.Model = emptyStore;
					break;
			}
		}

		public void OnWindowDeleteEvent (object o, DeleteEventArgs args) 
		{
			Application.Quit ();
			args.RetVal = true;
		}

		public void OnOkButtonClicked (object o, EventArgs e) 
		{
			Application.Quit ();
		}
    
		public void OnIssuerStatementButtonClicked (object o, EventArgs e) 
		{
			// TODO
		}
	}
}
