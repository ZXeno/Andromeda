namespace AndromedaCore.ViewModel
{
    public interface IWindowService
    {
        void ShowWindow<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new();
        void ShowDialog<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new();
    }
}