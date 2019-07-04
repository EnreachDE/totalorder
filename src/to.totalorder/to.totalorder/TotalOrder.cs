namespace to.totalorder
{
    using System;

    using contracts;
    using contracts.data.domain;

    internal struct IndexOrder
    {
        public int Order;
        public int Index;
    }

    public class TotalOrder : ITotalOrder
    {
        public int[] Order(Submission[] submissions)
        {
            return SortOrder(DetermineOrder(submissions));
        }

        private static IndexOrder[] DetermineOrder(Submission[] submissions)
        {
            var indexLen = 0;

            // determine the number of submission indices
            if (submissions.Length > 0)
            {
                indexLen = submissions[0].Indexes.Length;
            }

            // create array of order / index pairs
            var indexOrder = new IndexOrder[indexLen];
            for (var ii = 0; ii < indexLen; ii++) indexOrder[ii].Index = ii;

            // accumulate orders
            foreach (var submission in submissions)
                for (var ii = 0; ii < submission.Indexes.Length; ii++)
                    if (submission.Indexes[ii] < indexLen)
                    {
                        indexOrder[submission.Indexes[ii]].Order += ii;
                    }

            return indexOrder;
        }

        private static int[] SortOrder(IndexOrder[] indexOrder)
        {
            Array.Sort(indexOrder, (x, y) => x.Order.CompareTo(y.Order));

            var result = new int[indexOrder.Length];
            for (var ii = 0; ii < indexOrder.Length; ii++) result[ii] = indexOrder[ii].Index;

            return result;
        }
    }
}