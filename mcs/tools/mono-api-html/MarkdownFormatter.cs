// 
// Authors
//    Sebastien Pouliot  <sebastien.pouliot@microsoft.com>
//
// Copyright 2018 Microsoft Inc.
// 
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

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace Mono.ApiTools {

	class MarkdownFormatter : Formatter {

		public MarkdownFormatter (State state)
			: base (state)
		{
		}

		public override string LesserThan => "<";
		public override string GreaterThan => ">";

		public override void BeginDocument (TextWriter output, string title)
		{
			output.WriteLine ($"# {title}");
			output.WriteLine ();
		}

		public override void BeginAssembly (TextWriter output)
		{
			// this serves as ToC (table of content) entries so we skip the "Assembly: " prefix
			output.WriteLine ($"## {State.Assembly}.dll");
			output.WriteLine ();
		}

		public override void BeginNamespace (TextWriter output, string action)
		{
			output.WriteLine ($"### {action}Namespace {State.Namespace}");
			output.WriteLine ();
		}

		public override void BeginTypeAddition (TextWriter output)
		{
			output.WriteLine ($"#### New Type: {@State.Namespace}.{State.Type}");
			output.WriteLine ();
			output.WriteLine ("```csharp");
		}

		public override void EndTypeAddition (TextWriter output)
		{
			output.WriteLine ("```");
			output.WriteLine ();
		}

		public override void BeginTypeModification (TextWriter output)
		{
			output.WriteLine ($"#### Type Changed: {State.Namespace}.{State.Type}");
			output.WriteLine ();
		}

		public override void BeginTypeRemoval (TextWriter output)
		{
			output.WriteLine ($"#### Removed Type {State.Namespace}.{State.Type}");
		}

		public override void BeginMemberAddition (TextWriter output, IEnumerable<XElement> list, MemberComparer member)
		{
			if (State.BaseType == "System.Enum") {
				output.WriteLine ("Added value{0}:", list.Count () > 1 ? "s" : String.Empty);
			} else {
				output.WriteLine ("Added {0}:", list.Count () > 1 ? member.GroupName : member.ElementName);
			}
			output.WriteLine ();
			output.WriteLine ("```csharp");
		}

		public override void AddMember (TextWriter output, MemberComparer member, bool isInterfaceBreakingChange, string obsolete, string description)
		{
			output.Write (obsolete);
			output.WriteLine (description);
		}

		public override void EndMemberAddition (TextWriter output)
		{
			output.WriteLine ("```");
			output.WriteLine ();
		}

		public override void BeginMemberModification (TextWriter output, string sectionName)
		{
			output.WriteLine ($"{sectionName}:");
			output.WriteLine ();
			output.WriteLine ("```diff");
		}

		public override void EndMemberModification (TextWriter output)
		{
			output.WriteLine ("```");
			output.WriteLine ();
		}

		public override void BeginMemberRemoval (TextWriter output, IEnumerable<XElement> list, MemberComparer member)
		{
			if (State.BaseType == "System.Enum") {
				output.WriteLine ("Removed value{0}:", list.Count () > 1 ? "s" : String.Empty);
			} else {
				output.WriteLine ("Removed {0}:", list.Count () > 1 ? member.GroupName : member.ElementName);
			}
			output.WriteLine ();
			output.WriteLine ("```csharp");
		}

		public override void RemoveMember (TextWriter output, MemberComparer member, bool is_breaking, string obsolete, string description)
		{
			output.Write (obsolete);
			output.WriteLine (description);
		}

		public override void EndMemberRemoval (TextWriter output)
		{
			output.WriteLine ("```");
			output.WriteLine ();
		}

		public override void RenderObsoleteMessage (StringBuilder output, MemberComparer member, string description, string optionalObsoleteMessage)
		{
			output.Append ("[Obsolete (");
			if (!String.IsNullOrEmpty (optionalObsoleteMessage))
				output.Append ('"').Append (optionalObsoleteMessage).Append ('"');
			output.AppendLine (")]");
			output.Append (description);
		}

		string Clean (string line, string remove, string keep)
		{
			var cleaned = line.Replace (remove, String.Empty);
			int s = cleaned.IndexOf (keep, StringComparison.Ordinal);
			if (s != -1) {
				int e = cleaned.IndexOf (keep, s + keep.Length, StringComparison.Ordinal);
				cleaned = cleaned.Remove (s, e - s + keep.Length);
			}
			while (cleaned.Contains ("  "))
				cleaned = cleaned.Replace ("  ", " ");
			return cleaned;
		}

		public override void DiffAddition (StringBuilder output, string text, bool breaking)
		{
			output.Append ("+++");
			output.Append (text);
			output.Append ("+++");
		}

		public override void DiffModification (StringBuilder output, string old, string @new, bool breaking)
		{
			if (old.Length > 0)
				DiffAddition (output, old, breaking);
			if (@new.Length > 0)
				DiffRemoval (output, @new, true);
		}

		public override void DiffRemoval (StringBuilder output, string text, bool breaking)
		{
			output.Append ("---");
			output.Append (text);
			output.Append ("---");
		}

		public override void Diff (TextWriter output, ApiChange apichange)
		{
			foreach (var line in apichange.Member.ToString ().Split (new[] { Environment.NewLine }, 0)) {
				if (line.Contains ("+++")) {
					output.WriteLine ("-{0}", Clean (line, "+++", "---"));
					output.WriteLine ("+{0}", Clean (line, "---", "+++"));
				} else {
					output.WriteLine (" {0}", line);
				}
			}
		}
	}
}
