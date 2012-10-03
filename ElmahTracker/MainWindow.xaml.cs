using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ElmahTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private int _newIncomingErrorCount;
        private readonly TrayIcon _trayIcon;

        public MainWindow()
        {
            InitializeComponent();

            _trayIcon = new TrayIcon(this);
            Top = 0;
            Left = 100;
            Topmost = ConfigManager.Config.StayOnTop;

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        int count = ElmahErrorItem.PullItems();

                        if (count > 0)
                        {
                            // recount incoming errors
                            NewIncomingErrorCount += count;

                            // bind error list to list box
                            listBox1.Dispatcher.Invoke(
                                new Action(() => { listBox1.DataContext = ElmahErrorItem.LatestItems; }));
                        }
                    }
                    catch { }

                    // wait for next pull
                    Thread.Sleep(ConfigManager.Config.Interval * 1000);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public int NewIncomingErrorCount
        {
            get { return _newIncomingErrorCount; }
            set
            {
                _newIncomingErrorCount = value;

                if (value == 0)
                {
                    ErrorNotifier.Dispatcher.Invoke(
                        new Action(() => { ErrorNotifier.Visibility = Visibility.Hidden; }));

                    return;
                }

                ErrorCounterLabel.Dispatcher.Invoke(new Action(() =>
                {
                    ErrorNotifier.Visibility = Visibility.Visible;

                    ErrorCounterLabel.Text = value > 9
                        ? "N"
                        : value.ToString();
                }));
            }
        }

        private void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string uri = e.Uri.AbsoluteUri;

            Process.Start(new ProcessStartInfo(uri));

            e.Handled = true;
        }

        private void OnClickBar(object sender, MouseButtonEventArgs e)
        {
            switch (ListCanvas.Visibility)
            {
                case Visibility.Visible:
                    ListCanvas.Visibility = Visibility.Hidden;
                    break;
                case Visibility.Hidden:
                case Visibility.Collapsed:
                    ListCanvas.Visibility = Visibility.Visible;
                    NewIncomingErrorCount = 0;
                    break;
            }
        }

        private void OnDragBar(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            _trayIcon.Dispose();
        }
    }
}