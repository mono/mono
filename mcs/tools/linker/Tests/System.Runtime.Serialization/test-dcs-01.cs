using System;
using System.Runtime;
using System.Collections.Generic;

public class DataContractSerializerTest {
	public static void Main (string []args) {
		var source = new List<string>();
		source.Add("a");
		source.Add("b");
		using (System.IO.MemoryStream stream = new System.IO.MemoryStream()) {
			var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(List<string>));
			serializer.WriteObject(stream, source);
			stream.Flush();
		}
	}
}
