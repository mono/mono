using System;
using Cairo;
using Gtk;

namespace Cairo.Snippets
{
	public class CairoSnippetsGtk
	{
		int width = 400;
		int height = 200;

		DrawingArea da = new DrawingArea ();

		Snippets snips = new Snippets ();
		string selected = "";

		public static void Main(string[] args)
		{
			Application.Init ();
			new CairoSnippetsGtk ();
			Application.Run ();
		}

		public CairoSnippetsGtk ()
		{
			Window w = new Window ("Cairo snippets");
			w.SetDefaultSize (width, height);
			w.DeleteEvent += delegate { Application.Quit (); };

			HPaned hpane = new HPaned ();
			ScrolledWindow sw = new ScrolledWindow ();
			TreeView tv = new TreeView ();
			tv.HeadersVisible = false;
			tv.AppendColumn ("snippets", new CellRendererText (), "text", 0);
			tv.Model = GetModel ();
			tv.Selection.Changed += OnSelectionChanged;
			sw.Add (tv);
			hpane.Add1 (sw);
			da = new DrawingArea ();
			da.ExposeEvent += OnExposed;
			hpane.Add2 (da);
			hpane.Position = width / 2;
			w.Add (hpane);
			
			w.ShowAll ();
		}

		ListStore GetModel ()
		{
			ListStore store = new ListStore (typeof (string));
			foreach (string s in Snippets.snippets)
				store.AppendValues (s);
			return store;
		}

		void OnExposed (object sender, ExposeEventArgs e)
		{
			Context cr = Gdk.CairoHelper.Create (da.GdkWindow);

			int w, h;
			da.GdkWindow.GetSize (out w, out h);

			// set window bg
			cr.ColorRgb = new Color (1, 1, 1 );
			cr.Rectangle (0, 0, w, h);
			cr.Fill ();
			// reset it
			cr.ColorRgb = new Color (0, 0, 0);

			Snippets.InvokeSnippet (snips, selected, cr, w, h);

			e.RetVal = true;
		}

		void OnSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			TreeModel model;
			TreeSelection selection = sender as TreeSelection;

			if (selection.GetSelected (out model, out iter))
			{
				selected = (string) model.GetValue (iter, 0);
			}

			da.QueueDraw ();
		}
	}
}

