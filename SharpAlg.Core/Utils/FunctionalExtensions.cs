
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SharpAlg.Native {
    //(JsMode.Clr, Filename = SR.JS_Core_Utils)]
    public static class FunctionalExtensions {
        const string STR_InputSequencesHaveDifferentLength = "Input sequences have different length.";
        //public static IEnumerable<TOut> Map<TIn, TOut>(this Func<TIn, TOut> function, IEnumerable<TIn> input) {
        //    return input.Select(x => function(x));
        //}
        //public static IEnumerable<TOut> Map<TIn, TOut>(this Func<TIn, TOut> function, params TIn[] input) {
        //    return function.Map((IEnumerable<TIn>)input);
        //}

        //public static IEnumerable<TOut> Map<TIn1, TIn2, TOut>(this Func<TIn1, TIn2, TOut> function, IEnumerable<TIn1> input1, IEnumerable<TIn2> input2) {
        //    var result = new List<TOut>();
        //    Map((x, y) => result.Add(function(x, y)), input1, input2);
        //    return result;
        //}
        public static void Map<TIn1, TIn2>(this Action<TIn1, TIn2> action, IEnumerable<TIn1> input1, IEnumerable<TIn2> input2) {
            var enumerator1 = input1.GetEnumerator();
            var enumerator2 = input2.GetEnumerator();
            while(enumerator1.MoveNext()) {
                if(!enumerator2.MoveNext())
                    throw new ArgumentException(SR.STR_InputSequencesHaveDifferentLength);
                action(enumerator1.Current, enumerator2.Current);
            }
            if(enumerator2.MoveNext())
                throw new ArgumentException(SR.STR_InputSequencesHaveDifferentLength);
        }

        public static bool EnumerableEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer) {
            var en1 = first.GetEnumerator();
            var en2 = second.GetEnumerator();
            while(en1.MoveNext()) {
                if(!en2.MoveNext())
                    return false;
                if(!comparer(en1.Current, en2.Current))
                    return false;
            }
            return !en2.MoveNext();
        }
        public static bool SetEqual<T>(this IEnumerable<T> first, IEnumerable<T> second, Func<T, T, bool> comparer) {
            var list = second.ToList();
            foreach(var item in first) {
                bool found = false;
                foreach(var item2 in list) {
                    if(comparer(item, item2)) {
                        list.Remove(item2);
                        found = true;
                        break;
                    }
                }
                if(found == false)
                    return false;
            }
            return list.Count == 0;
        }
        public static IEnumerable<T> RemoveAt<T>(this IEnumerable<T> source, int index) {
            var en = source.GetEnumerator();
            while(en.MoveNext()) {
                if(index != 0)
                    yield return en.Current;
                index--;
            }
            if(index > 0)
                throw new IndexOutOfRangeException("index");
        }
        public static void Accumulate<T>(this IEnumerable<T> source, Action<T> init, Action<T> next) {
            var enumerator = source.GetEnumerator();
            if(enumerator.MoveNext())
                init(enumerator.Current);
            else
                throw new InvalidOperationException();
            while(enumerator.MoveNext()) {
                next(enumerator.Current);
            }
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            var enumerator = source.GetEnumerator();
            while(enumerator.MoveNext()) {
                action(enumerator.Current);
            }
        }
        //public static bool Equal<T>(this IEnumerable<T> first, params T[] second) {
        //    return Equal(first, (IEnumerable<T>)second);
        //}
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> source) {
            return source.Skip(1);
        }
        public static TVal TryGetValue<TKey, TVal>(this IDictionary<TKey, TVal> source, TKey key) where TVal : class {
            TVal result;
            if(source.TryGetValue(key, out result))
                return result;
            return null;
        }
        public static TOut ConvertAs<TOut>(this object source) where TOut : class {
            return source as TOut;
        }
        public static TOut ConvertCast<TOut>(this object source) {
            return (TOut)source;
        }
        public static IEnumerable<T> AsEnumerable<T>(this T source) {
            yield return source;
        }
        public static IEnumerable<T> Combine<T>(this T first, T next) {
            yield return first;
            yield return next;
        }
    }
}