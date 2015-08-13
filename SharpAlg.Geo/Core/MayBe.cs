
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace SharpAlg.Geo.Core {
    [DebuggerNonUserCode]
    public static class MayBe {
        public static TR With<TI, TR>(this TI input, Func<TI, TR> evaluator)
            where TI : class {
            if(input == null)
                return default(TR);
            return evaluator(input);
        }
        public static TR With<TI, TR>(this TI? input, Func<TI, TR> evaluator)
            where TI : struct {
            if(input == null)
                return default(TR);
            return evaluator(input.Value);
        }
        public static TR Return<TI, TR>(this TI? input, Func<TI?, TR> evaluator, Func<TR> fallback) where TI : struct {
            if(!input.HasValue)
                return fallback != null ? fallback() : default(TR);
            return evaluator(input.Value);
        }
        public static TR Return<TI, TR>(this TI input, Func<TI, TR> evaluator, Func<TR> fallback) where TI : class {
            if(input == null)
                return fallback != null ? fallback() : default(TR);
            return evaluator(input);
        }
        public static bool ReturnSuccess<TI>(this TI input) where TI : class {
            return input != null;
        }
        public static bool ReturnSuccess<TI>(this TI? input) where TI : struct {
            return input != null;
        }
        public static TI If<TI>(this TI input, Func<TI, bool> evaluator) where TI : class {
            if(input == null)
                return null;
            return evaluator(input) ? input : null;
        }
        public static TI? If<TI>(this TI? input, Func<TI, bool> evaluator) where TI : struct {
            if(input == null)
                return null;
            return evaluator(input.Value) ? input : null;
        }
        public static TI Do<TI>(this TI input, Action<TI> action) where TI : class {
            if(input == null)
                return null;
            action(input);
            return input;
        }
    }
    public static class LinqExtensions {
        public static IEnumerable<T> Yield<T>(this T item) {
            yield return item;
        }
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach(var item in source) {
                action(item);
            }
        }
        public static IEnumerable<T> Tail<T>(this IEnumerable<T> source) {
            return source.Skip(1);
        }
        public static TValue GetOrAdd<TValue, TKey>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> getValue) {
            TValue val;
            if(dict.TryGetValue(key, out val)) {
                return val;
            }
            return dict[key] = getValue(key);
        }
        public static TValue GetOrAdd<TValue, TKey>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) {
            TValue val;
            if(dict.TryGetValue(key, out val)) {
                return val;
            }
            return dict[key] = value;
        }
        public static Func<TI, TR> Memoize<TI, TR>(this Func<TI, TR> f, IEqualityComparer<TI> comparer = null) {
            var dict = new Dictionary<TI, TR>(comparer);
            return x => dict.GetOrAdd(x, f);
        }
        public static bool IsOrdered<T>(this IEnumerable<T> source, IComparer<T> comparer) {
            var current = source.First();
            foreach(var next in source.Tail()) {
                if(comparer.Compare(current, next) >= 0)
                    return false;
                current = next;
            }
            return true;
        }
        //public static IEnumerable<T> Flatten<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> getItems) {
        //    return source.SelectMany(item => item.Yield().Concat(getItems(item).Flatten(getItems)));
        //}
    }
    //public class NoMatchException : ApplicationException { }

    ////default can be only last and only call
    //public struct Matcher<T, TResult> {
    //    readonly T x;
    //    readonly TResult result;
    //    readonly bool hasResult;
    //    public Matcher(T x) {
    //        this.x = x;
    //        this.result = default(TResult);
    //        this.hasResult = false;
    //    }
    //    public TResult Result {
    //        get {
    //            if(hasResult)
    //                return result;
    //            throw new NoMatchException();
    //        }
    //    }
    //    public Matcher(TResult result) {
    //        this.x = default(T);
    //        this.result = result;
    //        this.hasResult = true;
    //    }
    //    public Matcher<T, TResult> Case(Func<T, bool> predicate, Func<T, TResult> result) {
    //        if(hasResult)
    //            return this;
    //        if(predicate(x))
    //            return new Matcher<T, TResult>(result(x));
    //        return this;
    //    }
    //    public Matcher<T, TResult> Case(Func<T, bool> predicate, TResult result) {
    //        return Case(predicate, x => result);
    //    }

    //    public Matcher<T, TResult> Default(TResult result) {
    //        return Default(x => result);
    //    }
    //    public Matcher<T, TResult> Default(Func<T, TResult> result) {
    //        return Case(x => true, result);
    //    }

    //    public Matcher<T, TResult> Case<TOther>(TResult result) where TOther : class, T {
    //        return Case<TOther>(x => result);
    //    }
    //    public Matcher<T, TResult> Case<TOther>(Func<T, TResult> result) where TOther : class, T {
    //        return Case<TOther>(x => true, result);
    //    }


    //    public Matcher<T, TResult> Case<TOther>(Func<TOther, bool> predicate, TResult value) where TOther : class, T {
    //        return Case<TOther>(predicate, x => value);
    //    }
    //    public Matcher<T, TResult> Case<TOther>(Func<TOther, bool> predicate, Func<T, TResult> result) where TOther : class, T {
    //        return Case(x => (x is TOther) && predicate((TOther)x), result);
    //    }
    //}
    //public static class Matcher {
    //    public static Matcher<T, TResult> Case<T, TResult>(this T value, Func<T, bool> predicate, Func<T, TResult> result) {
    //        return new Matcher<T, TResult>(value).Case(predicate, result);
    //    }
    //    public static Matcher<T, TResult> Case<T, TResult>(this T value, Func<T, bool> predicate, TResult result) {
    //        return new Matcher<T, TResult>(value).Case(predicate, result);
    //    }
    //}
    public static class Utility {
        public static Func<T1, TR> Func<T1, TR>(Func<T1, TR> f) {
            return f;
        }
        public static Func<T1, T2, TR> Func<T1, T2, TR>(Func<T1, T2, TR> f) {
            return f;
        }
        public static Action<T1> Action<T1>(Action<T1> f) {
            return f;
        }
        public static Action<T1, T2> Action<T1, T2>(Action<T1, T2> f) {
            return f;
        }
        public static Expression<Func<T1, TR>> Expr<T1, TR>(Expression<Func<T1, TR>> e) {
            return e;
        }
        //public static Func<T, T> Id<T>() {
        //    return x => x;
        //}
        //public static Action<T> Nop<T>() {
        //    return x => { };
        //}
    }
}
