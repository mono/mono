/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Document = Monodoc.Lucene.Net.Documents.Document;
using Term = Monodoc.Lucene.Net.Index.Term;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>A remote searchable implementation. </summary>
	[Serializable]
	public class RemoteSearchable : System.MarshalByRefObject, Monodoc.Lucene.Net.Search.Searchable
	{
		
		private Monodoc.Lucene.Net.Search.Searchable local;
		
		/// <summary>Constructs and exports a remote searcher. </summary>
		public RemoteSearchable(Monodoc.Lucene.Net.Search.Searchable local):base()
		{
			this.local = local;
		}
		
		public virtual void  Search(Query query, Filter filter, HitCollector results)
		{
			local.Search(query, filter, results);
		}
		
		public virtual void  Close()
		{
			local.Close();
		}
		
		public virtual int DocFreq(Term term)
		{
			return local.DocFreq(term);
		}
		
		public virtual int MaxDoc()
		{
			return local.MaxDoc();
		}
		
		public virtual TopDocs Search(Query query, Filter filter, int n)
		{
			return local.Search(query, filter, n);
		}
		
		public virtual TopFieldDocs Search(Query query, Filter filter, int n, Sort sort)
		{
			return local.Search(query, filter, n, sort);
		}
		
		public virtual Document Doc(int i)
		{
			return local.Doc(i);
		}
		
		public virtual Query Rewrite(Query original)
		{
			return local.Rewrite(original);
		}
		
		public virtual Explanation Explain(Query query, int doc)
		{
			return local.Explain(query, doc);
		}

        public override System.Object InitializeLifetimeService()
        {
            long initialLeaseTime, sponsorshipTimeout, renewOnCallTime;

            initialLeaseTime = SupportClass.AppSettings.Get("Monodoc.Lucene.Net.Remoting.Lifetime.initialLeaseTime", -1);
            sponsorshipTimeout = SupportClass.AppSettings.Get("Monodoc.Lucene.Net.Remoting.Lifetime.sponsorshipTimeout", -1);
            renewOnCallTime = SupportClass.AppSettings.Get("Monodoc.Lucene.Net.Remoting.Lifetime.renewOnCallTime", -1);

            if ((initialLeaseTime == -1) || (sponsorshipTimeout == -1) || (renewOnCallTime == -1))
            {
                return null;
            }
            else
            {
                System.Runtime.Remoting.Lifetime.ILease lease = 
                    (System.Runtime.Remoting.Lifetime.ILease) base.InitializeLifetimeService();
                if (lease.CurrentState == System.Runtime.Remoting.Lifetime.LeaseState.Initial)
                {
                    lease.InitialLeaseTime = System.TimeSpan.FromMinutes(initialLeaseTime);
                    lease.SponsorshipTimeout = System.TimeSpan.FromMinutes(sponsorshipTimeout);
                    lease.RenewOnCallTime = System.TimeSpan.FromSeconds(renewOnCallTime);
                }
                return lease;
            }
        }

		/// <summary>Exports a searcher for the index in args[0] named
		/// "//localhost/Searchable". 
		/// </summary>
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.Runtime.Remoting.RemotingConfiguration.Configure("Monodoc.Lucene.Net.Search.RemoteSearchable.config");
			System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(new System.Runtime.Remoting.Channels.Http.HttpChannel(1099));
			// create and install a security manager
			if (false) //{{}}// if (System.getSecurityManager() == null)    // {{Aroush}} >> 'java.lang.System.getSecurityManager()'
			{
				//{{}}// System.setSecurityManager(new RMISecurityManager());   // {{Aroush}} >> 'java.lang.System.setSecurityManager()' and 'java.rmi.RMISecurityManager.RMISecurityManager()'
			}
			
			Monodoc.Lucene.Net.Search.Searchable local = new IndexSearcher(args[0]);
			RemoteSearchable impl = new RemoteSearchable(local);
			
			// bind the implementation to "Searchable"
			System.Runtime.Remoting.RemotingServices.Marshal(impl, "tcp://localhost:1099/Searchable");
			System.Console.ReadLine();
		}
	}
}