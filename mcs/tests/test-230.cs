using System;
using System.Reflection;
using System.Diagnostics;

[module: DebuggableAttribute (false, false)] 

class TestClass {
        public static int Main()
        {
            Module[] moduleArray;
            moduleArray = Assembly.GetExecutingAssembly().GetModules(false);

            Module myModule = moduleArray[0];
            object[] attributes;
            
            attributes = myModule.GetCustomAttributes(typeof (DebuggableAttribute), false);
            if (attributes[0] != null)
            {
                Console.WriteLine ("Succeeded");
                return 0;
            }
            return 1;
        }
}
