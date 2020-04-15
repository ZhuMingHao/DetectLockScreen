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
      
        public LockScreenAppContext()
        {
            int processId = (int)ApplicationData.Current.LocalSettings.Values["processId"];
            process = Process.GetProcessById(processId);
            process.EnableRaisingEvents = true;
            process.Exited += DesktopExtensionAppContext_Exited;
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

        private void DesktopExtensionAppContext_Exited(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine("Connection_ServiceClosed");
            LockScreenInProgress = false;
        }

      
    }
   
}
