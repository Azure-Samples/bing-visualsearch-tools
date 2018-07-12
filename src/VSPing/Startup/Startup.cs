// This is a custom startup file added to intercept WPF App startup (as there is no explicit Main() )
// All App startup customizations are done here.

using System.Windows; // Application, StartupEventArgs, WindowState
using Microsoft.Win32;

namespace VSPing
{ 
    using VSPing.Views;

    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            // Unhandled exception handler
            Application.Current.DispatcherUnhandledException += UnhandledExceptionHandler;

            DockableWindow mainWindow = new DockableWindow();

            mainWindow.Show();
        }

        private void UnhandledExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string msg = 
                "Unhandled Exception raised!!\n\nPress Control-C to copy Msgbox content.\n\nThe app will shutdown when this dialog closes.\n\n" +
                e.Exception.ToString();

            System.Windows.Forms.MessageBox.Show(msg, "VS Ping unhandled exception", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);

            this.Shutdown();
        }
    }
}