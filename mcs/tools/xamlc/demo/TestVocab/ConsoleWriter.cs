using System;
using System.Windows;
using System.Windows.Serialization;

namespace Xaml.TestVocab.Console {
	public delegate string Filter(string s);
	
	public class ConsoleWriter : DependencyObject, IAddChild, IConsoleAction {
		ConsoleValue text;
		private Filter filter;

		public ConsoleWriter()
		{
			text = new ConsoleValueString("");
		}
		
		public ConsoleWriter(ConsoleValue text)
		{
			this.text = text;
		}

		public ConsoleValue Text {
			get { return text; }
			set { text = value; }
		}
		public Filter Filter {
			get { return Filter; }
			set { filter = value; }
		}

		public void AddText(string text)
		{
			this.text = new ConsoleValueAppend(this.text, new ConsoleValueString(text));
		}

		public void AddChild(Object o)
		{
			this.text = new ConsoleValueAppend(this.text, (ConsoleValue)o);
		}

		
		public void Run()
		{
			Filter f = filter;
			string s = text.Value;
			// apply filter, if it exists
			if (f != null)
				s = f(s);
			System.Console.WriteLine(s);
		}

		public override bool Equals(object o)
		{
			ConsoleWriter writer = (ConsoleWriter)o;
			return (writer.filter == filter) && (writer.text == text);
		}
		public override int GetHashCode()
		{
			return filter.GetHashCode() + text.GetHashCode();
		}
	}
}
