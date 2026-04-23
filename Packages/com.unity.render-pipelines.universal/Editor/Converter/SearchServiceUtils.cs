using System.Collections.Generic;
using System;
using UnityEditor.Search;

namespace UnityEditor.Rendering
{
    static class SearchServiceUtils
    {
        [Flags]
        public enum IndexingOptions
        {
            None = 0,
            DeepSearch = 1 << 0,
            PackageIndexing = 1 << 1
        }

        public static void RunQueuedSearch(
            IndexingOptions neededOptions,
            List<(string query, string description)> contextSearchQueriesAndIds,
            Action<SearchItem, string> onAssetGUIDFound,
            Action onSearchsFinished)
        {
            int index = 0;
            void ProcessNextSearch()
            {
                if (index >= contextSearchQueriesAndIds.Count)
                {
                    // No need to rollback the index info, the initialization is done now
                    onSearchsFinished?.Invoke();
                    return;
                }
                var id = contextSearchQueriesAndIds[index].description;
                var query = contextSearchQueriesAndIds[index].query;
                var context = Search.SearchService.CreateContext(query);

                Search.SearchService.Request(context, (searchContext, searchItems) =>
                {
                    foreach (var item in searchItems)
                    {
                        onAssetGUIDFound.Invoke(item, id);
                    }

                    searchContext?.Dispose();
                    index++;
                    ProcessNextSearch();
                });
            }

            // Indexing is now continuous in Unity 6, no need to change settings
            ProcessNextSearch();
        }
    }
}
