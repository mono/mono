/*
 *  Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *     The contents of this file are subject to the Initial 
 *     Developer's Public License Version 1.0 (the "License"); 
 *     you may not use this file except in compliance with the 
 *     License. You may obtain a copy of the License at 
 *     http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *     Software distributed under the License is distributed on 
 *     an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *     express or implied.  See the License for the specific 
 *     language governing rights and limitations under the License.
 * 
 *  Copyright (c) 2002, 2004 Carlos Guzman Alvarez
 *  All Rights Reserved.
 */

using System;
using System.Data;
using System.Data.Common;

using NUnit.Framework;
using FirebirdSql.Data.Firebird;

namespace FirebirdSql.Data.Firebird.Tests
{
	[TestFixture]
	public class FbParameterTest : BaseTest 
	{
		public FbParameterTest() : base()
		{		
		}
		
		[Test]
		public void ConstructorsTest()
		{
			FbParameter ctor01 = new FbParameter();
			FbParameter ctor02 = new FbParameter("ctor2", 10);
			FbParameter ctor03 = new FbParameter("ctor3", FbDbType.Char);
			FbParameter ctor04 = new FbParameter("ctor4", FbDbType.Integer, 4);
			FbParameter ctor05 = new FbParameter("ctor5", FbDbType.Integer, 4, "int_field");
			FbParameter ctor06 = new FbParameter(
				"ctor6", 
				FbDbType.Integer, 
				4, 
				ParameterDirection.Input, 
				false, 
				0, 
				0, 
				"int_field", 
				DataRowVersion.Original, 
				100);

            ctor01 = null;
            ctor02 = null;
            ctor03 = null;
            ctor04 = null;
            ctor05 = null;
            ctor06 = null;
		}

        [Test]
        public void CloneTest()
        {
            FbParameter p = new FbParameter("@p1", FbDbType.Integer);
            p.Value = 1;
            p.Charset = FbCharset.Dos850;

            FbParameter p1 = ((ICloneable)p).Clone() as FbParameter;

            Assert.AreEqual(p1.ParameterName, p.ParameterName);
            Assert.AreEqual(p1.FbDbType     , p.FbDbType);
            Assert.AreEqual(p1.DbType       , p.DbType);
            Assert.AreEqual(p1.Direction    , p.Direction);
            Assert.AreEqual(p1.SourceColumn , p.SourceColumn);
            Assert.AreEqual(p1.SourceVersion, p.SourceVersion);
            Assert.AreEqual(p1.Charset      , p.Charset);
            Assert.AreEqual(p1.IsNullable   , p.IsNullable);
            Assert.AreEqual(p1.Size         , p.Size);
            Assert.AreEqual(p1.Scale        , p.Scale);
            Assert.AreEqual(p1.Precision    , p.Precision);
            Assert.AreEqual(p1.Value        , p.Value);
        }
    }
}
