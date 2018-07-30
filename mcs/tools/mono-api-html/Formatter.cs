// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
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
using System.Xml.Linq;
using System.Text;

namespace Mono.ApiTools {

	abstract class Formatter {

		public Formatter(State state)
		{
			State = state;
		}

		public State State { get; }

		public abstract string LesserThan { get; }
		public abstract string GreaterThan { get; }

		public abstract void BeginDocument (TextWriter output, string title);
		public virtual void EndDocument (TextWriter output)
		{
		}

		public abstract void BeginAssembly (TextWriter output);
		public virtual void EndAssembly (TextWriter output)
		{
		}

		public abstract void BeginNamespace (TextWriter output, string action = "");
		public virtual void EndNamespace (TextWriter output)
		{
		}

		public abstract void BeginTypeAddition (TextWriter output);
		public abstract void EndTypeAddition (TextWriter output);

		public abstract void BeginTypeModification (TextWriter output);
		public virtual void EndTypeModification (TextWriter output)
		{
		}

		public abstract void BeginTypeRemoval (TextWriter output);
		public virtual void EndTypeRemoval (TextWriter output)
		{
		}

		public abstract void BeginMemberAddition (TextWriter output, IEnumerable<XElement> list, MemberComparer member);
		public abstract void AddMember (TextWriter output, MemberComparer member, bool isInterfaceBreakingChange, string obsolete, string description);
		public abstract void EndMemberAddition (TextWriter output);

		public abstract void BeginMemberModification (TextWriter output, string sectionName);
		public abstract void EndMemberModification (TextWriter output);

		public abstract void BeginMemberRemoval (TextWriter output, IEnumerable<XElement> list, MemberComparer member);
		public abstract void RemoveMember (TextWriter output, MemberComparer member, bool breaking, string obsolete, string description);
		public abstract void EndMemberRemoval (TextWriter output);

		public abstract void RenderObsoleteMessage (StringBuilder output, MemberComparer member, string description, string optionalObsoleteMessage);

		public abstract void DiffAddition (StringBuilder output, string text, bool breaking);
		public abstract void DiffModification (StringBuilder output, string old, string @new, bool breaking);
		public abstract void DiffRemoval (StringBuilder output, string text, bool breaking);
		public abstract void Diff (TextWriter output, ApiChange apichange);
	}
}
