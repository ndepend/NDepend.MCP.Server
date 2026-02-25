
namespace NDepend.Mcp.Tools.Common {

    [Description("Paginates server list responses to the client.")]
    public class PaginatedResult {

        internal const string PAGINATION_CURSOR_DESC = "Cursor for pagination; `0` starts from the beginning.";

        internal static readonly PaginatedResult Empty = new PaginatedResult(
            paginatedCount: 0,
            totalCount: 0,
            nextCursor: -1);

        // ctor called by derived classes
        protected PaginatedResult(PaginatedResult paginatedResult) {
            PaginatedCount = paginatedResult.PaginatedCount;
            TotalCount = paginatedResult.TotalCount;
            NextCursor = paginatedResult.NextCursor;
        }

        // ctor called by the Build() method below
        private PaginatedResult(
             int paginatedCount,
             int totalCount,
             int nextCursor) {
            PaginatedCount = paginatedCount;
            TotalCount = totalCount;
            NextCursor = nextCursor;
        }


        [Description("Number of items in the current page")]
        public int PaginatedCount { get; set; }

        [Description("Total items matching the request")]
        public int TotalCount { get; set; }

        [Description("Next page cursor, or -1 if no more page available")]
        public int NextCursor { get; set; }

        internal static PaginatedResult Build<T,C>(
                ILogger<C> logger,
                IEnumerable<T> items, 
                int cursor, 
                int pageSize,
                string maxPageSizeStr, // Must be a string to be includable in 'pageSize' parameter description literal string
                out IEnumerable<T> paginatedItems) {
            int count = items.Count();

            // Apply pagination
            int startIndex = cursor >= 0 ? cursor : 0;
            int maxPageSize = int.Parse(maxPageSizeStr);
            if(pageSize <= 0 || pageSize > maxPageSize) { pageSize = maxPageSize; }

            //  if(startIndex > count)   Skip(startIndex) will return an empty sequence
            //  Take(pageSize) will return available paginatedItems, even if it's less than pageSize.
            paginatedItems = items.Skip(startIndex).Take(pageSize);
            int endIndex = Math.Min(startIndex + pageSize, count);
            int nextCursor = endIndex == count ? -1 : endIndex;

            logger.LogInformation($"{count} total items. Pagination returns items from {startIndex} till {endIndex}.");

            return new PaginatedResult(
                paginatedCount: endIndex - startIndex,
                totalCount: count,
                nextCursor: nextCursor);
        }
    }
}
