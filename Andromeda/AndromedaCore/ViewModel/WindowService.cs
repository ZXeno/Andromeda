using System;
using System.Collections.Generic;
using System.Windows;

namespace AndromedaCore.ViewModel
{
    public class WindowService : IWindowService
    {
        private Dictionary<IntPtr, ApplicationWindowBase> _openWindows;

        public WindowService()
        {
            _openWindows = new Dictionary<IntPtr, ApplicationWindowBase>();
        }

        public void ShowWindow<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
            win.Closed += OnWindowClose;

            win.Closed += OnWindowClose;
            win.Activated += (sender, args) =>
            {
                _openWindows.Add(win.WindowHandle, win);
            };

            win.Show();
        }

        public void ShowDialog<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };

            win.Closed += OnWindowClose;
            win.Activated += (sender, args) => 
            {
                _openWindows.Add(win.WindowHandle, win);
            };
            win.Owner = Application.Current.MainWindow;
            win.ShowAsTopmostDialog();
        }

        private void OnWindowClose(object sender, EventArgs e)
        {
            var win = sender as ApplicationWindowBase;
            if (win == null) {return;}

            _openWindows.Remove(win.WindowHandle);
            
            win.Dispose();
        }
    }
}