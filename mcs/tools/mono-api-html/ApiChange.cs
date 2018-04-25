using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Xamarin.ApiDiff
{
	public class ApiChange
	{
		public string Header;
		public StringBuilder Member = new StringBuilder ();
		public bool Breaking;
		public bool AnyChange;
		public bool HasIgnoredChanges;
		public string SourceDescription;

		public ApiChange (string sourceDescription)
		{
			SourceDescription = sourceDescription;
		}

		public ApiChange Append (string text)
		{
			Member.Append (text);
			return this;
		}

		public ApiChange AppendAdded (string text, bool breaking = false)
		{
			Formatter.Current.DiffAddition (Member, text, breaking);
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}

		public ApiChange AppendRemoved (string text, bool breaking = true)
		{
			Formatter.Current.DiffRemoval (Member, text, breaking);
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}

		public ApiChange AppendModified (string old, string @new, bool breaking = true)
		{
			Formatter.Current.DiffModification (Member, old, @new, breaking);
			Breaking |= breaking;
			AnyChange = true;
			return this;
		}
	}

	public class ApiChanges : Dictionary<string, List<ApiChange>> {
		public void Add (XElement source, XElement target, ApiChange change)
		{
			if (!change.AnyChange) {
				// This is most likely because the rendering doesn't take into account something that's different (solution: fix rendering).
				if (!change.HasIgnoredChanges) {
					var isField = source.Name.LocalName == "field";
					if (isField) {
						Console.WriteLine ("Comparison resulting in no changes (src: {2} dst: {3}) :\n{0}\n{1}\n\n", source.ToString (), target.ToString (), source.GetFieldAttributes (), target.GetFieldAttributes ());
					} else {
						Console.WriteLine ("Comparison resulting in no changes (src: {2} dst: {3}) :\n{0}\n{1}\n\n", source.ToString (), target.ToString (), source.GetMethodAttributes (), target.GetMethodAttributes ());
					}
				}
				return;
			}

			var changeDescription = $"{State.Namespace}.{State.Type}: {change.Header}: {change.SourceDescription}";
			State.LogDebugMessage ($"Possible -r value: {changeDescription}");
			if (State.IgnoreRemoved.Any (re => re.IsMatch (changeDescription)))
				return;

			List<ApiChange> list;
			if (!TryGetValue (change.Header, out list)) {
				list = new List<ApiChange> ();
				base.Add (change.Header, list);
			}
			list.Add (change);
		}
	}
}

