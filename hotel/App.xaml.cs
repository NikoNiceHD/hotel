using System.Globalization;
using System.Threading;
using System.Windows;

namespace hotel
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Setze die globale Kultur auf Deutsch
            Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("de-DE");

            base.OnStartup(e);
        }
    }
}
