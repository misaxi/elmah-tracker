using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using ElmahTracker.Properties;

namespace ElmahTracker
{
    public class TrayIcon : IDisposable
    {
        private bool _disposed;
        private NotifyIcon _icon;

        public TrayIcon(Window window)
        {
            _icon = new NotifyIcon
            {
                Visible = true,
                Icon = Icon.FromHandle(Resources.ce16.GetHicon()),
                ContextMenu = new ContextMenu(new[]
                {
                    new MenuItem("&Show", (sender, args) => window.Show()),
                    new MenuItem("&Hide", (sender, args) => window.Hide()),
                    new MenuItem("Stay on &Top", (sender, args) => 
                    { 
                        window.Topmost = !window.Topmost;
                        ((MenuItem) sender).Checked = window.Topmost;
                    }),
                    new MenuItem("-"),
                    new MenuItem("Settings", (sender, args) => 
                    {
                        var dlg = new SettingsWindow { Owner = window };
                        dlg.ShowDialog(); 
                    }),
                    new MenuItem("-"),
                    new MenuItem("&Exit", (sender, args) => window.Close())
                })
            };

            _icon.ContextMenu.MenuItems[2].Checked = ConfigManager.Config.StayOnTop;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion

        ~TrayIcon()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    if (_icon != null)
                    {
                        _icon.ContextMenu.Dispose();
                        _icon.Icon.Dispose();
                        _icon.Dispose();
                    }
                }

                _icon = null;
                _disposed = true;
            }
        }
    }
}