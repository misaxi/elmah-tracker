using System;
using System.Threading;
using System.Windows;

namespace ElmahTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var id = "{0DB755CF-FC78-4913-A1CE-8E3D17A61DD9}";

            bool createNew;

            var mutex = new Mutex(true, id, out createNew);

            if (createNew)
            {
                using (mutex)
                {
                    var app = new App();
                    app.InitializeComponent();
                    app.Run();
                }
            }
        }
    }
}
