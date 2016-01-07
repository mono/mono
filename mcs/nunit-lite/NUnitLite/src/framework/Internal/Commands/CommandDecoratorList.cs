using System;
using NUnit.Framework.Api;

namespace NUnit.Framework.Internal.Commands
{
    /// <summary>
    /// CommandDecoratorList maintains a list of ICommandDecorators
    /// and is able to sort them by level so that they are applied
    /// in the proper order.
    /// </summary>
#if CLR_2_0 || CLR_4_0
    public class CommandDecoratorList : System.Collections.Generic.List<ICommandDecorator>
#else
    public class CommandDecoratorList : System.Collections.ArrayList
#endif
    {
        /// <summary>
        /// Order command decorators by the stage at which they apply.
        /// </summary>
        public void OrderByStage()
        {
            Sort(CommandDecoratorComparison);
        }

#if CLR_2_0 || CLR_4_0
        private int CommandDecoratorComparison(ICommandDecorator x, ICommandDecorator y)
        {
            return x.Stage.CompareTo(y.Stage);
        }
#else
        private CommandDecoratorComparer CommandDecoratorComparison = new CommandDecoratorComparer();

        private class CommandDecoratorComparer : System.Collections.IComparer
        {
            public int Compare(object x, object y)
            {
                ICommandDecorator xDecorator = x as ICommandDecorator;
                ICommandDecorator yDecorator = y as ICommandDecorator;

                if (xDecorator == null || yDecorator == null)
                    return 0;

                return xDecorator.Stage.CompareTo(yDecorator.Stage);
            }
        }
#endif
    }
}
