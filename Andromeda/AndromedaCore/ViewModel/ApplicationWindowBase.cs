using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace AndromedaCore.ViewModel
{
    public class ApplicationWindowBase : Window, IDisposable
    {
        public IntPtr WindowHandle { get; set; }
        public bool MinimizeToTray
        {
            get
            {
                var vmContext = DataContext as IViewModelBase;
                return vmContext != null && vmContext.MinimizeWindowToTray;
            }
        }

        protected NotifyIcon TrayIcon;

        public ApplicationWindowBase()
        {
            DataContextChanged += OnDataContextChanged;
            Activated += OnOpen;
        }

        /// <summary>
        /// Always opens a new Dialog as the top-most window.
        /// </summary>
        /// 
        public void ShowAsTopmostDialog()
        {
            Topmost = true;
            ShowDialog();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs de)
        {
            var model = de.NewValue as IRequestCloseViewModel;
            if (model != null)
            {
                model.RequestClose += OnClose;
            }

            IViewModelBase vmContext = DataContext as IViewModelBase;
            if (vmContext != null && vmContext.MinimizeWindowToTray)
            {
                SetNotificationTrayIcon();
            }
        }

        private void OnClose(object sender, EventArgs de)
        {
            var model = DataContext as IRequestCloseViewModel;
            if (model != null)
            {
                model.RequestClose -= OnClose;
            }

            Close();
        }

        private void OnOpen(object sender, EventArgs de)
        {
            WindowHandle = new WindowInteropHelper(this).Handle;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            IViewModelBase vmContext = DataContext as IViewModelBase;
            if (vmContext != null && vmContext.MinimizeWindowToTray)
            {
                switch (WindowState)
                {
                    case WindowState.Minimized:
                        if (TrayIcon == null)
                        {
                            SetNotificationTrayIcon();
                        }
                        TrayIcon.Visible = true;
                        Hide();
                        break;
                    case WindowState.Normal:
                        TrayIcon.Visible = false;
                        break;
                }
            }

            base.OnStateChanged(e);
        }

        protected void SetNotificationTrayIcon()
        {
            TrayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.appicon,
                Visible = true
            };

            TrayIcon.DoubleClick +=
                delegate
                {
                    Show();
                    WindowState = WindowState.Normal;
                };
        }

        public void Dispose()
        {
            TrayIcon?.Dispose();
        }
    }
}