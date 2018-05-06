using System.Collections.Generic;

namespace CachePopulator.Extensions
{
    public static class EnumerableExtensions
    {
		public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> collection, int size)
        {
            var nextbatch = new List<T>(size);
            foreach (T item in collection)
            {
                nextbatch.Add(item);
				if (nextbatch.Count == size)
                {
                    yield return nextbatch;
					nextbatch = new List<T>(size);
                }
            }

            if (nextbatch.Count > 0)
            {
                yield return nextbatch;
            }
        }
    }
}
