namespace PersonalizedLearningPath.DataStructures.Sorting;

public static class MergeSort
{
    // Stable merge sort (keeps relative order for equal keys).
    public static void Sort<T>(T[] items, Comparison<T> compare)
    {
        if (items == null) throw new ArgumentNullException(nameof(items));
        if (compare == null) throw new ArgumentNullException(nameof(compare));
        if (items.Length <= 1) return;

        var buffer = new T[items.Length];
        SortRange(items, buffer, 0, items.Length, compare);
    }

    private static void SortRange<T>(T[] items, T[] buffer, int start, int end, Comparison<T> compare)
    {
        int length = end - start;
        if (length <= 1) return;

        int mid = start + (length / 2);
        SortRange(items, buffer, start, mid, compare);
        SortRange(items, buffer, mid, end, compare);
        Merge(items, buffer, start, mid, end, compare);
    }

    private static void Merge<T>(T[] items, T[] buffer, int start, int mid, int end, Comparison<T> compare)
    {
        int i = start;
        int j = mid;
        int k = start;

        while (i < mid && j < end)
        {
            // <= makes it stable: left item wins ties.
            if (compare(items[i], items[j]) <= 0)
            {
                buffer[k++] = items[i++];
            }
            else
            {
                buffer[k++] = items[j++];
            }
        }

        while (i < mid) buffer[k++] = items[i++];
        while (j < end) buffer[k++] = items[j++];

        for (int x = start; x < end; x++)
        {
            items[x] = buffer[x];
        }
    }
}
