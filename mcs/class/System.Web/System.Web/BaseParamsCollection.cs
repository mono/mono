using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace System.Web
{
	/// <summary>
	/// Summary description for BaseParamsCollection.
	/// </summary>
	abstract class BaseParamsCollection : WebROCollection
	{
		protected HttpRequest _request;
		protected bool _loaded = false;

		public BaseParamsCollection(HttpRequest request)
		{		
			_request = request;
			IsReadOnly = true;
		}

		private void LoadInfo()
		{
			if (_loaded)
				return;
			IsReadOnly = false;

            InsertInfo();
	
			IsReadOnly = true;
			_loaded = true;

		}

		protected abstract void InsertInfo();

		public override string Get(int index)
		{
			LoadInfo();
			return base.Get(index); 
		}

		protected abstract string InternalGet(string name);

		public override string Get(string name)
		{			
			if (!_loaded)			
				return InternalGet(name);						
				
			return base.Get(name);		
		}

		public override string GetKey(int index)
		{
			LoadInfo();
			return base.GetKey(index); 
		}
 
		public override string[] GetValues(int index)
		{
			string text1;
			string[] array1;
			text1 = Get(index);
			if (text1 == null)
			{
				return null; 
			}
			array1 = new string[1];
			array1[0] = text1;
			return array1; 
		}
 
		public override string[] GetValues(string name)
		{
			string text1;
			string[] array1;
			text1 = Get(name);
			if (text1 == null)
			{
				return null; 
			}
			array1 = new string[1];
			array1[0] = text1;
			return array1; 
		}
 
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			throw new SerializationException(); 
		}

		public override string[] AllKeys 
		{
			get 
			{
				LoadInfo();
				return base.AllKeys;
			}
		}

		public override int Count 
		{
			get 
			{
				LoadInfo();
				return base.Count;
			}
		}
	}
}
