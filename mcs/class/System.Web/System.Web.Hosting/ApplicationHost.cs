//
// System.Web.Hosting.ApplicationHost
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   (class signature from Bob Smith <bob@thestuff.net> (C) )
//

using System;
using System.Runtime.Remoting;

namespace System.Web.Hosting {
   public sealed class ApplicationHost {
      [MonoTODO("object CreateApplicationHost() Implement (dummy implementation right now)")]
      public static object CreateApplicationHost(Type HostType, string VirtualPath, string PhysicalPath) {
         // Construct and own AppDomain via DomainFactory? Can be good to have control over the web appdomain
         // Dummy impl: just return a init object..

         // TODO: Save in the created app domain....
         System.Threading.Thread.GetDomain().SetData(".ASP.Net.App.VirtualPath", VirtualPath);
         System.Threading.Thread.GetDomain().SetData(".ASP.Net.App.Path", PhysicalPath);
         
         // TODO: Set to the install path of the runtime engine....
         System.Threading.Thread.GetDomain().SetData(".ASP.Net.App.InstallPath", "");

         // TODO: Create a name and id for the application...
         // TODO: Copy all of the domain info to our new domain

         ObjectHandle obj = System.Threading.Thread.GetDomain().CreateInstance(HostType.Module.Assembly.FullName, HostType.FullName);
         return obj.Unwrap();
      }
   }
}
