using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;


[AttributeUsage (AttributeTargets.All)]
public class MyAttribute : Attribute {
	public MyAttribute (object o) {
            this.o = o;
	}

        public object my
        {
            get {
                return o;
            }
        }

        object o;
}

public class MyConstructorBuilder
{
   public static int Main()
   {
      Type myHelloworld = MyCreateCallee(Thread.GetDomain());
      ConstructorInfo myConstructor = myHelloworld.GetConstructor(new Type[]{typeof(String)});
      object[] myAttributes1 = myConstructor.GetCustomAttributes(true);
      if (myAttributes1 == null)
	      return 1;
      if (myAttributes1.Length != 1)
	      return 2;
      MyAttribute myAttribute = myAttributes1[0] as MyAttribute;
      if (myAttribute == null)
	      return 3;
      if (myAttribute.my.GetType() != typeof(TypeCode))
	      return 4;
      return 0;
   }

   private static Type MyCreateCallee(AppDomain domain)
   {
      AssemblyName myAssemblyName = new AssemblyName();
      myAssemblyName.Name = "EmittedAssembly";
      // Define a dynamic assembly in the current application domain.
      AssemblyBuilder myAssembly =
                  domain.DefineDynamicAssembly(myAssemblyName,AssemblyBuilderAccess.Run);
      // Define a dynamic module in this assembly.
      ModuleBuilder myModuleBuilder = myAssembly.DefineDynamicModule("EmittedModule");
      // Construct a 'TypeBuilder' given the name and attributes.
      TypeBuilder myTypeBuilder = myModuleBuilder.DefineType("HelloWorld",
         TypeAttributes.Public);
      // Define a constructor of the dynamic class.
      ConstructorBuilder myConstructor = myTypeBuilder.DefineConstructor(
               MethodAttributes.Public, CallingConventions.Standard, new Type[]{typeof(String)});
      ILGenerator myILGenerator = myConstructor.GetILGenerator();
      myILGenerator.Emit(OpCodes.Ldstr, "Constructor is invoked");
      myILGenerator.Emit(OpCodes.Ldarg_1);
      MethodInfo myMethodInfo =
                     typeof(Console).GetMethod("WriteLine",new Type[]{typeof(string)});
      myILGenerator.Emit(OpCodes.Call, myMethodInfo);
      myILGenerator.Emit(OpCodes.Ret);
      Type myType = typeof(MyAttribute);
      ConstructorInfo myConstructorInfo = myType.GetConstructor(new Type[]{typeof(object)});
      try
      {
        CustomAttributeBuilder methodCABuilder = new CustomAttributeBuilder (myConstructorInfo, new object [] { TypeCode.Double } );        

         myConstructor.SetCustomAttribute(methodCABuilder);
      }
      catch(ArgumentNullException ex)
      {
         Console.WriteLine("The following exception has occured : "+ex.Message);
      }
      catch(Exception ex)
      {
         Console.WriteLine("The following exception has occured : "+ex.Message);
      }
      return myTypeBuilder.CreateType();
   }
}

