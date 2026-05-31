using System.Windows;

namespace PhysX;

public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        var window = new MainWindow();

        if (e.Args.Contains("--smoke-test"))
        {
            window.Measure(new Size(1320, 840));
            var settingsWindow = new SettingsWindow();
            settingsWindow.Measure(new Size(560, 420));
            Shutdown(0);
            return;
        }

        MainWindow = window;
        MainWindow.Show();
    }
}
