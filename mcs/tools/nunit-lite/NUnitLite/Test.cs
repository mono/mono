
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Filters;

namespace Xamarin
{
	[Serializable]
	class Test : ITest
	{
		public Test (ITest test)
		{
			Name = test.Name;
			FullName = test.FullName;
			IsSuite = test.IsSuite;
		}

		public int Id
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public string Name
		{
			get;
		}

		public string FullName
		{
			get;
		}

		public Type FixtureType
		{
			get { throw new NotImplementedException (); }
		}

		public RunState RunState
		{
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		public int TestCaseCount
		{
			get { throw new NotImplementedException (); }
		}

		public IPropertyBag Properties
		{
			get { throw new NotImplementedException (); }
		}

		public ITest Parent
		{
			get { throw new NotImplementedException (); }
		}

		public bool IsSuite
		{
			get;
		}

		public bool HasChildren
		{
			get { throw new NotImplementedException (); }
		}

		public int Seed
		{
			get { throw new NotImplementedException (); }
		}

		public IList<ITest> Tests
		{
			get { throw new NotImplementedException (); }
		}

		public XmlNode ToXml(bool recursive)
		{
			throw new NotImplementedException ();
		}

		public XmlNode AddToXml(XmlNode parentNode, bool recursive)
		{
			throw new NotImplementedException ();
		}
	}
}
