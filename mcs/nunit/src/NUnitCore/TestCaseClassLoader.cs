namespace NUnit.Runner 
{
	using System;

	/// <summary>
	/// 
	/// </summary>
	[Obsolete("Use StandardLoader or UnloadingLoader")]
	public class TestCaseClassLoader : StandardTestSuiteLoader 
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="resolve"></param>
		/// <returns></returns>
		public Type LoadClass(string name, bool resolve) 
		{
			return Load(name);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public bool IsExcluded(string name) 
		{
			return false;
		}
	}
}


#if false
// commented out till figure out .net class reloading

namespace NUnit.Runner {

 /**
 * A custom class loader which enables the reloading
 * of classes for each test run. The class loader
 * can be configured with a list of package paths that
 * should be excluded from loading. The loading
 * of these packages is delegated to the system class
 * loader. They will be shared across test runs.
 * <p>
 * The list of excluded package paths is specified in
 * a properties file "excluded.properties" that is located in 
 * the same place as the TestCaseClassLoader class.
 * <p>
 * <b>Known limitation:</b> the TestCaseClassLoader cannot load classes
 * from jar files.
 */


  public class TestCaseClassLoader: ClassLoader {
    /** scanned class path */
    private string[] fPathItems;
    /** excluded paths */
    private string[] fExcluded= { "com.sun.", "sun."};
    /** name of excluded properties file */
    static final string EXCLUDED_FILE= "excluded.properties";
    /**
     * Constructs a TestCaseLoader. It scans the class path
     * and the excluded package paths
     */
    public TestCaseClassLoader() {
      super();
      string classPath= System.getProperty("java.class.path");
      string separator= System.getProperty("path.separator");
        
      // first pass: count elements
      StringTokenizer st= new StringTokenizer(classPath, separator);
      int i= 0;
      while (st.hasMoreTokens()) {
        st.nextToken();
        i++;
      }
      // second pass: split
      fPathItems= new string[i];
      st= new StringTokenizer(classPath, separator);
      i= 0;
      while (st.hasMoreTokens()) {
        fPathItems[i++]= st.nextToken();
      }

      string[] excluded= ReadExcludedPackages();
      if (excluded != null)
        fExcluded= excluded;    
    }
    public java.net.URL GetResource(string name) {
      return ClassLoader.getSystemResource(name);
    }
    public InputStream GetResourceAsStream(string name) {
      return ClassLoader.getSystemResourceAsStream(name);
    }
    protected boolean IsExcluded(string name) {
      // exclude the "java" and "junit" packages.
      // They always need to be excluded so that they are loaded by the system class loader
      if (name.startsWith("java.") || 
          name.startsWith("junit.framework") ||
          name.startsWith("junit.extensions") ||
          name.startsWith("junit.util") ||
          name.startsWith("junit.ui"))
        return true;
            
      // exclude the user defined package paths
      for (int i= 0; i < fExcluded.length; i++) {
        if (name.startsWith(fExcluded[i])) {
          return true;
        }
      }
      return false;    
    }
    public synchronized Class LoadClass(string name, boolean resolve)
      throws ClassNotFoundException {
            
      Class c= FindLoadedClass(name);
      if (c != null)
        return c;
      //
      // Delegate the loading of excluded classes to the
      // standard class loader.
      //
      if (IsExcluded(name)) {
        try {
          c= findSystemClass(name);
          return c;
        } catch (ClassNotFoundException e) {
          // keep searching
        }
      }
      if (c == null) {
        File file= Locate(name);
        if (file == null)
          throw new ClassNotFoundException();
        byte data[]= LoadClassData(file);
        c= defineClass(name, data, 0, data.length);
      }
      if (resolve) 
        resolveClass(c);
      return c;
    }
    private byte[] LoadClassData(File f) throws ClassNotFoundException {
      try {
        //System.out.println("loading: "+f.getPath());
        FileInputStream stream= new FileInputStream(f);
            
        try {
          byte[] b= new byte[stream.available()];
          stream.read(b);
          stream.close();
          return b;
        }
        catch (IOException e) {
          throw new ClassNotFoundException();
        }
      }
      catch (FileNotFoundException e) {
        throw new ClassNotFoundException();
      }
    }
    /**
     * Locate the given file.
     * @return Returns null if file couldn't be found.
     */
    private File Locate(string fileName) { 
      fileName= fileName.replace('.', '/')+".class";
      File path= null;
        
      if (fileName != null) {
        for (int i= 0; i < fPathItems.length; i++) {
          path= new File(fPathItems[i], fileName);
          if (path.exists())
            return path;
        }
      }
      return null;
    }
    private string[] ReadExcludedPackages() {        
      InputStream is= getClass().GetResourceAsStream(EXCLUDED_FILE);
      if (is == null) 
        return null;
      Properties p= new Properties();
      try {
        p.Load(is);
      }
      catch (IOException e) {
        return null;
      }
      Vector v= new Vector(10);
      for (Enumeration e= p.propertyNames(); e.hasMoreElements(); ) {
        string key= (string)e.nextElement();
        if (key.startsWith("excluded.")) {
          string path= p.getProperty(key);
          if (path.endsWith("*"))
            path= path.substring(0, path.length()-1);
          if (path.length() > 0) 
            v.addElement(path);                
        }
      }
      string[] excluded= new string[v.size()];
      for (int i= 0; i < v.size(); i++)
        excluded[i]= (string)v.elementAt(i);
      return excluded;
    }
  }
#endif
