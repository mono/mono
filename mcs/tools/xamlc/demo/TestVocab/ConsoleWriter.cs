using System;
using System.Windows;
using System.Windows.Serialization;

namespace Xaml.TestVocab.Console {
	public class ConsoleWriter : DependencyObject, IAddChild, IConsoleAction {
		string text;

		public ConsoleWriter()
		{
			text = "";
		}
		
		public ConsoleWriter(string text)
		{
			this.text = text;
		}

		public string Text {
			get { return text; }
			set { text = value; }
		}

		public void AddText(string text)
		{
			this.text += text;
		}

		public void AddChild(Object o)
		{
			throw new NotImplementedException();
		}

		
		public void Run()
		{
			System.Console.WriteLine(text);
		}
	}
}
