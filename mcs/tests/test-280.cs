using System;
namespace AppFramework.Util
{
   public class Logic
   {
       static public bool EnumInSet(Enum anEnum, Enum[] checkSet)
       {
           foreach(Enum aVal in checkSet)
           {
               if (aVal == anEnum)
               {
                   return true;
               }
           }
           return false;
       }
   }
}
 
