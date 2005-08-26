using System;
using System.IO;
using System.Diagnostics;
using System.Web.UI;
using System.Reflection;

namespace Helper {
	public class HtmlWriter : HtmlTextWriter {
		bool full_trace;
		TextWriter output;
		int call_count;

		int NextIndex ()
		{
			return call_count++;
		}

		public HtmlWriter (TextWriter writer) : this (writer, DefaultTabString)
		{
		}
	
		public HtmlWriter (TextWriter writer, string tabString) : base (writer, tabString)
		{
			full_trace = (Environment.GetEnvironmentVariable ("HTMLWRITER_FULLTRACE") == "yes");
			string file = Environment.GetEnvironmentVariable ("HTMLWRITER_FILE");
			Console.WriteLine ("file: '{0}' (null? {1})", file, file == null);
			if (file != null && file != "") {
				output = new StreamWriter (new FileStream (file, FileMode.OpenOrCreate | FileMode.Append));
				Console.WriteLine ("Sending log to '{0}'.", file);
			} else {
				output = Console.Out;
			}
		}

		void WriteTrace (StackTrace trace)
		{
			int n = trace.FrameCount;
			for (int i = 0; i < n; i++) {
				StackFrame frame = trace.GetFrame (i);
				Type type = frame.GetMethod ().DeclaringType;
				string ns = type.Namespace;
				if (ns != "Helper" && !ns.StartsWith ("System.Web.UI"))
					break;
				output.Write ("\t{0}.{1}", type.Name, frame);
			}
			output.WriteLine ();
		}

		public override void AddAttribute (HtmlTextWriterAttribute key, string value, bool fEncode)
		{
			output.WriteLine ("{0:###0} AddAttribute ({1}, {2}, {3}))", NextIndex (), key, value, fEncode);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.AddAttribute (key, value, fEncode);
		}
		
		public override void AddAttribute (string name, string value, bool fEncode)
		{
			output.WriteLine ("{0:###0} AddAttribute ({1}, {2}, {3}))", NextIndex (), name, value, fEncode);
			if (full_trace)
				WriteTrace (new StackTrace ());

			if (fEncode)
				; // FIXME

			base.AddAttribute (name, value, (HtmlTextWriterAttribute) 0);
		}
		
		protected override void AddAttribute (string name, string value, HtmlTextWriterAttribute key)
		{
			output.WriteLine ("{0:###0} AddAttribute ({1}, {2}, {3}))", NextIndex (), name, value, key);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.AddAttribute (name, value, key);
		}
		
		protected override void AddStyleAttribute (string name, string value, HtmlTextWriterStyle key)
		{
			output.WriteLine ("{0:###0} AddStyleAttribute ({1}, {2}, {3}))", NextIndex (), name, value, key);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.AddStyleAttribute (name, value, key);
		}
		
