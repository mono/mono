// CS0218: The type `UserOperatorClass' must have operator `true' and operator `false' defined when `UserOperatorClass.operator &(UserOperatorClass, UserOperatorClass)' is used as a short circuit operator
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


