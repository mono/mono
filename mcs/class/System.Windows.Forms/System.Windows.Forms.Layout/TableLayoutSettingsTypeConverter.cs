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
// Copyright (c) 2006 Novell, Inc.
//


using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Collections.Generic;

namespace System.Windows.Forms.Layout
{
	public class TableLayoutSettingsTypeConverter : TypeConverter
	{
		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
				return true;

			return base.CanConvertTo (context, destinationType);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		

		public override object ConvertTo (ITypeDescriptorContext context,
						  CultureInfo culture,
						  object value,
						  Type destinationType)
		{
			if (!(value is TableLayoutSettings) || destinationType != typeof (string))
				return base.ConvertTo (context, culture, value, destinationType);

			TableLayoutSettings settings = value as TableLayoutSettings;
			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.WriteStartDocument ();
			List<ControlInfo> list = settings.GetControls ();
			xw.WriteStartElement ("TableLayoutSettings");
			xw.WriteStartElement ("Controls");
			
			foreach (ControlInfo info in list) {
				xw.WriteStartElement ("Control");
				xw.WriteAttributeString ("Name", info.Control.ToString ());
				xw.WriteAttributeString ("Row", info.Row.ToString ());
				xw.WriteAttributeString ("RowSpan", info.RowSpan.ToString ());
				xw.WriteAttributeString ("Column", info.Col.ToString ());
				xw.WriteAttributeString ("ColumnSpan", info.ColSpan.ToString ());
				xw.WriteEndElement ();
			}
			xw.WriteEndElement ();


			List<string> styles = new List<string> ();
			
			foreach (ColumnStyle style in settings.ColumnStyles) {
				styles.Add (style.SizeType.ToString ());
				styles.Add (style.Width.ToString (CultureInfo.InvariantCulture));
			}

			
			xw.WriteStartElement ("Columns");
			xw.WriteAttributeString ("Styles", String.Join (",", styles.ToArray ()));
			xw.WriteEndElement ();
			
			styles.Clear();
			foreach (RowStyle style in settings.RowStyles) {
				styles.Add (style.SizeType.ToString ());
				styles.Add (style.Height.ToString (CultureInfo.InvariantCulture));
			}

			xw.WriteStartElement ("Rows");
			xw.WriteAttributeString ("Styles", String.Join (",", styles.ToArray ()));
			xw.WriteEndElement ();

			xw.WriteEndElement ();
			xw.WriteEndDocument ();
			xw.Close ();

			return sw.ToString ();

		}

		public override object ConvertFrom (ITypeDescriptorContext context,
						    CultureInfo culture,
						    object value)
		{
			if (!(value is string))
				return base.ConvertFrom(context, culture, value);

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml (value as string);
			TableLayoutSettings settings = new TableLayoutSettings(null);
			int count = ParseControl (xmldoc, settings);
			ParseColumnStyle (xmldoc, settings);
			ParseRowStyle (xmldoc, settings);
			settings.RowCount = count;
			

			return settings;
		}


		private int ParseControl (XmlDocument xmldoc, TableLayoutSettings settings)
		{
			int count = 0;
			foreach (XmlNode node in xmldoc.GetElementsByTagName ("Control")) {
				if (node.Attributes["Name"] == null || string.IsNullOrEmpty(node.Attributes["Name"].Value))
					continue;
				if (node.Attributes["Row"] != null) {
					settings.SetRow (node.Attributes["Name"].Value, GetValue (node.Attributes["Row"].Value));
					count++;
				}
				if (node.Attributes["RowSpan"] != null) {
					settings.SetRowSpan (node.Attributes["Name"].Value, GetValue (node.Attributes["RowSpan"].Value));
				}
				if (node.Attributes["Column"] != null)
					settings.SetColumn (node.Attributes["Name"].Value, GetValue (node.Attributes["Column"].Value));
				if (node.Attributes["ColumnSpan"] != null)
					settings.SetColumnSpan (node.Attributes["Name"].Value, GetValue (node.Attributes["ColumnSpan"].Value));
			}
			return count;
		}

		private void ParseColumnStyle (XmlDocument xmldoc, TableLayoutSettings settings)
		{
			foreach (XmlNode node in xmldoc.GetElementsByTagName ("Columns")) {
				if (node.Attributes["Styles"] == null)
					continue;
				string styles = node.Attributes["Styles"].Value;
				if (string.IsNullOrEmpty (styles))
					continue;
				string[] list = BuggySplit (styles);
				for (int i = 0; i < list.Length; i+=2) {
					float width = 0f;
					SizeType type = (SizeType) Enum.Parse (typeof (SizeType), list[i]);
					float.TryParse (list[i+1], NumberStyles.Float, CultureInfo.InvariantCulture, out width);
					settings.ColumnStyles.Add (new ColumnStyle (type, width));
				}				
			}
		}

		private void ParseRowStyle (XmlDocument xmldoc, TableLayoutSettings settings)
		{
			foreach (XmlNode node in xmldoc.GetElementsByTagName ("Rows")) {
				if (node.Attributes["Styles"] == null)
					continue;
				string styles = node.Attributes["Styles"].Value;
				if (string.IsNullOrEmpty(styles))
					continue;
				string[] list = BuggySplit (styles);
				for (int i = 0; i < list.Length; i += 2) {
					float height = 0f;
					SizeType type = (SizeType) Enum.Parse (typeof (SizeType), list[i]);
					float.TryParse (list[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out height);
					settings.RowStyles.Add (new RowStyle (type, height));
				}
			}
		}

		private int GetValue (string attValue)
		{
			int val = -1;
			if (!string.IsNullOrEmpty (attValue)) {
				int.TryParse (attValue, out val);
			}
			return val;
		}

		// .Net accidently uses the local culture separator, so
		// Percent,50.0,Percent,50.0	can be
		// Percent,50,0,Percent,50,0
		// Make sure we can handle this
		private string[] BuggySplit (string s)
		{
			List<string> l = new List<string> ();
			
			string[] split = s.Split (',');
			
			for (int i = 0; i < split.Length; i++) {
				switch (split[i].ToLowerInvariant ()) {
					case "autosize":
					case "absolute":
					case "percent":
						l.Add (split[i]);
						break;
					default:
						if (i + 1 < split.Length) {
							float test;
							
							if (float.TryParse (split[i + 1], out test)) {
								l.Add (string.Format ("{0}.{1}", split[i], split[i + 1]));
								i++;	
							} else
								l.Add (split[i]);
						} else
							l.Add (split[i]);
						break;
				}
			}
			
			return l.ToArray ();
		}
	}
}
