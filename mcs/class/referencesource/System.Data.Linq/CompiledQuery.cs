using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Linq.Mapping;
using System.Linq.Expressions;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Transactions;
using System.Data.Linq.Provider;
using System.Diagnostics.CodeAnalysis;

namespace System.Data.Linq {

    public sealed class CompiledQuery {
        LambdaExpression query;
        ICompiledQuery compiled;
        MappingSource mappingSource;

        private CompiledQuery(LambdaExpression query) {
            this.query = query;
        }

        public LambdaExpression Expression {
            get { return this.query; }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TResult> Compile<TArg0, TResult>(Expression<Func<TArg0, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TResult> Compile<TArg0, TArg1, TResult>(Expression<Func<TArg0, TArg1, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TResult> Compile<TArg0, TArg1, TArg2, TResult>(Expression<Func<TArg0, TArg1, TArg2, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "[....]: Generic types are an important part of Linq APIs and they could not exist without nested generic support.")]
        public static Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> Compile<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(Expression<Func<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>> query) where TArg0 : DataContext {
            if (query == null) {
                Error.ArgumentNull("query");
            }
            if (UseExpressionCompile(query)) {
                return query.Compile();
            }
            else {
                return new CompiledQuery(query).Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>;
            }
        }

        private static bool UseExpressionCompile(LambdaExpression query) {
            return typeof(ITable).IsAssignableFrom(query.Body.Type);
        }

        private TResult Invoke<TArg0, TResult>(TArg0 arg0) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0});
        }

        private TResult Invoke<TArg0, TArg1, TResult>(TArg0 arg0, TArg1 arg1) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14});
        }

        private TResult Invoke<TArg0, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(TArg0 arg0, TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5, TArg6 arg6, TArg7 arg7, TArg8 arg8, TArg9 arg9, TArg10 arg10, TArg11 arg11, TArg12 arg12, TArg13 arg13, TArg14 arg14, TArg15 arg15) where TArg0 : DataContext {
            return (TResult) this.ExecuteQuery(arg0, new object[] {arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15});
        }

        private object ExecuteQuery(DataContext context, object[] args) {
            if (context == null) {
                throw Error.ArgumentNull("context");
            }
            if (this.compiled == null) {
                lock (this) {
                    if (this.compiled == null) {
                        this.compiled = context.Provider.Compile(this.query);
                        this.mappingSource = context.Mapping.MappingSource;
                    }
                }
            }
            else {
                if (context.Mapping.MappingSource != this.mappingSource)
                    throw Error.QueryWasCompiledForDifferentMappingSource();
            }
            return this.compiled.Execute(context.Provider, args).ReturnValue;
        }
    }
}
