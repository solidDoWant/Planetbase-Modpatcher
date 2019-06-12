using System;
using System.Collections.Generic;
using System.Linq;

namespace Patcher
{
    public static class LINQExtensions
    {
        public static IEnumerable<TSource> SelectManyRecursive<TSource>(this IEnumerable<TSource> source,
            Func<TSource, IEnumerable<TSource>> selector) =>
            source.SelectMany(sourceChild => sourceChild.SelectManyRecursive(selector));

        public static IEnumerable<TSource> SelectManyRecursive<TSource>(this TSource source,
            Func<TSource, IEnumerable<TSource>> selector)
        {
            if (source == null)
                return null;

            var baseSelection = selector(source).ToList();
            var recursiveSelection = new List<TSource>();

            if (baseSelection.Any())
            {
                foreach (var item in baseSelection)
                {
                    var selectionResults = item.SelectManyRecursive(selector);
                    //if (selectionResults == null ||)
                    //    recursiveSelection.Add(item);
                    //else
                    recursiveSelection.AddRange(selectionResults);
                }
            }

            recursiveSelection.Add(source);

            return recursiveSelection;
        }
    }
}