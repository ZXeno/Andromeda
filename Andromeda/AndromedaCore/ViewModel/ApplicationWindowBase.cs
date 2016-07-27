using System.Windows;

namespace AndromedaCore.ViewModel
{
    public class ApplicationWindowBase : Window
    {
        public ApplicationWindowBase()
        {
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs de)
        {
            if (de.NewValue is IRequestCloseViewModel)
            {
                // if the new datacontext supports the IRequestCloseViewModel we can use
                // the event to be notified when the associated viewmodel wants to close
                // the window
                ((IRequestCloseViewModel)de.NewValue).RequestClose += (s, e) => this.Close();
            }
        }
    }
}