using System;
using System.Security;
using System.Security.Permissions;

public class Program {

        static public void Main (string [] args)
        {
                SecurityAction a = SecurityAction.Demand;
                switch (a) {
                        case (SecurityAction)13:
                        case SecurityAction.Demand:
                                Console.WriteLine ("ok");
                        break;
                }
        }
}
 
