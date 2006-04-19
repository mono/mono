using System;
using System.Collections;
using System.Xml;
using Commons.Xml.Relaxng;

namespace Commons.Xml.Nvdl
{
	public class NvdlReader
	{
		public static NvdlRules Read (XmlReader reader)
		{
			return new NvdlReader (reader).ReadRules ();
		}

		XmlReader reader;
		IXmlLineInfo lineInfo;
		XmlDocument doc = new XmlDocument ();

		private NvdlReader (XmlReader reader)
		{
			// FIXME: use .rnc validation.
			this.reader = reader;
			this.lineInfo = reader as IXmlLineInfo;
			reader.MoveToContent ();
		}

		private void FillForeignAttribute (NvdlAttributable el)
		{
			if (!reader.MoveToFirstAttribute ())
				return;
			do {
				if (reader.NamespaceURI == "")
					continue;
				XmlAttribute a = doc.CreateAttribute (
						reader.Prefix,
						reader.LocalName,
						reader.NamespaceURI);
				a.Value = reader.Value;
				el.Foreign.Add (a);
			} while (reader.MoveToNextAttribute ());
			reader.MoveToElement ();
		}

		private void FillNonXmlAttributes (NvdlMessage el)
		{
			if (!reader.MoveToFirstAttribute ())
				return;
			do {
				if (reader.NamespaceURI == Nvdl.XmlNamespaceUri)
					continue;
				XmlAttribute a = doc.CreateAttribute (
						reader.Prefix,
						reader.LocalName,
						reader.NamespaceURI);
				a.Value = reader.Value;
				el.ForeignAttributes.Add (a);
			} while (reader.MoveToNextAttribute ());
			reader.MoveToElement ();
		}

		private void FillLocation (NvdlElementBase el)
		{
			el.SourceUri = reader.BaseURI;
			if (lineInfo != null) {
				el.LineNumber = lineInfo.LineNumber;
				el.LinePosition = lineInfo.LinePosition;
			}
		}

		private NvdlRuleTarget ParseMatch (string s)
		{
			if (s == null)
				return NvdlRuleTarget.None;
			if (s.IndexOf ("elements") >= 0)
				return (s.IndexOf ("attributes") >= 0) ?
					NvdlRuleTarget.Both :
					NvdlRuleTarget.Elements;
			else
				return (s.IndexOf ("attributes") >= 0) ?
					NvdlRuleTarget.Attributes :
					NvdlRuleTarget.None;
		}

