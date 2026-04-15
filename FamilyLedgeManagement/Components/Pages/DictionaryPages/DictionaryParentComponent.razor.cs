namespace FamilyLedgeManagement.Components.Pages.DictionaryPages
{
    public partial class DictionaryParentComponent
    {

        private DictionaryChildRight? _rightTable;
        private bool _loading;
        private bool Loading
        {
            get => _loading;
            set
            {
                if (_loading != value)
                {
                    _loading = value;
                    StateHasChanged();
                }
            }
        }

        private Action _openLoading => () => Loading = true;
        private Action _closeLoading => () => Loading = false;

        private Action<string> _treeSelectedIdChange => async (value) =>
        {
            await _rightTable.OnClickSearchAsync(value);
            await _rightTable.ResetSelectItemsAsync();
        };


    }
}