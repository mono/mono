// cs0218.cs: The type ('UserOperatorClass') must contain declarations of operator true and operator false
// Line: 22

public class UserOperatorClass
{
        public static UserOperatorClass operator & (UserOperatorClass u1, UserOperatorClass u2) {
                return new UserOperatorClass();
        }
    
//        public static bool operator true (UserOperatorClass u) {
//                return true;
//        }

//        public static bool operator false (UserOperatorClass u) {
//                return false;
//        }

        public static void Main() {
                
                UserOperatorClass x = new UserOperatorClass();
                UserOperatorClass y = new UserOperatorClass();
                UserOperatorClass z = x && y;
        }
}


