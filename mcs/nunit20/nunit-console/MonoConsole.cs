//
// NUnit.Console.MonoConsole.cs
//
// Author:
//   Jackson Harper (Jackson@LatitudeGeo.com)
//

using System;
using System.Reflection;
using NUnit.Framework;


namespace NUnit.Console {

  /// <summary>
  ///    This is a lightweight NUnit 2.0 console runner designed to run
  ///    in mono's current state. As soon as AppDomains are fully implemented
  ///    in mono the nunit-console class can be used
  /// </summary>
  public class MonoConsole {

    private Assembly    mAssembly;

    //
    // Public Constructors
    //

    public MonoConsole( string assemblypath ) {
      mAssembly  =  Assembly.LoadFrom( assemblypath );
      Run();
    }
    
    //
    // Private Methods
    //
    
    private void Run() {

    }

    private void TestType( Type t ) {

    }

    private void TestMethod( MemberInfo method ) {

    }

  }

}