		public override void Close ()
		{
			output.WriteLine ("{0:###0} Close ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			if (output != Console.Out)
				output.Close ();
			base.Close ();	
		}

		protected override string EncodeAttributeValue (HtmlTextWriterAttribute attrKey, string value)
		{
			output.WriteLine ("{0:###0} EncodeAttributeValue ({1}, {2})", NextIndex (), attrKey, value);
			if (full_trace)
				WriteTrace (new StackTrace ());

			return base.EncodeAttributeValue (attrKey, value);
		}
		
		protected override void FilterAttributes ()
		{
			output.WriteLine ("{0:###0} FilterAttributes ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.FilterAttributes ();
		}
	
		public override void Flush ()
		{
			output.WriteLine ("{0:###0} Flush ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.Flush ();
		}

		protected override HtmlTextWriterTag GetTagKey (string tagName) 
		{
			output.WriteLine ("{0:###0} GetTagKey ({1})", NextIndex (), tagName);
			if (full_trace)
				WriteTrace (new StackTrace ());

			return base.GetTagKey (tagName);
		}

		protected override string GetTagName (HtmlTextWriterTag tagKey)
		{
			output.WriteLine ("{0:###0} GetTagName ({1})", NextIndex (), tagKey);
			if (full_trace)
				WriteTrace (new StackTrace ());

			return base.GetTagName (tagKey);
		}
		
		protected override bool OnAttributeRender (string name, string value, HtmlTextWriterAttribute key)
		{
			output.WriteLine ("{0:###0} OnAttributeRender ({1}, {2}, {3})", NextIndex (), name, value, key);
			if (full_trace)
				WriteTrace (new StackTrace ());

			return base.OnAttributeRender (name, value, key);
		}
		
		protected override bool OnStyleAttributeRender (string name, string value, HtmlTextWriterStyle key)
		{
			output.WriteLine ("{0:###0} OnStyleAttributeRender ({1}, {2}, {3})", NextIndex (), name, value, key);
			if (full_trace)
				WriteTrace (new StackTrace ());

			return base.OnStyleAttributeRender (name, value, key);
		}
		
		protected override bool OnTagRender (string name, HtmlTextWriterTag key)
		{
			output.WriteLine ("{0:###0} OnTagRender ({1}, {2})", NextIndex (), name, key);
			if (full_trace)
				WriteTrace (new StackTrace ());

			return base.OnTagRender (name, key);
		}
		
	
		protected override void OutputTabs ()
		{
			output.WriteLine ("{0:###0} OutputTabs ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.OutputTabs ();
		}
	
		protected override string RenderAfterContent ()
		{
			output.WriteLine ("{0:###0} RenderAfterContent ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			return null;
		}
		
		protected override string RenderAfterTag ()
		{
			output.WriteLine ("{0:###0} RenderAfterTag ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			return null;
		}
		
		protected override string RenderBeforeContent ()
		{
			output.WriteLine ("{0:###0} RenderBeforeContent ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			return null;
		}
			
		protected override string RenderBeforeTag ()
		{
			output.WriteLine ("{0:###0} RenderBeforeTag ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			return null;
		}

		public override void RenderBeginTag (string tagName)
		{
			output.WriteLine ("{0:###0} RenderBeginTag ({1})", NextIndex (), tagName);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.RenderBeginTag (tagName);
		}
		
		public override void RenderBeginTag (HtmlTextWriterTag tagKey)
		{
			output.WriteLine ("{0:###0} RenderBeginTag ({1})", NextIndex (), tagKey);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.RenderBeginTag (tagKey);
		}

		public override void RenderEndTag ()
		{
			output.WriteLine ("{0:###0} RenderEndTag ()", NextIndex ());
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.RenderEndTag ();
		}
		
		public override void WriteAttribute (string name, string value, bool fEncode)
		{
			output.WriteLine ("{0:###0} WriteAttribute ({1}, {2}, {3})", NextIndex (), name, value, fEncode);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.WriteAttribute (name, value, fEncode);
		}
		
	
		public override void WriteBeginTag (string tagName)
		{
			output.WriteLine ("{0:###0} WriteBeginTag ({1})", NextIndex (), tagName);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.WriteBeginTag (tagName);
		}
		
		public override void WriteEndTag (string tagName)
		{
			output.WriteLine ("{0:###0} WriteEndTag ({1})", NextIndex (), tagName);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.WriteEndTag (tagName);
		}
		
		public override void WriteFullBeginTag (string tagName)
		{
			output.WriteLine ("{0:###0} WriteFullBeginTag ({1})", NextIndex (), tagName);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.WriteFullBeginTag (tagName);
		}
			
		public override void WriteStyleAttribute (string name, string value, bool fEncode)
		{
			output.WriteLine ("{0:###0} WriteStyleAttribute ({1}, {2}, {3})", NextIndex (), name, value, fEncode);
			if (full_trace)
				WriteTrace (new StackTrace ());

			base.WriteStyleAttribute (name, value, fEncode);
		}
	}
}

