//
// Mono.CSharp.Debugger/AssemblerWriterI386.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// IAssemblerWriter implementation for the Intel i386.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public class AssemblerWriterI386 : IAssemblerWriter
	{
		public AssemblerWriterI386 (StreamWriter writer) {
			this.writer = writer;
		}

		private StreamWriter writer;
		private static int next_anon_label_idx = 0;

		public void WriteLabel (string label)
		{
			writer.WriteLine (".L_" + label + ":");
		}

		public int GetNextLabelIndex ()
		{
			return ++next_anon_label_idx;
		}

		public int WriteLabel ()
		{
			int index = GetNextLabelIndex ();

			WriteLabel (index);

			return index;
		}

		public string GetLabelName (int index)
		{
			return index.ToString ();
		}

		public void WriteLabel (int index)
		{
			WriteLabel (GetLabelName (index));
		}

		public void WriteUInt8 (bool value)
		{
			writer.WriteLine ("\t.byte\t\t" + (value ? 1 : 0));
		}

		public void WriteUInt8 (int value)
		{
			writer.WriteLine ("\t.byte\t\t" + value);
		}

		public void WriteInt8 (int value)
		{
			writer.WriteLine ("\t.byte\t\t" + value);
		}

		public void WriteUInt16 (int value)
		{
			writer.WriteLine ("\t.2byte\t\t" + value);
		}

		public void WriteInt16 (int value)
		{
			writer.WriteLine ("\t.2byte\t\t" + value);
		}

		public void WriteUInt32 (int value)
		{
			writer.WriteLine ("\t.long\t\t" + value);
		}

		public void WriteInt32 (int value)
		{
			writer.WriteLine ("\t.long\t\t" + value);
		}

		public void WriteSLeb128 (int value)
		{
			writer.WriteLine ("\t.sleb128\t" + value);
		}

		public void WriteULeb128 (int value)
		{
			writer.WriteLine ("\t.uleb128\t" + value);
		}

		public void WriteAddress (int value)
		{
			writer.WriteLine ("\t.long\t\t" + value);
		}

		public void WriteString (string value)
		{
			writer.WriteLine ("\t.string\t\t\"" + value + "\"");
		}

		public void WriteSectionStart (String section)
		{
			writer.WriteLine ("\t.section\t." + section);
		}

		public void WriteSectionEnd ()
		{
			writer.WriteLine ("\t.previous\n");
		}

		public void WriteRelativeOffset (string start_label, string end_label)
		{
			writer.WriteLine ("\t.long\t\t.L_" + end_label + " - .L_" + start_label);
		}

		public void WriteShortRelativeOffset (string start_label, string end_label)
		{
			writer.WriteLine ("\t.byte\t\t.L_" + end_label + " - .L_" + start_label);
		}

		public void WriteAbsoluteOffset (string label)
		{
			writer.WriteLine ("\t.long\t\t.L_" + label);
		}

		public object StartSubsectionWithSize ()
		{
			int start_index = GetNextLabelIndex ();
			int end_index = GetNextLabelIndex ();

			WriteRelativeOffset (GetLabelName (start_index), GetLabelName (end_index));
			WriteLabel (start_index);

			return end_index;
		}

		public object StartSubsectionWithShortSize ()
		{
			int start_index = GetNextLabelIndex ();
			int end_index = GetNextLabelIndex ();

			WriteShortRelativeOffset (GetLabelName (start_index), GetLabelName (end_index));
			WriteLabel (start_index);

			return end_index;
		}

		public void EndSubsection (object end_index)
		{
			WriteLabel ((int) end_index);
		}
	}
}

