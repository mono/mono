using Xaml.TestVocab.Console;
using System.Xml;
using System.Windows.Serialization;

class RuntimeTest {
	public static void Main(string[] args) {
		ConsoleApp c = (ConsoleApp)Parser.LoadXml(new XmlTextReader("runtimetest.xaml"));

		c.Run();
	}
}
