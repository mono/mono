// cs1722.cs: In Class `My.Namespace.MyBaseClass', `My.Namespace.MyInterfaceBase' is not an interface, a base class must be listed first
// Line: 21

using System;

namespace My.Namespace {


    public interface IMyInterface {

        String InterfaceProperty { get; }
    }

    public abstract class MyInterfaceBase : IMyInterface {

        protected abstract String SubclassProperty { get; }

        public String InterfaceProperty { get { return this.SubclassProperty; } }
    }

    public class MyBaseClass : IMyInterface, MyInterfaceBase {
      //  protected override String SubclassProperty { get { return "foo"; } }
    }
}

