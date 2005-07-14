using System;
using System.Windows;
using System.Windows.Serialization;

namespace Xaml.TestVocab.Console {
	public delegate string Filter(string s);
	
	public class ConsoleWriter : DependencyObject, IAddChild, IConsoleAction {
		string text;
		private Filter filter;

		public ConsoleWriter()
		{
			text = "";
		}
		
		public ConsoleWriter(ConsoleValue text)
		{
			this.text = text.Value;
		}

		public string Text {
			get { return text; }
			set { text = value; }
		}
		public Filter Filter {
			get { return Filter; }
			set { filter = value; }
		}

		public void AddText(string text)
		{
			this.text += text;
		}

		public void AddChild(Object o)
		{
			this.text += ((ConsoleValue)o).Value;
		}

		
		public void Run()
		{
			Filter f = filter;
			string s = text;
			// apply filter, if it exists
			if (f != null)
				s = f(s);
			System.Console.WriteLine(s);
		}
	}
}
