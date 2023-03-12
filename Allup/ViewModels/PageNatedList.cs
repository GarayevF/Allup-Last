namespace Allup.ViewModels
{
    public class PageNatedList<T> : List<T>
    {
        public PageNatedList(IQueryable<T> query, int pageIndex, int totalCount)
        {
            PageIndex = pageIndex;
            TotalCount = totalCount;

            if(totalCount < query.Count())
            {
                int start = PageIndex - 4;
                int end = PageIndex + 5;

                if (start <= 0)
                {
                    end = end - (start - 1);
                    start = 1;
                }

                if (end > TotalCount)
                {
                    end = TotalCount;
                    start = totalCount - 9;
                }

                StartPage = start;
                EndPage = end;
            }
            else
            {
                StartPage = 1;
                EndPage = totalCount;
            }

            this.AddRange(query);
        }
        public int PageIndex { get; }

        public int TotalCount { get; }

        public bool HasNext => PageIndex < TotalCount;
        public bool HasPrev => PageIndex > 1;
        public int StartPage { get; }
        public int EndPage { get; }
        public static PageNatedList<T> Create(IQueryable<T> query, int pageIndex, int itemCount)
        {
            int totalCount = (int)Math.Ceiling((decimal)query.Count() / itemCount);
            query = query.Skip((pageIndex - 1) * itemCount).Take(itemCount);

            return new PageNatedList<T>(query, pageIndex, totalCount);
        }
    }
}
