using Xaml.TestVocab.Console;
using System.Xml;
using System.Windows.Serialization;
using Mono.Windows.Serialization;

class RuntimeTest {
	public static void Main(string[] args) {
		ConsoleApp c = (ConsoleApp)ObjectWriter.Parse(new XmlTextReader("runtimetest.xaml"));

		c.Run();
	}
}
