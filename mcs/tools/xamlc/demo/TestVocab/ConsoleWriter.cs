using System.Windows;

namespace Xaml.TestVocab.Console {
	public class ConsoleWriter : DependencyObject {
		string text;

		public ConsoleWriter(string text)
		{
			this.text = text;
		}

		public string Text {
			get { return text; }
			set { text = value; }
		}

		
		public void Run()
		{
			System.Console.WriteLine(text);
		}
	}
}
