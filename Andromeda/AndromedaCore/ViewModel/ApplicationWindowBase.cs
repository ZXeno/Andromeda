using System.Windows;

namespace AndromedaCore.ViewModel
{
    public class ApplicationWindowBase : Window
    {
        public ApplicationWindowBase()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs de)
        {
            var model = de.NewValue as IRequestCloseViewModel;
            if (model != null)
            {
                // if the new datacontext supports the IRequestCloseViewModel we can use
                // the event to be notified when the associated viewmodel wants to close
                // the window
                model.RequestClose += (s, e) => Close();
            }
        }
    }
}