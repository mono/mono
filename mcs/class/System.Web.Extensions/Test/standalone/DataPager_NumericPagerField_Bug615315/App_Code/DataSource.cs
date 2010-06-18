using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace Prueba {
	public class Data {
		public int M1 { get; set; }
		public string M2 { get; set; }
		public Data(int m1, string m2) {
			M1 = m1;
			M2 = m2;
		}
	}
	public class DataCollection : Collection<Data> { }
	[DataObject(true)]
	public class DataSource {
		private static DataCollection data = new DataCollection();
		static DataSource(){
			for(int i = 0; i < 100; i++) {
				data.Add(new Data(i, i.ToString()));
			}
		}
		public DataCollection Retrieve() {
			return data;
		}
		public void insert(int m1, string m2) {
			foreach(Data i in data) {
				if(i.M1 == m1)
					return;
			}
			data.Add(new Data(m1, m2));
		}
		public void Update(int m1, string m2, int oldM1) {
			foreach(Data i in data) {
				if(i.M1 == oldM1) {
					i.M1 = m1;
					i.M2 = m2;
				}
			}
		}
		public void Delete(int oldM1) {
			Data deleting = null;
			foreach(Data i in data) {
				if(i.M1 == oldM1) {
					deleting = i;
				}
			}
			if(deleting != null)
				data.Remove(deleting);
		}
	}
}
