//
// Mono.CSharp.Debugger/IAssemblerWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This is a platform and assembler independent assembler output interface.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public interface IAssemblerWriter
	{
		int GetNextLabelIndex ();

		void WriteLabel (int index);

		void WriteLabel (string label);

		int WriteLabel ();

		void WriteUInt8 (bool value);

		void WriteUInt8 (int value);

		void WriteInt8 (int value);

		void Write2Bytes (int a, int b);

		void WriteUInt16 (int value);

		void WriteInt16 (int value);

		void WriteUInt32 (int value);

		void WriteInt32 (int value);

		void WriteSLeb128 (int value);

		void WriteULeb128 (int value);

		void WriteAddress (int value);

		void WriteString (string value);

		void WriteSectionStart (String section);

		void WriteSectionEnd ();

		void WriteRelativeOffset (int start_label, int end_label);

		void WriteShortRelativeOffset (int start_label, int end_label);

		void WriteAbsoluteOffset (int index);

		void WriteAbsoluteOffset (string label);

		object StartSubsectionWithSize ();

		object StartSubsectionWithShortSize ();

		void EndSubsection (object end_index);
	}
}
