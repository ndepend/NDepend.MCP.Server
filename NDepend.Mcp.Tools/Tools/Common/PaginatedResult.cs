using NDepend.Mcp.Helpers;

namespace NDepend.Mcp.Tools.Common {

    [Description("This base class let's paginate the server's response to a list items request from the client.")]
    public class PaginatedResult {

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
             string? nextCursor = null) {
            PaginatedCount = paginatedCount;
            TotalCount = totalCount;
            NextCursor = nextCursor;
        }


        [Description("Gets or sets the number of items paginated in the sequence Issues.")]
        public int PaginatedCount { get; set; }

        [Description("Gets or sets the total number of items matched by the request. " +
                     "Useful when the client requests only the total count without retrieving all items.")]
        public int TotalCount { get; set; }

        [Description("Gets or sets the server's response to a list items request from the client.")]
        public string? NextCursor { get; set; }

        internal static PaginatedResult Build<T,C>(
                ILogger<C> logger,
                IEnumerable<T> items, 
                string? cursor, 
                int pageSize,
                out IEnumerable<T> paginatedItems) {
            int count = items.Count();

            // Apply pagination
            int startIndex = 0;
            if (cursor.IsValid()) {
                if (int.TryParse(cursor, out int cursorTmp)) {
                    startIndex = cursorTmp;
                }
            }
            //  if(startIndex > count)   Skip(startIndex) will return an empty sequence
            //  Take(pageSize) will return available paginatedItems, even if it's less than pageSize.
            paginatedItems = items.Skip(startIndex).Take(pageSize);
            int endIndex = Math.Min(startIndex + pageSize, count);
            string? nextCursor = endIndex == count ? null : endIndex.ToString();

            logger.LogInformation($"{count} total items. Pagination returns items from {startIndex} till {endIndex}.");

            return new PaginatedResult(
                paginatedCount: endIndex - startIndex,
                totalCount: count,
                nextCursor: nextCursor);
        }
    }
}
