using System;
using System.Collections.Generic;
using System.Windows;
using AndromedaCore.Infrastructure;

namespace AndromedaCore.ViewModel
{
    public class WindowService : IWindowService
    {
        private readonly Dictionary<IntPtr, ApplicationWindowBase> _openWindows;

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
                if (!_openWindows.ContainsKey(win.WindowHandle))
                {
                    _openWindows.Add(win.WindowHandle, win);
                }
            };
            win.Owner = Application.Current.MainWindow;
            win.ShowAsTopmostDialog();
        }



        public void CloseAllWindows()
        {
            if (_openWindows.Keys.Count == 0) { return; }

            foreach (var key in _openWindows.Keys)
            {
                _openWindows[key].Close();
            }
        }

        private void OnWindowClose(object sender, EventArgs e)
        {
            var win = sender as ApplicationWindowBase;
            if (win == null) {return;}

            _openWindows.Remove(win.WindowHandle);
        }
    }
}