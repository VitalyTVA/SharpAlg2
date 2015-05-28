using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SharpAlg.Native;
//

namespace SharpAlg.Tests {
    [System.Diagnostics.DebuggerNonUserCode]
    //(JsMode.Clr, Filename = SR.JSTestsName)]
    public static class FluentAssert {
        public static TInput IsNull<TInput>(this TInput obj, Func<TInput, object> valueEvaluator = null) where TInput : class {
            Assert.IsNull(GetActualValue(obj, valueEvaluator));
            return obj;
        }
        public static TInput IsNotNull<TInput>(this TInput obj, Func<TInput, object> valueEvaluator = null) where TInput : class {
            Assert.IsNotNull(GetActualValue(obj, valueEvaluator));
            return obj;
        }
        public static TInput IsEqual<TInput>(this TInput obj, object expectedValue) {
            AreEqual(expectedValue, obj);
            return obj;
        }
        public static TInput IsEqual<TInput>(this TInput obj, Func<TInput, object> valueEvaluator, object expectedValue) {
            AreEqual(expectedValue, valueEvaluator(obj));
            return obj;
        }
        public static TInput IsNotEqual<TInput>(this TInput obj, object expectedValue) {
            Assert.AreNotEqual(expectedValue, obj);
            return obj;
        }
        public static TInput IsNotEqual<TInput>(this TInput obj, Func<TInput, object> valueEvaluator, object expectedValue) {
            Assert.AreNotEqual(expectedValue, valueEvaluator(obj));
            return obj;
        }
        public static TInput IsTrue<TInput>(this TInput obj, Func<TInput, bool> valueEvaluator) {
            AreEqual(true, valueEvaluator(obj));
            return obj;
        }
        public static TInput IsFalse<TInput>(this TInput obj, Func<TInput, bool> valueEvaluator) {
            AreEqual(false, valueEvaluator(obj));
            return obj;
        }
        public static bool IsTrue(this bool val) {
            AreEqual(true, val);
            return val;
        }
        public static bool IsFalse(this bool val) {
            AreEqual(false, val);
            return val;
        }
        //public static TInput IsInstanceOfType<TInput>(this TInput obj, Type expectedType) where TInput : class {
        //    Assert.IsInstanceOfType(expectedType, obj);
        //    return obj;
        //}
        //public static TInput IsInstanceOfType<TInput>(this TInput obj, Func<TInput, object> valueEvaluator, Type expectedType) where TInput : class {
        //    Assert.IsInstanceOfType(expectedType, valueEvaluator(obj));
        //    return obj;
        //}
        static object GetActualValue<TInput>(TInput obj, Func<TInput, object> valueEvaluator) {
            return valueEvaluator == null ? obj : valueEvaluator(obj);
        }

        public static IEnumerable<T> IsSequenceEqual<T>(this IEnumerable<T> first, IEnumerable<T> second) {
            Action<T, T> assert = (x, y) => AreEqual(x, y);
            assert.Map(first, second);
            return first;
        }
        //public static IEnumerable<T> IsSequenceEqual<T>(this IEnumerable<T> first, params T[] second) {
        //    return IsSequenceEqual(first, (IEnumerable<T>)second);
        //}

        public static TInput Fails<TInput>(this TInput obj, Action<TInput> action, Type exceptionType = null, Action<Exception> exceptionCheck = null) {
            try {
                action(obj);
            } catch(Exception e) {
                CheckExceptionType(exceptionType, e);
                if(exceptionCheck != null)
                    exceptionCheck(e);
                return obj;
            }
            throw new AssertionException("Exception expected");
        }
        #region JS compatibility
        static void CheckExceptionType(Type exceptionType, Exception e) {
            e.GetType().IsEqual(exceptionType);
        }
        //(JsMode.Clr, Filename = SR.JSTestsName)]
        public class JsAssertionException : Exception {
            public JsAssertionException(string message)
                : base(message) {
            }
        }
        static void AreEqual(object expected, object actual) {
            Assert.AreEqual(expected, actual);
        }

        static void JsAreEqual(object expected, object actual) {
            if(!object.Equals(expected, actual))
                throw new JsAssertionException("Expected: " + expected + " but was: " + actual);
        }
        #endregion
    }
}
