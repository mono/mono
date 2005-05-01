using java.util;

namespace System.Data.Common
{
	public class DbStringManager
	{
		public DbStringManager(string bundleName)
		{
			_bundleName = bundleName;
			_resourceBundle = ResourceBundle.getBundle(_bundleName);
		}

        private readonly string _bundleName;

        private readonly ResourceBundle _resourceBundle;

        public string GetString(string key)
        {
            try {
                return _resourceBundle.getString(key);
            }
            catch (MissingResourceException) {
                return null;
            }
        }
	
        public string GetString(string key, string defaultValue)
        {
            try {
                return _resourceBundle.getString(key);
            }
            catch (MissingResourceException) {
                return defaultValue;
            }
        }
	
	
        public string[] GetStringArray(String key)
        {
            try {
                string tmp = _resourceBundle.getString(key);
                java.util.StringTokenizer st = new java.util.StringTokenizer(tmp, ",");
			
                String[] strArr = new String[st.countTokens()];
			
                for (int i = 0; i < strArr.Length; i++) {
                    strArr[i] = st.nextToken();
                }				
                return strArr;			
            }
            catch (MissingResourceException) {
                return null;
            }
        }
    }
}
