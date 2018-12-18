

namespace to.totalorder
{
    using System;
    using contracts;
    using contracts.data.domain;

    struct IndexOrder
    {
        public int order;
        public int index;
    }

    public class TotalOrder : ITotalOrder
    {
        public int[] Order(Submission[] submissions)
        {
            return SortOrder(DetermineOrder(submissions));
        }

        private static IndexOrder[] DetermineOrder(Submission[] submissions)
        {
            int indexLen = 0;

            // determine the number of submission indices
            if (submissions.Length > 0)
            {
                indexLen = submissions[0].Indexes.Length;
            }

            // create array of order / index pairs
            IndexOrder[] indexOrder = new IndexOrder[indexLen];
            for (int ii = 0; ii < indexLen; ii++)
            {
                indexOrder[ii].index = ii;
            }

            // accumulate orders
            foreach (var submission in submissions)
            {
                for (int ii = 0; ii < submission.Indexes.Length; ii++)
                {
                    if (submission.Indexes[ii] < indexLen)
                    {
                        indexOrder[submission.Indexes[ii]].order += ii;
                    }
                }
            }

            return indexOrder;
        }

        private static int[] SortOrder(IndexOrder[] indexOrder)
        {
            Array.Sort<IndexOrder>(indexOrder, (x, y) => x.order.CompareTo(y.order));

            int[] result = new int[indexOrder.Length];
            for (int ii = 0; ii < indexOrder.Length; ii++)
            {
                result[ii] = indexOrder[ii].index;
            }

            return result;
        }
    }
}
