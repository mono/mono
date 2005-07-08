using Xaml.TestVocab.Console;
using System.Windows.Serialization;
using Mono.Windows.Serialization;

class RuntimeTest {
	public static void Main(string[] args) {
		ObjectWriter ow = new ObjectWriter();
		XamlParser r = new XamlParser("runtimetest.xaml", ow);
		r.Parse();

		ConsoleApp c = (ConsoleApp)(ow.instance);
		c.Run();
	}
}
