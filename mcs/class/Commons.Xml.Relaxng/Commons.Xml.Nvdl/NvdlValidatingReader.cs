using System;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Xml;
using Commons.Xml;

namespace Commons.Xml.Nvdl
{
	public class NvdlValidatingReader : XmlDefaultReader
	{
		NvdlDispatcher dispatcher;

		public NvdlValidatingReader (XmlReader reader, NvdlRules rules)
			: this (reader, rules, new XmlUrlResolver ())
		{
		}

		public NvdlValidatingReader (XmlReader reader, NvdlRules rules, XmlResolver resolver)
			: this (reader, rules, resolver, Nvdl.DefaultConfig)
		{
		}

		public NvdlValidatingReader (XmlReader reader, NvdlRules rules,
			XmlResolver resolver, NvdlConfig config)
			: base (reader)
		{
			dispatcher = new NvdlDispatcher (new SimpleRules (
				new NvdlCompileContext (
				rules, config, resolver)), this);
		}

		internal NvdlValidatingReader (XmlReader reader, SimpleRules rules)
			: base (reader)
		{
			dispatcher = new NvdlDispatcher (rules, this);
		}

		public event NvdlMessageEventHandler ActionStarted;

		internal void OnMessage (ListDictionary messages)
		{
			if (messages == null)
				return;
			string message = messages [Thread.CurrentThread.CurrentCulture.Name] as string;
			if (message == null)
				message = messages [String.Empty] as string;
			if (message == null)
				return;
			if (ActionStarted != null)
				ActionStarted (this, new NvdlMessageEventArgs (message));
		}

		// validation.

		public override bool Read ()
		{
			if (!Reader.Read ())
				return false;
			switch (Reader.NodeType) {
			case XmlNodeType.Element:
				dispatcher.StartElement ();
				break;
			case XmlNodeType.EndElement:
				dispatcher.EndElement ();
				break;
			case XmlNodeType.Text:
			case XmlNodeType.CDATA:
			case XmlNodeType.SignificantWhitespace:
				dispatcher.Text ();
				break;
			case XmlNodeType.Whitespace:
				dispatcher.Whitespace ();
				break;
			}
			return true;
		}
	}
}
