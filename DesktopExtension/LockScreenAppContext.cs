using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.Storage;


namespace DesktopExtension
{
    class LockScreenAppContext : ApplicationContext
    {

        private Process process = null;
        private bool LockScreenInProgress = false;
        private HotKeyWindow hotkeyWindow = null;

        public LockScreenAppContext()
        {
            int processId = (int)ApplicationData.Current.LocalSettings.Values["processId"];
            process = Process.GetProcessById(processId);
            process.EnableRaisingEvents = true;
            process.Exited += HotkeyAppContext_Exited;
            hotkeyWindow = new HotKeyWindow();
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);

        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {

                case SessionSwitchReason.SessionLock:
                    SentMessage(true);
                    break;
                case SessionSwitchReason.SessionUnlock:
                    SentMessage(false);
                    break;

            }
        }

        private async void SentMessage(bool isLocked)
        {
            ValueSet ScreebLocked = new ValueSet();
            ScreebLocked.Add("Lock", isLocked);

            AppServiceConnection connection = new AppServiceConnection();
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.AppServiceName = "LockScreenConnection";
            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Debug.WriteLine(status);
                Application.Exit();
            }
            connection.ServiceClosed += Connection_ServiceClosed;
            AppServiceResponse response = await connection.SendMessageAsync(ScreebLocked);
        }

        private void HotkeyAppContext_Exited(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine("Connection_ServiceClosed");
            LockScreenInProgress = false;
        }

        private async void hotkeys_HotkeyPressed(int ID)
        {

            // bring the UWP to the foreground (optional)
            IEnumerable<AppListEntry> appListEntries = await Package.Current.GetAppListEntriesAsync();
            await appListEntries.First().LaunchAsync();

            // send the key ID to the UWP
            ValueSet hotkeyPressed = new ValueSet();
            hotkeyPressed.Add("ID", ID);

            AppServiceConnection connection = new AppServiceConnection();
            connection.PackageFamilyName = Package.Current.Id.FamilyName;
            connection.AppServiceName = "HotkeyConnection";
            AppServiceConnectionStatus status = await connection.OpenAsync();
            if (status != AppServiceConnectionStatus.Success)
            {
                Debug.WriteLine(status);
                Application.Exit();
            }
            connection.ServiceClosed += Connection_ServiceClosed;
            AppServiceResponse response = await connection.SendMessageAsync(hotkeyPressed);
        }
    }
    public class HotKeyWindow : NativeWindow
    {
      
       

        // creates a headless Window to register for and handle WM_HOTKEY
        public HotKeyWindow()
        {
            this.CreateHandle(new CreateParams());
            Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
        }

      

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            this.DestroyHandle();
        }

       
    }
}
