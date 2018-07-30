// 
// Authors
//    Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013-2014 Xamarin Inc. http://www.xamarin.com
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Mono.ApiTools {

	abstract class MemberComparer : Comparer {

		// true if this is the first element being added or removed in the group being rendered
		protected bool first;

		public MemberComparer (State state)
			: base (state)
		{
		}

		public abstract string GroupName { get; }
		public abstract string ElementName { get; }

		protected virtual bool IsBreakingRemoval (XElement e)
		{
			return !e.Elements ("attributes").SelectMany (a => a.Elements ("attribute")).Any (c => c.Attribute ("name")?.Value == "System.ObsoleteAttribute");
		}

		public void Compare (XElement source, XElement target)
		{
			var s = source.Element (GroupName);
			var t = target.Element (GroupName);
			if (XNode.DeepEquals (s, t))
				return;

			if (s == null) {
				Add (t.Elements (ElementName));
			} else if (t == null) {
				Remove (s.Elements (ElementName));
			} else {
				Compare (s.Elements (ElementName), t.Elements (ElementName));
			}
		}

		public override void SetContext (XElement current)
		{
		}

		string GetContainingType (XElement el)
		{
			return el.Ancestors ("class").First ().Attribute ("type").Value;
		}

		bool IsInInterface (XElement el)
		{
			return GetContainingType (el) == "interface";
		}

		public XElement Source { get; set; }

		public virtual bool Find (XElement e)
		{
			return e.GetAttribute ("name") == Source.GetAttribute ("name");
		}

		XElement Find (IEnumerable<XElement> target)
		{
			return State.Lax ? target.FirstOrDefault (Find) : target.SingleOrDefault (Find);
		}

		public override void Compare (IEnumerable<XElement> source, IEnumerable<XElement> target)
		{
			removed.Clear ();
			modified.Clear ();

			foreach (var s in source) {
				SetContext (s);
				Source = s;
				var t = Find (target);
				if (t == null) {
					// not in target, it was removed
					removed.Add (s);
				} else {
					t.Remove ();
					// possibly modified
					if (Equals (s, t, modified))
						continue;

					Modified (s, t, modified);
				}
			}
			// delayed, that way we show "Modified", "Added" and then "Removed"
			Remove (removed);

			Modify (modified);

			// remaining == newly added in target
			Add (target);
		}

		void Add (IEnumerable<XElement> elements)
		{
			bool a = false;
			foreach (var item in elements) {
				var memberDescription = $"{State.Namespace}.{State.Type}: Added {GroupName}: {GetDescription (item)}";
				State.LogDebugMessage ($"Possible -a value: {memberDescription}");
				SetContext (item);
				if (State.IgnoreAdded.Any (re => re.IsMatch (memberDescription)))
					continue;
				if (!a) {
					Formatter.BeginMemberAddition (Output, elements, this);
					a = true;
				}
				Added (item, false);
			}
			if (a)
				Formatter.EndMemberAddition (Output);
		}

		void Modify (ApiChanges modified)
		{
			foreach (var changes in modified) {
				if (State.IgnoreNonbreaking && changes.Value.All (c => !c.Breaking))
					continue;
				Formatter.BeginMemberModification (Output, changes.Key);
				foreach (var element in changes.Value) {
					if (State.IgnoreNonbreaking && !element.Breaking)
						continue;
					Formatter.Diff (Output, element);
				}
				Formatter.EndMemberModification (Output);
			}
		}

		void Remove (IEnumerable<XElement> elements)
		{
			bool r = false;
			foreach (var item in elements) {
				var memberDescription = $"{State.Namespace}.{State.Type}: Removed {GroupName}: {GetDescription (item)}";
				State.LogDebugMessage ($"Possible -r value: {memberDescription}");
				if (State.IgnoreRemoved.Any (re => re.IsMatch (memberDescription)))
					continue;
				SetContext (item);
				if (State.IgnoreNonbreaking && !IsBreakingRemoval (item))
					continue;
				if (!r) {
					Formatter.BeginMemberRemoval (Output, elements, this);
					first = true;
					r = true;
				}
				Removed (item);
			}
			if (r)
				Formatter.EndMemberRemoval (Output);
		}
			
		public abstract string GetDescription (XElement e);

		protected StringBuilder GetObsoleteMessage (XElement e)
		{
			var sb = new StringBuilder ();
			string o = e.GetObsoleteMessage ();
			if (o != null) {
				sb.Append ("[Obsolete");
				if (o.Length > 0)
					sb.Append (" (\"").Append (o).Append ("\")");
				sb.AppendLine ("]");
			}
			return sb;
		}

		public override bool Equals (XElement source, XElement target, ApiChanges changes)
		{
			RenderAttributes (source, target, changes);

			// We don't want to compare attributes.
			RemoveAttributes (source);
			RemoveAttributes (target);

			return base.Equals (source, target, changes);
		}

		public override void Added (XElement target, bool wasParentAdded)
		{
			var o = GetObsoleteMessage (target);
			if (!first && (o.Length > 0))
				Output.WriteLine ();
			Indent ();
			bool isInterfaceBreakingChange = !wasParentAdded && IsInInterface (target);
			Formatter.AddMember (Output, this, isInterfaceBreakingChange, o.ToString (), GetDescription (target));
			first = false;
		}

		public override void Modified (XElement source, XElement target, ApiChanges change)
		{
		}

		public override void Removed (XElement source)
		{
			var o = GetObsoleteMessage (source);
			if (!first && (o.Length > 0))
				Output.WriteLine ();

			bool is_breaking = IsBreakingRemoval (source);

			Formatter.RemoveMember (Output, this, is_breaking, o.ToString (), GetDescription (source));
			first = false;
		}

		string RenderGenericParameter (XElement gp)
		{
			var sb = new StringBuilder ();
			sb.Append (gp.GetTypeName ("name", State));

			var constraints = gp.DescendantList ("generic-parameter-constraints", "generic-parameter-constraint");
			if (constraints != null && constraints.Count > 0) {
				sb.Append (" : ");
				for (int i = 0; i < constraints.Count; i++) {
					if (i > 0)
						sb.Append (", ");
					sb.Append (constraints [i].GetTypeName ("name", State));
				}
			}
			return sb.ToString ();
		}

		protected void RenderGenericParameters (XElement source, XElement target, ApiChange change)
		{
			var src = source.DescendantList ("generic-parameters", "generic-parameter");
			var tgt = target.DescendantList ("generic-parameters", "generic-parameter");
			var srcCount = src == null ? 0 : src.Count;
			var tgtCount = tgt == null ? 0 : tgt.Count;

			if (srcCount == 0 && tgtCount == 0)
				return;

			change.Append (Formatter.LesserThan);
			for (int i = 0; i < System.Math.Max (srcCount, tgtCount); i++) {
				if (i > 0)
					change.Append (", ");
				if (i >= srcCount) {
					change.AppendAdded (RenderGenericParameter (tgt [i]), true);
				} else if (i >= tgtCount) {
					change.AppendRemoved (RenderGenericParameter (src [i]), true);
				} else {
					var srcName = RenderGenericParameter (src [i]);
					var tgtName = RenderGenericParameter (tgt [i]);

					if (srcName != tgtName) {
						change.AppendModified (srcName, tgtName, true);
					} else {
						change.Append (srcName);
					}
					}
				}
			change.Append (Formatter.GreaterThan);
		}

		protected string FormatValue (string type, string value)
		{
			if (value == null)
				return "null";

			if (type == "string")
				return "\"" + value + "\"";
			else if (type == "bool") {
				switch (value) {
				case "True":
					return "true";
				case "False":
					return "false";
				default:
					return value;
				}
			}

			return value;
		}

		protected void RenderParameters (XElement source, XElement target, ApiChange change)
		{
			var src = source.DescendantList ("parameters", "parameter");
			var tgt = target.DescendantList ("parameters", "parameter");
			var srcCount = src == null ? 0 : src.Count;
			var tgtCount = tgt == null ? 0 : tgt.Count;

			change.Append (" (");
			for (int i = 0; i < System.Math.Max (srcCount, tgtCount); i++) {
				if (i > 0)
					change.Append (", ");

				string mods_tgt = tgt [i].GetAttribute ("direction") ?? "";
				string mods_src = src [i].GetAttribute ("direction") ?? "";

				if (mods_tgt.Length > 0)
					mods_tgt = mods_tgt + " ";

				if (mods_src.Length > 0)
					mods_src = mods_src + " ";

				if (i >= srcCount) {
					change.AppendAdded (mods_tgt + tgt [i].GetTypeName ("type", State) + " " + tgt [i].GetAttribute ("name"), true);
				} else if (i >= tgtCount) {
					change.AppendRemoved (mods_src + src [i].GetTypeName ("type", State) + " " + src [i].GetAttribute ("name"), true);
				} else {
					var paramSourceType = src [i].GetTypeName ("type", State);
					var paramTargetType = tgt [i].GetTypeName ("type", State);

					var paramSourceName = src [i].GetAttribute ("name");
					var paramTargetName = tgt [i].GetAttribute ("name");

					if (mods_src != mods_tgt) {
						change.AppendModified (mods_src, mods_tgt, true);
					} else {
						change.Append (mods_src);
					}

					if (paramSourceType != paramTargetType) {
						change.AppendModified (paramSourceType, paramTargetType, true);
					} else {
						change.Append (paramSourceType);
					}
					change.Append (" ");
					if (!State.IgnoreParameterNameChanges && paramSourceName != paramTargetName) {
						change.AppendModified (paramSourceName, paramTargetName, true);
					} else {
						change.Append (paramSourceName);
					}

					var optSource = src [i].Attribute ("optional");
					var optTarget = tgt [i].Attribute ("optional");
					var srcValue = FormatValue (paramSourceType, src [i].GetAttribute ("defaultValue"));
					var tgtValue = FormatValue (paramTargetType, tgt [i].GetAttribute ("defaultValue"));

					if (optSource != null) {
						if (optTarget != null) {
							change.Append (" = ");
							if (srcValue != tgtValue) {
								change.AppendModified (srcValue, tgtValue, false);
							} else {
								change.Append (tgtValue);
							}
						} else {
							change.AppendRemoved (" = " + srcValue);
						}
					} else {
						if (optTarget != null)
							change.AppendAdded (" = " + tgtValue);
					}
				}
			}

			change.Append (")");
		}

		void RenderVTable (MethodAttributes source, MethodAttributes target, ApiChange change)
		{
			var srcAbstract = (source & MethodAttributes.Abstract) == MethodAttributes.Abstract;
			var tgtAbstract = (target & MethodAttributes.Abstract) == MethodAttributes.Abstract;
			var srcFinal = (source & MethodAttributes.Final) == MethodAttributes.Final;
			var tgtFinal = (target & MethodAttributes.Final) == MethodAttributes.Final;
			var srcVirtual = (source & MethodAttributes.Virtual) == MethodAttributes.Virtual;
			var tgtVirtual = (target & MethodAttributes.Virtual) == MethodAttributes.Virtual;
			var srcOverride = (source & MethodAttributes.VtableLayoutMask) != MethodAttributes.NewSlot;
			var tgtOverride = (target & MethodAttributes.VtableLayoutMask) != MethodAttributes.NewSlot;

			var srcWord = srcVirtual ? (srcOverride ? "override" : "virtual") : string.Empty;
			var tgtWord = tgtVirtual ? (tgtOverride ? "override" : "virtual") : string.Empty;
			var breaking = srcWord.Length > 0 && tgtWord.Length == 0;

			if (srcAbstract) {
				if (tgtAbstract) {
					change.Append ("abstract ");
				} else if (tgtVirtual) {
					change.AppendModified ("abstract", tgtWord, false).Append (" ");
				} else {
					change.AppendRemoved ("abstract").Append (" ");
				}
			} else {
				if (tgtAbstract) {
					change.AppendAdded ("abstract", true).Append (" ");
				} else if (srcWord != tgtWord) {
					if (!tgtFinal) {
						if (State.IgnoreVirtualChanges) {
							change.HasIgnoredChanges = true;
						} else {
							change.AppendModified (srcWord, tgtWord, breaking).Append (" ");
						}
					}
				} else if (tgtWord.Length > 0) {
					change.Append (tgtWord).Append (" ");
				} else if (srcWord.Length > 0) {
					change.AppendRemoved (srcWord, breaking).Append (" ");
				}
			}

			if (srcFinal) {
				if (tgtFinal) {
					change.Append ("final ");
				} else {
					if (srcVirtual && !tgtVirtual && State.IgnoreVirtualChanges) {
						change.HasIgnoredChanges = true;
					} else {
						change.AppendRemoved ("final", false).Append (" "); // removing 'final' is not a breaking change.
					}
				}
			} else {
				if (tgtFinal && srcVirtual) {
					change.AppendModified ("virtual", "final", true).Append (" "); // adding 'final' is a breaking change if the member was virtual
				}
			}

			if (!srcVirtual && !srcFinal && tgtVirtual && tgtFinal) {
				// existing member implements a member from a new interface
				// this would show up as 'virtual final', which is redundant, so show nothing at all.
				change.HasIgnoredChanges = true;
			}

			// Ignore non-breaking virtual changes.
			if (State.IgnoreVirtualChanges && !change.Breaking) {
				change.AnyChange = false;
				change.HasIgnoredChanges = true;
			}

			var tgtSecurity = (source & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity;
			var srcSecurity = (target & MethodAttributes.HasSecurity) == MethodAttributes.HasSecurity;

			if (tgtSecurity != srcSecurity)
				change.HasIgnoredChanges = true;

			var srcPInvoke = (source & MethodAttributes.PinvokeImpl) == MethodAttributes.PinvokeImpl;
			var tgtPInvoke = (target & MethodAttributes.PinvokeImpl) == MethodAttributes.PinvokeImpl;
			if (srcPInvoke != tgtPInvoke)
				change.HasIgnoredChanges = true;
		}

		protected string GetVisibility (MethodAttributes attr)
		{
			switch (attr) {
			case MethodAttributes.Private:
			case MethodAttributes.PrivateScope:
				return "private";
			case MethodAttributes.Assembly:
				return "internal";
			case MethodAttributes.FamANDAssem:
				return "private internal";
			case MethodAttributes.FamORAssem:
				return "protected"; // customers don't care about 'internal';
			case MethodAttributes.Family:
				return "protected";
			case MethodAttributes.Public:
				return "public";
			default:
				throw new NotImplementedException ();
			}
		}

		protected void RenderVisibility (MethodAttributes source, MethodAttributes target, ApiChange diff)
		{
			source = source & MethodAttributes.MemberAccessMask;
			target = target & MethodAttributes.MemberAccessMask;

			if (source == target) {
				diff.Append (GetVisibility (target));
			} else {
				var breaking = false;
				switch (source) {
				case MethodAttributes.Private:
				case MethodAttributes.Assembly:
				case MethodAttributes.FamANDAssem:
					break; // these are not publicly visible, thus not breaking
				case MethodAttributes.FamORAssem:
				case MethodAttributes.Family:
					switch (target) {
					case MethodAttributes.Public:
						// to public is not a breaking change
						break;
					case MethodAttributes.Family:
					case MethodAttributes.FamORAssem:
						// not a breaking change, but should still show up in diff
						break;
					default:
						// anything else is a breaking change
						breaking = true;
						break;
					}
					break;
				case MethodAttributes.Public:
				default:
					// any change from public is breaking.
					breaking = true;
					break;
				}

				diff.AppendModified (GetVisibility (source), GetVisibility (target), breaking);
			}
			diff.Append (" ");
		}

		protected void RenderStatic (MethodAttributes src, MethodAttributes tgt, ApiChange diff)
		{
			var srcStatic = (src & MethodAttributes.Static) == MethodAttributes.Static;
			var tgtStatic = (tgt & MethodAttributes.Static) == MethodAttributes.Static;

			if (srcStatic != tgtStatic) {
				if (srcStatic) {
					diff.AppendRemoved ("static", true).Append (" ");
				} else {
					diff.AppendAdded ("static", true).Append (" ");
				}
			}
		}

		protected void RenderMethodAttributes (MethodAttributes src, MethodAttributes tgt, ApiChange diff)
		{
			RenderStatic (src, tgt, diff);
			RenderVisibility (src & MethodAttributes.MemberAccessMask, tgt & MethodAttributes.MemberAccessMask, diff);
			RenderVTable (src, tgt, diff);
		}

		protected void RenderMethodAttributes (XElement source, XElement target, ApiChange diff)
		{
			RenderMethodAttributes (source.GetMethodAttributes (), target.GetMethodAttributes (), diff);
		}

		protected void RemoveAttributes (XElement element)
		{
			var srcAttributes = element.Element ("attributes");
			if (srcAttributes != null)
				srcAttributes.Remove ();

			foreach (var el in element.Elements ())
				RemoveAttributes (el);
		}

		protected void RenderAttributes (XElement source, XElement target, ApiChanges changes)
		{
			var srcObsolete = source.GetObsoleteMessage ();
			var tgtObsolete = target.GetObsoleteMessage ();

			if (srcObsolete == tgtObsolete)
				return; // nothing changed

			if (srcObsolete == null) {
				if (tgtObsolete == null)
					return; // neither is obsolete
				var change = new ApiChange (GetDescription (source), State);
				change.Header = "Obsoleted " + GroupName;
				Formatter.RenderObsoleteMessage (change.Member, this, GetDescription (target), tgtObsolete);
				change.AnyChange = true;
				changes.Add (source, target, change);
			} else if (tgtObsolete == null) {
				// Made non-obsolete. Do we care to report this?
			} else {
				// Obsolete message changed. Do we care to report this?
			}
		}

		protected void RenderName (XElement source, XElement target, ApiChange change)
		{
			var name = target.GetAttribute ("name");
			// show the constructor as it would be defined in C#
			name = name.Replace (".ctor", State.Type);

			var p = name.IndexOf ('(');
			if (p >= 0)
				name = name.Substring (0, p);

			change.Append (name);
		}

	}
}
