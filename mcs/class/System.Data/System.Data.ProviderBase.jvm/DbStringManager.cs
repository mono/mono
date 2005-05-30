using System.Resources;
using System.Reflection;

namespace System.Data.Common
{
	public class DbStringManager
	{
		#region Fields

		private readonly string _resourceName;
        private readonly ResourceManager _resourceManager;
		private readonly char[] _delimiters;

		#endregion // Fields

		#region Constructors

		public DbStringManager(string resourceName)
		{
			_resourceName = resourceName;
			_resourceManager = new ResourceManager(resourceName,Assembly.GetExecutingAssembly());
			_delimiters = new char[] {','};
		}        

		#endregion // Constructors

		#region Methods

        public string GetString(string key)
        {
			return _resourceManager.GetString(key);
        }
	
        public string GetString(string key, string defaultValue)
        {
            try {
                return GetString(key);
            }
            catch (Exception) {
                return defaultValue;
            }
        }
	
        public string[] GetStringArray(String key)
        {
            string tmp  = _resourceManager.GetString(key);
            string[] values = tmp.Split(_delimiters);
			
			for(int i=0; i < values.Length; i++) {
				values[i] = values[i].Trim();
			}
            return values;			
        }

		#endregion // Methods
    }
}
