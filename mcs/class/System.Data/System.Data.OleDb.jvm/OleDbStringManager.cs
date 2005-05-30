//namespace System.Data.OleDb
//{
//
//    using java.util;
//
//    public class OleDbStringManager : IDbStringManager
//    {
//
//        private static readonly String BUNDLE_NAME = "OleDBStrings"; //$NON-NLS-1$
//
//        private static readonly ResourceBundle RESOURCE_BUNDLE = ResourceBundle.getBundle(BUNDLE_NAME);
//
//        private OleDbStringManager()
//        {
//        }
//
//        public String getString(String key)
//        {
//            try
//            {
//                return RESOURCE_BUNDLE.getString(key);
//            }
//            catch (MissingResourceException)
//            {
//                return null;
//            }
//        }
//	
//        public String getString(String key, String defaultValue)
//        {
//            try
//            {
//                return RESOURCE_BUNDLE.getString(key);
//            }
//            catch (MissingResourceException)
//            {
//                return defaultValue;
//            }
//        }
//	
//	
//        public String[] getStringArray(String key)
//        {
//            try
//            {
//                String tmp = RESOURCE_BUNDLE.getString(key);
//                java.util.StringTokenizer st = new java.util.StringTokenizer(tmp, ",");
//			
//                String[] strArr = new String[st.countTokens()];
//			
//                for (int i = 0; i < strArr.Length; i++)
//                {
//                    strArr[i] = st.nextToken();
//				
//                }
//				
//                return strArr;
//			
//            }
//            catch (MissingResourceException)
//            {
//                return null;
//            }
//        }
//    }
//}