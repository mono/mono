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

