//
// System.Data.Odbc.OdbcCommandBuilder
//
// Author:
//   Umadevi S (sumadevi@novell.com)
//
// Copyright (C) Novell Inc, 2004
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.Odbc
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>

#if ONLY_1_1
	public sealed class OdbcCommandBuilder : Component
#else // NET_2_0 and higher
        public sealed class OdbcCommandBuilder : DbCommandBuilder
#endif // ONLY_1_1
	{
		#region Fields

		OdbcDataAdapter adapter;
		string quotePrefix;
		string quoteSuffix;

		#endregion // Fields

		#region Constructors
		
		public OdbcCommandBuilder ()
		{
			adapter = null;
			quotePrefix = String.Empty;
			quoteSuffix = String.Empty;
		}

		public OdbcCommandBuilder (OdbcDataAdapter adapter) 
			: this ()
		{
			this.adapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		[OdbcDescriptionAttribute ("The DataAdapter for which to automatically generate OdbcCommands")]
		[DefaultValue (null)]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcDataAdapter DataAdapter {
			get {
				return adapter;
			}
			set {
				adapter = value;
			}
		}

		[BrowsableAttribute (false)]
		[OdbcDescriptionAttribute ("The prefix string wrapped around sql objects")]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
                override
#endif // NET_2_0
                string QuotePrefix {
			get {
				return quotePrefix;
			}
			set {
				quotePrefix = value;
			}
		}

		[BrowsableAttribute (false)]
                [OdbcDescriptionAttribute ("The suffix string wrapped around sql objects")]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
                override
#endif // NET_2_0
                string QuoteSuffix {
			get {
				return quoteSuffix;
			}
			set {
				quoteSuffix = value;
			}
		}

		#endregion // Properties

		#region Methods

		public static void DeriveParameters (OdbcCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing) 
		{
			throw new NotImplementedException ();		
		}

		[MonoTODO]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand GetDeleteCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand GetInsertCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcCommand GetUpdateCommand ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public
#if NET_2_0
                override
#endif // NET_2_0
                void RefreshSchema ()
		{
			throw new NotImplementedException ();
		}
                
#if NET_2_0
                [MonoTODO]
                protected override void ApplyParameterInfo (IDbDataParameter dbParameter, DataRow row)
                {
                        throw new NotImplementedException ();
                }

                [MonoTODO]
                protected override string GetParameterName (int position)
                {
                        throw new NotImplementedException ();                        
                }
                

                [MonoTODO]
                protected override string GetParameterPlaceholder (int position)
                {
                        throw new NotImplementedException ();                        
                }
                
                [MonoTODO]
                protected override DbProviderFactory ProviderFactory
                {
                        get {throw new NotImplementedException ();}
                }

                [MonoTODO]
                protected override void SetRowUpdatingHandler (DbDataAdapter adapter)
                {
                        throw new NotImplementedException ();
                }

#endif // NET_2_0


		#endregion // Methods
	}
}
