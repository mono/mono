// cs0217.cs: In order to be applicable as a short circuit operator a user-defined logical operator ('UserOperatorClass.operator &(UserOperatorClass, UserOperatorClass)') must have the same return type as the type of its 2 parameters
// Line: 22

public class UserOperatorClass
{
        public static bool operator & (UserOperatorClass u1, UserOperatorClass u2) {
                return true;
        }
    
        public static bool operator true (UserOperatorClass u) {
                return true;
        }

        public static bool operator false (UserOperatorClass u) {
                return false;
        }

        public static void Main () {
                
                UserOperatorClass x = new UserOperatorClass();
                UserOperatorClass y = new UserOperatorClass();
                UserOperatorClass z = x && y;
        }
}