		private NvdlRules ReadRules ()
		{
			NvdlRules el = new NvdlRules ();
			FillLocation (el);
			el.SchemaType = reader.GetAttribute ("schemaType");
			el.StartMode = reader.GetAttribute ("startMode");
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				switch (reader.LocalName) {
				case "mode":
					el.Modes.Add (ReadMode ());
					break;
				case "namespace":
					el.Rules.Add (ReadNamespace ());
					break;
				case "anyNamespace":
					el.Rules.Add (ReadAnyNamespace ());
					break;
				case "trigger":
					el.Triggers.Add (ReadTrigger ());
					break;
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private NvdlTrigger ReadTrigger ()
		{
			NvdlTrigger el = new NvdlTrigger ();
			FillLocation (el);
			el.NS = reader.GetAttribute ("ns");
			el.NameList = reader.GetAttribute ("nameList");
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private NvdlMode ReadMode ()
		{
			NvdlMode el = new NvdlMode ();
			FillLocation (el);
			el.Name = reader.GetAttribute ("name");
			ReadModeCommon (el);
			return el;
		}

		private NvdlIncludedMode ReadIncludedMode ()
		{
			NvdlIncludedMode el = new NvdlIncludedMode ();
			FillLocation (el);
			el.Name = reader.GetAttribute ("name");
			ReadModeCommon (el);
			return el;
		}

		private NvdlNestedMode ReadNestedMode ()
		{
			NvdlNestedMode el = new NvdlNestedMode ();
			FillLocation (el);
			ReadModeCommon (el);
			return el;
		}

		private void ReadModeCommon (NvdlModeBase el)
		{
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				switch (reader.LocalName) {
				case "mode":
					el.IncludedModes.Add (ReadIncludedMode ());
					break;
				case "namespace":
					el.Rules.Add (ReadNamespace ());
					break;
				case "anyNamespace":
					el.Rules.Add (ReadAnyNamespace ());
					break;
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return;
		}

		private NvdlAnyNamespace ReadAnyNamespace ()
		{
			NvdlAnyNamespace el = new NvdlAnyNamespace ();
			FillLocation (el);
			el.Match = ParseMatch (reader.GetAttribute ("match"));
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				ReadAction (el);
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private NvdlNamespace ReadNamespace ()
		{
			NvdlNamespace el = new NvdlNamespace ();
			FillLocation (el);
			el.NS = reader.GetAttribute ("ns");
			el.Wildcard = reader.GetAttribute ("wildCard");
			el.Match = ParseMatch (reader.GetAttribute ("match"));
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				ReadAction (el);
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private void ReadAction (NvdlRule el)
		{
			switch (reader.LocalName) {
			case "cancelNestedActions":
				el.Actions.Add (ReadCancelAction ());
				break;
			case "validate":
				el.Actions.Add (ReadValidate ());
				break;
			case "allow":
				el.Actions.Add (ReadAllow ());
				break;
			case "reject":
				el.Actions.Add (ReadReject ());
				break;
			case "attach":
				el.Actions.Add (ReadAttach ());
				break;
			case "attachPlaceHolder":
				el.Actions.Add (ReadAttachPlaceholder ());
				break;
			case "unwrap":
				el.Actions.Add (ReadUnwrap ());
				break;
			default:
				throw new NotSupportedException ();
			}
		}

		private NvdlCancelAction ReadCancelAction ()
		{
			NvdlCancelAction el = new NvdlCancelAction ();
			FillLocation (el);
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private NvdlValidate ReadValidate ()
		{
			NvdlValidate el = new NvdlValidate ();
			NvdlModeUsage mu = new NvdlModeUsage ();
			el.ModeUsage = mu;
			FillLocation (el);
			el.SchemaType = reader.GetAttribute ("schemaType");
			el.SimpleMessage = reader.GetAttribute ("message");
			el.SchemaUri = reader.GetAttribute ("schema");
			mu.UseMode = reader.GetAttribute ("useMode");
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				switch (reader.LocalName) {
				case "message":
					el.Messages.Add (ReadMessage ());
					break;
				case "option":
					el.Options.Add (ReadOption ());
					break;
				case "schema":
					el.SchemaBody = (XmlElement) doc.ReadNode (reader);
					break;
				case "mode":
					mu.NestedMode = ReadNestedMode ();
					break;
				case "context":
					mu.Contexts.Add (ReadContext ());
					break;
				default:
					throw new NotSupportedException ();
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private NvdlAllow ReadAllow ()
		{
			NvdlAllow el = new NvdlAllow ();
			ReadCommonActionContent (el);
			return el;
		}

		private NvdlReject ReadReject ()
		{
			NvdlReject el = new NvdlReject ();
			ReadCommonActionContent (el);
			return el;
		}

		private NvdlAttach ReadAttach ()
		{
			NvdlAttach el = new NvdlAttach ();
			ReadCommonActionContent (el);
			return el;
		}

		private NvdlAttachPlaceholder ReadAttachPlaceholder ()
		{
			NvdlAttachPlaceholder el = new NvdlAttachPlaceholder ();
			ReadCommonActionContent (el);
			return el;
		}

		private NvdlUnwrap ReadUnwrap ()
		{
			NvdlUnwrap el = new NvdlUnwrap ();
			ReadCommonActionContent (el);
			return el;
		}

		private void ReadCommonActionContent (NvdlNoCancelAction el)
		{
			FillLocation (el);
			NvdlModeUsage mu = new NvdlModeUsage ();
			el.ModeUsage = mu;
			FillLocation (el);
			el.SimpleMessage = reader.GetAttribute ("message");
			mu.UseMode = reader.GetAttribute ("useMode");
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				switch (reader.LocalName) {
				case "mode":
					mu.NestedMode = ReadNestedMode ();
					break;
				case "message":
					el.Messages.Add (ReadMessage ());
					break;
				case "context":
					mu.Contexts.Add (ReadContext ());
					break;
				default:
					throw new NotSupportedException ();
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
		}

		private NvdlMessage ReadMessage ()
		{
			NvdlMessage el = new NvdlMessage ();
			FillLocation (el);
			el.XmlLang = reader.GetAttribute ("lang", Nvdl.XmlNamespaceUri);
			FillNonXmlAttributes (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				el.Text = "";
			}
			else
				el.Text = reader.ReadElementString ();
			return el;
		}

		private NvdlOption ReadOption ()
		{
			NvdlOption el = new NvdlOption ();
			FillLocation (el);
			el.Name = reader.GetAttribute ("name");
			el.Arg = reader.GetAttribute ("arg");
			el.MustSupport = reader.GetAttribute ("mustSupport");
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}

		private NvdlContext ReadContext ()
		{
			NvdlContext el = new NvdlContext ();
			FillLocation (el);
			el.Path = reader.GetAttribute ("path");
			el.UseMode = reader.GetAttribute ("useMode");
			FillForeignAttribute (el);
			if (reader.IsEmptyElement) {
				reader.Skip ();
				return el;
			}
			reader.Read ();
			do {
				reader.MoveToContent ();
				if (reader.NodeType == XmlNodeType.EndElement)
					break;
				if (reader.NamespaceURI != Nvdl.Namespace) {
					el.Foreign.Add (doc.ReadNode (reader));
					continue;
				}
				switch (reader.LocalName) {
				case "mode":
					el.NestedMode = ReadNestedMode ();
					break;
				default:
					throw new NotSupportedException ();
				}
			} while (!reader.EOF);
			if (!reader.EOF)
				reader.Read ();
			return el;
		}
	}
}

