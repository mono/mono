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
			writer.WriteLine ("#NOAPP");
		}

		private StreamWriter writer;
		private static int next_anon_label_idx = 0;
		private bool in_section = false;

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
			int index = ++next_anon_label_idx;

			WriteLabel (index);

			return index;
		}

		public void WriteLabel (int index)
		{
			char[] output = { '.', 'L', '_', '\0', '\0', '\0', '\0', '\0', '\0',
					  ':', '\n' };

			unchecked {
				int value = (int) index;
				output [3] = hex [(value & 0xf00000) >> 20];
				output [4] = hex [(value & 0x0f0000) >> 16];
				output [5] = hex [(value & 0x00f000) >> 12];
				output [6] = hex [(value & 0x000f00) >>  8];
				output [7] = hex [(value & 0x0000f0) >>  4];
				output [8] = hex [(value & 0x00000f)];
			}

			writer.Write (output, 0, output.Length);
		}

		private static readonly char[] hex = { '0', '1', '2', '3', '4', '5', '6', '7',
						       '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

		public void WriteUInt8 (bool value)
		{
			WriteUInt8 (value ? 1 : 0);
		}

		public void WriteUInt8 (int value)
		{
			char[] output = { '\t', '.', 'b', 'y', 't', 'e', ' ',
					  '0', 'x', '\0', '\0',
					  '\n' };

			unchecked {
				output [9] = hex [(value & 0xf0) >> 4];
				output [10] = hex [value & 0x0f];
			}

			writer.Write (output, 0, output.Length);
		}

		public void WriteInt8 (int value)
		{
			char[] output = { '\t', '.', 'b', 'y', 't', 'e', ' ',
					  '0', 'x', '\0', '\0',
					  '\n' };

			unchecked {
				uint uvalue = (uint) value;
				output [9] = hex [(uvalue & 0xf0) >> 4];
				output [10] = hex [uvalue & 0x0f];
			}

			writer.Write (output, 0, output.Length);
		}

		public void Write2Bytes (int a, int b)
		{
			char[] output = { '\t', '.', 'b', 'y', 't', 'e', ' ',
					  '0', 'x', '\0', '\0', ',', ' ',
					  '0', 'x', '\0', '\0',
					  '\n' };

			unchecked {
				uint ua = (uint) a;
				uint ub = (uint) b;
				output [9] = hex [(ua & 0xf0) >> 4];
				output [10] = hex [ua & 0x0f];
				output [15] = hex [(ub & 0xf0) >> 4];
				output [16] = hex [ub & 0x0f];
			}

			writer.Write (output, 0, output.Length);
		}

		public void WriteUInt16 (int value)
		{
			writer.WriteLine ("\t.word " + value);
		}

		public void WriteInt16 (int value)
		{
			writer.WriteLine ("\t.word " + value);
		}

		public void WriteUInt32 (int value)
		{
			char[] output = { '\t', '.', 'l', 'o', 'n', 'g', ' ',
					  '0', 'x', '\0', '\0', '\0', '\0', '\0', '\0',
					  '\0', '\0', '\n' };

			unchecked {
				output [9] = hex [(value & 0xf0000000) >> 28];
				output [10] = hex [(value & 0x0f000000) >> 24];
				output [11] = hex [(value & 0x00f00000) >> 20];
				output [12] = hex [(value & 0x000f0000) >> 16];
				output [13] = hex [(value & 0x0000f000) >> 12];
				output [14] = hex [(value & 0x00000f00) >>  8];
				output [15] = hex [(value & 0x000000f0) >>  4];
				output [16] = hex [(value & 0x0000000f)];
			}

			writer.Write (output, 0, output.Length);
		}

		public void WriteInt32 (int value)
		{
			char[] output = { '\t', '.', 'l', 'o', 'n', 'g', ' ',
					  '0', 'x', '\0', '\0', '\0', '\0', '\0', '\0',
					  '\0', '\0', '\n' };

			unchecked {
				uint uvalue = (uint) value;
				output [9] = hex [(uvalue & 0xf0000000) >> 28];
				output [10] = hex [(uvalue & 0x0f000000) >> 24];
				output [11] = hex [(uvalue & 0x00f00000) >> 20];
				output [12] = hex [(uvalue & 0x000f0000) >> 16];
				output [13] = hex [(uvalue & 0x0000f000) >> 12];
				output [14] = hex [(uvalue & 0x00000f00) >>  8];
				output [15] = hex [(uvalue & 0x000000f0) >>  4];
				output [16] = hex [(uvalue & 0x0000000f)];
			}

			writer.Write (output, 0, output.Length);
		}

		public void WriteSLeb128 (int value)
		{
			writer.WriteLine ("\t.sleb128 " + value);
		}

		public void WriteULeb128 (int value)
		{
			writer.WriteLine ("\t.uleb128 " + value);
		}

		public void WriteAddress (int value)
		{
			if (value == 0)
				writer.WriteLine ("\t.long 0");
			else
				writer.WriteLine ("\t.long " + value);
		}

		public void WriteString (string value)
		{
			writer.WriteLine ("\t.string \"" + value + "\"");
		}

		public void WriteSectionStart (String section)
		{
			if (in_section)
				throw new Exception ();
			in_section = true;
			writer.WriteLine ("\t.section ." + section);
		}

		public void WriteSectionEnd ()
		{
			in_section = false;
		}

		public void WriteRelativeOffset (int start_label, int end_label)
		{
			char[] output = { '\t', '.', 'l', 'o', 'n', 'g', ' ',
					  '.', 'L', '_', '\0', '\0', '\0', '\0', '\0', '\0',
					  ' ', '-', ' ',
					  '.', 'L', '_', '\0', '\0', '\0', '\0', '\0', '\0',
					  '\n' };

			unchecked {
				output [10] = hex [(end_label & 0xf00000) >> 20];
				output [11] = hex [(end_label & 0x0f0000) >> 16];
				output [12] = hex [(end_label & 0x00f000) >> 12];
				output [13] = hex [(end_label & 0x000f00) >>  8];
				output [14] = hex [(end_label & 0x0000f0) >>  4];
				output [15] = hex [(end_label & 0x00000f)];
				output [22] = hex [(start_label & 0xf00000) >> 20];
				output [23] = hex [(start_label & 0x0f0000) >> 16];
				output [24] = hex [(start_label & 0x00f000) >> 12];
				output [25] = hex [(start_label & 0x000f00) >>  8];
				output [26] = hex [(start_label & 0x0000f0) >>  4];
				output [27] = hex [(start_label & 0x00000f)];
			}

			writer.Write (output, 0, output.Length);
		}

		public void WriteShortRelativeOffset (int start_label, int end_label)
		{
			char[] output = { '\t', '.', 'b', 'y', 't', 'e', ' ',
					  '.', 'L', '_', '\0', '\0', '\0', '\0', '\0', '\0',
					  ' ', '-', ' ',
					  '.', 'L', '_', '\0', '\0', '\0', '\0', '\0', '\0',
					  '\n' };

			unchecked {
				output [10] = hex [(end_label & 0xf00000) >> 20];
				output [11] = hex [(end_label & 0x0f0000) >> 16];
				output [12] = hex [(end_label & 0x00f000) >> 12];
				output [13] = hex [(end_label & 0x000f00) >>  8];
				output [14] = hex [(end_label & 0x0000f0) >>  4];
				output [15] = hex [(end_label & 0x00000f)];
				output [22] = hex [(start_label & 0xf00000) >> 20];
				output [23] = hex [(start_label & 0x0f0000) >> 16];
				output [24] = hex [(start_label & 0x00f000) >> 12];
				output [25] = hex [(start_label & 0x000f00) >>  8];
				output [26] = hex [(start_label & 0x0000f0) >>  4];
				output [27] = hex [(start_label & 0x00000f)];
			}

			writer.Write (output, 0, output.Length);
		}

		public void WriteAbsoluteOffset (string label)
		{
			writer.WriteLine ("\t.long .L_" + label);
		}

		public void WriteAbsoluteOffset (int index)
		{
			char[] output = { '\t', '.', 'l', 'o', 'n', 'g', ' ',
					  '.', 'L', '_', '\0', '\0', '\0', '\0', '\0', '\0',
					  '\n' };

			unchecked {
				output [10] = hex [(index & 0xf00000) >> 20];
				output [11] = hex [(index & 0x0f0000) >> 16];
				output [12] = hex [(index & 0x00f000) >> 12];
				output [13] = hex [(index & 0x000f00) >>  8];
				output [14] = hex [(index & 0x0000f0) >>  4];
				output [15] = hex [(index & 0x00000f)];
			}

			writer.Write (output, 0, output.Length);

		}

		public object StartSubsectionWithSize ()
		{
			int start_index = ++next_anon_label_idx;
			int end_index = ++next_anon_label_idx;

			WriteRelativeOffset (start_index, end_index);
			WriteLabel (start_index);

			return end_index;
		}

		public object StartSubsectionWithShortSize ()
		{
			int start_index = ++next_anon_label_idx;
			int end_index = ++next_anon_label_idx;

			WriteShortRelativeOffset (start_index, end_index);
			WriteLabel (start_index);

			return end_index;
		}

		public void EndSubsection (object end_index)
		{
			WriteLabel ((int) end_index);
		}
	}
}
