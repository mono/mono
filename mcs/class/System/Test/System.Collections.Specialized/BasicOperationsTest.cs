// created on 7/21/2001 at 2:36 PM
//
// Author: Martin Willemoes Hansen <mwh@sysrq.dk>
// 
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.Specialized {

	[TestFixture]
	public class BasicOperationsTest : Assertion {

		protected NameValueCollection nvc;
		private static Random rnd;

		[SetUp]
		public void GetReady() 
		{
			nvc = new NameValueCollection();
			rnd=new Random();
		}

		private void SetDefaultData() 
		{
			nvc.Clear();
			nvc.Add("k1","this");
			nvc.Add("k2","test");
			nvc.Add("k3","is");
			nvc.Add("k4","silly");
		}

		private static string FormatForPrinting (NameValueCollection nv)
		{
			if (nv==null) 
				return null;
			int max = nv.Count;
			StringBuilder sb = new StringBuilder("-\t-Key-\t-Value-\n");
			for (int i=0; i<max; i++){
				
				sb.Append("\t"+nv.GetKey(i)+"\t"+nv[i]+"\n");
			}
			return sb.ToString();
		}

		[Test]
		public void AddRemoveClearSetGet() 
		{
			nvc.Clear();
			Assert(nvc.Count==0&& !nvc.HasKeys());

			SetDefaultData();
			Assert(nvc.Count==4);
			Assert("Get operation returns wrong result.\n"+FormatForPrinting(nvc),(nvc.Get(0).Equals("this"))&&(nvc.Get("k1").Equals("this")));


			nvc.Add("k2","programmer");
			Assert(nvc["k2"].Equals("test,programmer"));

			nvc["k2"]="project";
			nvc.Add("k2","project");
			Assert(nvc.Count==4);
			Assert("Wrong effect of add(samekey,samevalue)\n"+FormatForPrinting(nvc), nvc["k2"].Equals("project,project"));
			// TODO: add Remove test
			nvc.Remove("k4");
			Assert("wrong nvc.Count="+nvc.Count,nvc.Count==3);
			Assert(nvc["k4"]==null);
			
			NameValueCollection nvc1 = new NameValueCollection();
			nvc1["k1"]="these";
			nvc1["k5"]="!";
			nvc.Add(nvc1);
			Assert(FormatForPrinting(nvc)+"Count is wrong after Add(nvc1) Count="+nvc.Count,nvc.Count==4);
			Assert("Values are wrong after Add(nvc1)",(nvc["k1"].Equals("this,these"))&&(nvc["k5"].Equals("!")));
			
			nvc.Set("k3","accomplished");
			Assert("Wrong result of Set operation",nvc["k3"].Equals("accomplished"));
			
		}
		
		[Test]
		public void GetKeyGetValues()
		{
			SetDefaultData();
			Assert(nvc.GetKey(0).Equals("k1"));
			string[] values = nvc.GetValues(0);
			Assert(values[0].Equals("this"));
			
		}
		
		[Test]
		public void CopyTo() {
			SetDefaultData();
			string[] entries=new string[nvc.Count];
			nvc.CopyTo(entries,0);
			//Message(FormatForPrinting(nvc));
			//Assert("Not an entry.",entries[0] is DictionaryEntry);
		}

		[Test]
		public void UnderHeavyLoad() {
			
			//TODO: add memory and time measurement
			
			nvc.Clear();
			int max=10000;
			String[] cache=new String[max*2];
			int n=0;

			for (int i=0;i<max;i++) {
				int id=rnd.Next()&0xFFFF;
				String key=""+id+"-key-"+id;
				String val="value-"+id;
				if (nvc[key]==null) {
					nvc[key]=val;
					cache[n]=key;
					cache[n+max]=val;
					n++;
				}
			}

			Assert(nvc.Count==n);

			for (int i=0;i<n;i++) {
				String key=cache[i];
				String val=nvc[key] as String;
				String err="nvc[\""+key+"\"]=\""+val+
				      "\", expected \""+cache[i+max]+"\"";
				Assert(err,val!=null && val.Equals(cache[i+max]));
			}

			int r1=(n/3);
			int r2=r1+(n/5);

			for (int i=r1;i<r2;i++) {
				nvc.Remove(cache[i]);
			}


			for (int i=0;i<n;i++) {
				if (i>=r1 && i<r2) {
					Assert(nvc[cache[i]]==null);
				} else {
					String key=cache[i];
					String val=nvc[key] as String;
					String err="ht[\""+key+"\"]=\""+val+
					      "\", expected \""+cache[i+max]+"\"";
					Assert(err,val!=null && val.Equals(cache[i+max]));
				}
			}

		}

	}
}

