using System;
using Cairo;
using System.Windows.Forms;
using System.Drawing;

namespace Cairo.Snippets
{
	class CairoSnippetsSwf
	{
		int width = 400;
		int height = 200;

		DrawingArea da;

		static void Main ()
		{
			new CairoSnippetsSwf ();
			Application.Run ();
		}

		CairoSnippetsSwf ()
		{
			Form f = new Form ();
			f.ClientSize = new Size (width, height);
			f.Closed += OnClosed;

			Splitter split = new Splitter ();
			split.Dock = DockStyle.Left;
			split.SplitPosition = width / 2;
			ListView lv = new ListView ();
			foreach (string s in Snippets.snippets)
				lv.Items.Add (new ListViewItem (s));
			lv.Dock = DockStyle.Left;
			lv.SelectedIndexChanged += OnSelected;

			da = new DrawingArea ();
			da.Dock = DockStyle.Right;
			f.Controls.AddRange (new Control[] {split, lv, da});
			
			f.Show ();
		}

		void OnClosed (object sender, EventArgs e)
		{
			Application.Exit ();
		}

		void OnSelected (object sender, EventArgs e)
		{
			ListView lv = sender as ListView;
			if (lv.SelectedItems.Count > 0)
				da.Draw (lv.SelectedItems[0].Text, width / 2, height);
		}
	}

	public class DrawingArea : Panel
	{
		string name = "arc";
		Snippets snips = new Snippets ();
		int w, h;
			
		public void Draw (string snippet, int width, int height)
		{
			name = snippet;
			w = width;
			h = height;
			Invalidate ();
		}
			
		protected override void OnPaint (PaintEventArgs e)
		{
			IntPtr hdc = e.Graphics.GetHdc ();
			// will only work on win32
			Win32Surface s = new Win32Surface (hdc);
			Context cr = new Context (s);
			Snippets.InvokeSnippet (snips, name, cr, w, h);
			e.Graphics.ReleaseHdc (hdc);
		}
	}
}

