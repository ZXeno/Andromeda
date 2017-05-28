namespace AndromedaCore.ViewModel
{
    public class WindowService : IWindowService
    {
        public void ShowWindow<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel
            };
            win.Show();
        }

        public void ShowDialog<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel
            };
            
            win.ShowAsTopmostDialog();
        }
    }
}