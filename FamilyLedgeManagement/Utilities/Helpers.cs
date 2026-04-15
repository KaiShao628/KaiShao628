using BootstrapBlazor.Components;
using FamilyLedgeManagement.Dtos.DictionaryDtos;

namespace FamilyLedgeManagement.Utilities
{
    public static class Helpers
    {
        public static IEnumerable<int> PageItemSource => new int[] { 50 };

        public static long MaxFileLength => 200 * 1024 * 1024;

        public static IEnumerable<TreeViewItem<DictionaryDto>> CascadingDictionaryTree(IEnumerable<DictionaryDto> items) => items.Select(x => new TreeViewItem<DictionaryDto>(x));

    }
}
