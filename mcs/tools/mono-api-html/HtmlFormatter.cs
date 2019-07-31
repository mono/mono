// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013 Xamarin Inc. http://www.xamarin.com
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

	class HtmlFormatter : Formatter {

		public HtmlFormatter (State state)
			: base (state)
		{
		}

		public override string LesserThan => "&lt;";
		public override string GreaterThan => "&gt;";

		protected void Indent (TextWriter output)
		{
			 for (int i = 0; i < State.Indent; i++)
			 	output.Write ("\t");
		}

		public override void BeginDocument (TextWriter output, string title)
		{
			output.WriteLine ("<div>");
			if (State.Colorize) {
				output.WriteLine ("<style scoped>");
				output.WriteLine ("\t.obsolete { color: gray; }");
				output.WriteLine ("\t.added { color: green; }");
				output.WriteLine ("\t.removed-inline { text-decoration: line-through; }");
				output.WriteLine ("\t.removed-breaking-inline { color: red;}");
				output.WriteLine ("\t.added-breaking-inline { text-decoration: underline; }");
				output.WriteLine ("\t.nonbreaking { color: black; }");
				output.WriteLine ("\t.breaking { color: red; }");
				output.WriteLine ("</style>");
			}
			output.WriteLine (
				@"<script type=""text/javascript"">
	// Only some elements have 'data-is-[non-]breaking' attributes. Here we
	// iterate over all descendents elements, and set 'data-is-[non-]breaking'
	// depending on whether there are any descendents with that attribute.
	function propagateDataAttribute (element)
	{
		if (element.hasAttribute ('data-is-propagated'))
			return;

		var i;
		var any_breaking = element.hasAttribute ('data-is-breaking');
		var any_non_breaking = element.hasAttribute ('data-is-non-breaking');
		for (i = 0; i < element.children.length; i++) {
			var el = element.children [i];
			propagateDataAttribute (el);
			any_breaking |= el.hasAttribute ('data-is-breaking');
			any_non_breaking |= el.hasAttribute ('data-is-non-breaking');
		}
		
		if (any_breaking)
			element.setAttribute ('data-is-breaking', null);
		else if (any_non_breaking)
			element.setAttribute ('data-is-non-breaking', null);
		element.setAttribute ('data-is-propagated', null);
	}

	function hideNonBreakingChanges ()
	{
		var topNodes = document.querySelectorAll ('[data-is-topmost]');
		var n;
		var i;
		for (n = 0; n < topNodes.length; n++) {
			propagateDataAttribute (topNodes [n]);
			var elements = topNodes [n].querySelectorAll ('[data-is-non-breaking]');
			for (i = 0; i < elements.length; i++) {
				var el = elements [i];
				if (!el.hasAttribute ('data-original-display'))
					el.setAttribute ('data-original-display', el.style.display);
				el.style.display = 'none';
			}
		}
		
		var links = document.getElementsByClassName ('hide-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = 'none';
		links = document.getElementsByClassName ('restore-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = '';
	}

	function showNonBreakingChanges ()
	{
		var elements = document.querySelectorAll ('[data-original-display]');
		var i;
		for (i = 0; i < elements.length; i++) {
			var el = elements [i];
			el.style.display = el.getAttribute ('data-original-display');
		}

		var links = document.getElementsByClassName ('hide-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = '';
		links = document.getElementsByClassName ('restore-nonbreaking');
		for (i = 0; i < links.length; i++)
			links [i].style.display = 'none';
	}
</script>");
		}

		public override void BeginAssembly (TextWriter output)
		{
			output.WriteLine ($"<h1>{State.Assembly}.dll</h1>");
			if (!State.IgnoreNonbreaking) {
				output.WriteLine ("<a href='javascript: hideNonBreakingChanges (); ' class='hide-nonbreaking'>Hide non-breaking changes</a>");
				output.WriteLine ("<a href='javascript: showNonBreakingChanges (); ' class='restore-nonbreaking' style='display: none;'>Show non-breaking changes</a>");
				output.WriteLine ("<br/>");
			}
			output.WriteLine ("<div data-is-topmost>");
		}

		public override void EndAssembly (TextWriter output)
		{
			output.WriteLine ("</div> <!-- end topmost div -->");
			output.WriteLine ("</div>");
		}

		public override void BeginNamespace (TextWriter output, string action)
		{
			output.WriteLine ($"<!-- start namespace {State.Namespace} --> <div>");
			output.WriteLine ($"<h2>{action}Namespace {State.Namespace}</h2>");
		}

		public override void EndNamespace (TextWriter output)
		{
			output.WriteLine ($"</div> <!-- end namespace {State.Namespace} -->");
		}

		public override void BeginTypeAddition (TextWriter output)
		{
			output.WriteLine ($"<div> <!-- start type {State.Type} -->");
			output.WriteLine ($"<h3>New Type {State.Namespace}.{State.Type}</h3>");
			output.WriteLine ("<pre class='added' data-is-non-breaking>");
		}

		public override void EndTypeAddition (TextWriter output)
		{
			output.WriteLine ("</pre>");
			output.WriteLine ($"</div> <!-- end type {State.Type} -->");
		}

		public override void BeginTypeModification (TextWriter output)
		{
			output.WriteLine ($"<!-- start type {State.Type} --> <div>");
			output.WriteLine ($"<h3>Type Changed: {State.Namespace}.{State.Type}</h3>");
		}

		public override void EndTypeModification (TextWriter output)
		{
			output.WriteLine ($"</div> <!-- end type {State.Type} -->");
		}

		public override void BeginTypeRemoval (TextWriter output)
		{
			output.Write ($"<h3>Removed Type <span class='breaking' data-is-breaking>{State.Namespace}.{State.Type}</span></h3>");
		}

		public override void BeginMemberAddition (TextWriter output, IEnumerable<XElement> list, MemberComparer member)
		{
			output.WriteLine ("<div>");
			if (State.BaseType == "System.Enum") {
				output.WriteLine ("<p>Added value{0}:</p>", list.Count () > 1 ? "s" : String.Empty);
				output.WriteLine ("<pre class='added' data-is-non-breaking>");
			} else {
				output.WriteLine ("<p>Added {0}:</p>", list.Count () > 1 ? member.GroupName : member.ElementName);
				output.WriteLine ("<pre>");
			}
			State.Indent++;
		}

		public override void AddMember (TextWriter output, MemberComparer member, bool isInterfaceBreakingChange, string obsolete, string description)
		{
			output.Write ("<span class='added added-{0} {1}' {2}>", member.ElementName, isInterfaceBreakingChange ? "breaking" : string.Empty, isInterfaceBreakingChange ? "data-is-breaking" : "data-is-non-breaking");
			output.Write ($"{obsolete}{description}");
			output.WriteLine ("</span>");
		}

		public override void EndMemberAddition (TextWriter output)
		{
			State.Indent--;
			output.WriteLine ("</pre>");
			output.WriteLine ("</div>");
		}

		public override void BeginMemberModification (TextWriter output, string sectionName)
		{
			output.WriteLine ($"<p>{sectionName}:</p>");
			output.WriteLine ("<pre>");
		}

		public override void EndMemberModification (TextWriter output)
		{
			output.WriteLine ("</pre>");
		}

		public override void BeginMemberRemoval (TextWriter output, IEnumerable<XElement> list, MemberComparer member)
		{
			if (State.BaseType == "System.Enum") {
				output.WriteLine ("<p>Removed value{0}:</p>", list.Count () > 1 ? "s" : String.Empty);
				output.WriteLine ("<pre class='removed' data-is-breaking>");
			} else {
				output.WriteLine ("<p>Removed {0}:</p>", list.Count () > 1 ? member.GroupName : member.ElementName);
				output.WriteLine ("<pre>");
			}
			State.Indent++;
		}

		public override void RemoveMember (TextWriter output, MemberComparer member, bool breaking, string obsolete, string description)
		{
			Indent (output);
			output.Write ("<span class='removed removed-{0} {2}' {1}>", member.ElementName, breaking ? "data-is-breaking" : "data-is-non-breaking", breaking ? "breaking" : string.Empty);
			if (obsolete.Length > 0) {
				output.Write (obsolete);
				Indent (output);
			}
			output.Write (description);
			output.WriteLine ("</span>");
		}

		public override void RenderObsoleteMessage (StringBuilder output, MemberComparer member, string description, string optionalObsoleteMessage)
		{
			output.Append ($"<span class='obsolete obsolete-{member.ElementName}' data-is-non-breaking>");
			output.Append ("[Obsolete (");
			if (!String.IsNullOrEmpty (optionalObsoleteMessage))
				output.Append ('"').Append (optionalObsoleteMessage).Append ('"');
			output.AppendLine (")]");
			output.Append (description);
			output.Append ("</span>");
		}

		public override void EndMemberRemoval (TextWriter output)
		{
			State.Indent--;
			output.WriteLine ("</pre>");;
		}

		public override void DiffAddition (StringBuilder output, string text, bool breaking)
		{
			output.Append ("<span class='added ");
			if (breaking)
				output.Append ("added-breaking-inline");
			output.Append ("'>");
			output.Append (text);
			output.Append ("</span>");
		}

		public override void DiffModification (StringBuilder output, string old, string @new, bool breaking)
		{
			if (old.Length > 0)
				DiffRemoval (output, old, breaking);
			if (old.Length > 0 && @new.Length > 0)
				output.Append (' ');
			if (@new.Length > 0)
				DiffAddition (output, @new, false);
		}

		public override void DiffRemoval (StringBuilder output, string text, bool breaking)
		{
			output.Append ("<span class='removed removed-inline ");
			if (breaking)
				output.Append ("removed-breaking-inline");
			output.Append ("'>");
			output.Append (text);
			output.Append ("</span>");
		}

		public override void Diff (TextWriter output, ApiChange apichange)
		{
			output.Write ("<div {0}>", apichange.Breaking ? "data-is-breaking" : "data-is-non-breaking");
			foreach (var line in apichange.Member.ToString ().Split (new[] { Environment.NewLine }, 0)) {
				output.Write ('\t');
				output.WriteLine (line);
			}
			output.Write ("</div>");
		}
	}
}
