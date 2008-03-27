using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace System.Linq.jvm
{

    public class Interpreter
    {
        private class InternalVoidSubstitute
        {
        }

        private static readonly Type VOID_SUBSTITUTE = 
            typeof(InternalVoidSubstitute);

        LambdaExpression _expression;

        static MethodInfo [] _delegateMap = null;

        private const int MapSize = 5;

        static Interpreter()
        {
            InitDelegateMap();
        }

        private static void InitDelegateMap()
        {
            MethodInfo[] mia = 
                typeof(Interpreter).GetMethods(
                BindingFlags.Instance | 
                BindingFlags.Public);
            _delegateMap = new MethodInfo[MapSize];
            foreach (MethodInfo m in mia)
            {
                if (m.Name == "GetDelegate")
                {
                    _delegateMap[m.GetGenericArguments().Length - 1] = m;
                }
            }
        }


        public LambdaExpression Expression
        {
            get { return _expression; }
        }

        public Interpreter(LambdaExpression expression)
        {
            _expression = expression;
        }

        public Delegate CreateDelegate()
        {
            Type[] arr = ExtractGenerecParameters();
            MethodInfo mi = _delegateMap[arr.Length - 1];
            MethodInfo mgi = mi.MakeGenericMethod(arr);
            return (Delegate)mgi.Invoke(this, new object[0]);    
        }

        public void Validate()
        {
            ExpressionValidator validator = new ExpressionValidator(this.Expression);
            validator.Validate();
        }

        private Type[] ExtractGenerecParameters()
        {
            Type[] arr = new Type[Expression.Parameters.Count + 1];
            Type rt = Expression.GetReturnType();
            if (rt == typeof(void))
            {
                rt = VOID_SUBSTITUTE;
            }
            arr[Expression.Parameters.Count] = rt;
            for (int i = 0; i < Expression.Parameters.Count; i++)
            {
                arr[i] = Expression.Parameters[i].Type;
            }
            return arr;
        }

        private object Run(object[] arg)
        {
            ExpressionInterpreter inter = new ExpressionInterpreter(arg);
            inter.Run((LambdaExpression)_expression);
            return inter.Value;
        }

        public Delegate GetDelegate<TResult>()
        {
            if (typeof(TResult) == VOID_SUBSTITUTE)
            {
                return new Action(this.ActionAccessor);
            }
            return new Func<TResult>(this.FuncAccessor<TResult>);
        }

        public TResult FuncAccessor<TResult>()
        {
            return (TResult) Run(new object[0]);
        }

        public void ActionAccessor()
        {
            Run(new object[0]);
        }
        
        public Delegate GetDelegate<T, TResult>()
        {
            if (typeof(TResult) == VOID_SUBSTITUTE)
            {
                return new Action<T>(this.ActionAccessor<T>);
            }
            return new Func<T, TResult>(this.FuncAccessor<T, TResult>);           
        }

        public TResult FuncAccessor<T, TResult>(T arg)
        {
            return (TResult)Run(new object[] { arg });
        }

        public void ActionAccessor<T>(T arg)
        {
            Run(new object[] { arg });
        }

        public Delegate GetDelegate<T1, T2, TResult>()
        {
            if (typeof(TResult) == VOID_SUBSTITUTE)
            {
                return new Action<T1, T2>(this.ActionAccessor<T1, T2>);
            }
            return new Func<T1, T2, TResult>(this.FuncAccessor<T1, T2, TResult>);
        }

        public TResult FuncAccessor<T1, T2, TResult>(T1 arg1, T2 arg2)
        {
            return (TResult)Run(new object[] { arg1, arg2 });
        }

        public void ActionAccessor<T1, T2>(T1 arg1, T2 arg2)
        {
            Run(new object[] { arg1, arg2 });
        }

        public Delegate GetDelegate<T1, T2, T3, TResult>()
        {
            if (typeof(TResult) == VOID_SUBSTITUTE)
            {
                return new Action<T1, T2, T3>(this.ActionAccessor<T1, T2, T3>);
            }
            return new Func<T1, T2, T3, TResult>(this.FuncAccessor<T1, T2, T3, TResult>);
        }

        public TResult FuncAccessor<T1, T2, T3, TResult>(T1 arg1, T2 arg2, T3 arg3)
        {
            return (TResult)Run(new object[] { arg1, arg2, arg3 });
        }

        public void ActionAccessor<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
        {
            Run(new object[] { arg1, arg2, arg3 });
        }

        public Delegate GetDelegate<T1, T2, T3, T4, TResult>()
        {
            if (typeof(TResult) == VOID_SUBSTITUTE)
            {
                return new Action<T1, T2, T3, T4>(this.ActionAccessor<T1, T2, T3, T4>);
            }
            return new Func<T1, T2, T3, T4, TResult>(this.FuncAccessor<T1, T2, T3, T4, TResult>);
        }

        public TResult FuncAccessor<T1, T2, T3, T4, TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            return (TResult)Run(new object[] { arg1, arg2, arg3, arg4});
        }

        public void ActionAccessor<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            Run(new object[] { arg1, arg2, arg3, arg4 });
        }

        
    }    
   

    
}
