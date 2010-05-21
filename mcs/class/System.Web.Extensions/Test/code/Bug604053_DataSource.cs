using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Bug604053.Prueba {
	public class Data {
		public int M1 { get; set; }
		public string M2 { get; set; }
		public Data(int m1, string m2) {
			M1 = m1;
			M2 = m2;
		}
	}
	[DataObject(true)]
	public class DataSource {
		public Data[] Retrieve() {
			Data[] data = new Data[10];
			for(int i = 0; i < 10; i++) {
				data[i] = new Data(i, i.ToString());
			}
			return data;
		}
	}
}
