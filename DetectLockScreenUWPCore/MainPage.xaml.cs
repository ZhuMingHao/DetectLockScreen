using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DetectLockScreenUWPCore
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (ApiInformation.IsApiContractPresent("Windows.ApplicationModel.FullTrustAppContract", 1, 0))
            {
                Process process = Process.GetCurrentProcess();
                ApplicationData.Current.LocalSettings.Values["processID"] = process.Id;

                App.AppServiceConnected += AppServiceConnected;
                await FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();

                MessageDialog dlg = new MessageDialog("Registered");
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await dlg.ShowAsync();
                });
            }
        }

        private void AppServiceConnected(object sender, AppServiceTriggerDetails e)
        {
            e.AppServiceConnection.RequestReceived += AppServiceConnection_RequestReceived;
        }

        private async void AppServiceConnection_RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            AppServiceDeferral messageDeferral = args.GetDeferral();
            bool isLock = (bool)args.Request.Message["Lock"];
            switch (isLock)
            {
                case true://stingray
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        MessageDialog dlg = new MessageDialog("The Screen Locked");
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await dlg.ShowAsync();
                        });


                    });
                    break;
                case false://octopus
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        MessageDialog dlg = new MessageDialog("The Screen UnLocked");
                        await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                        {
                            await dlg.ShowAsync();
                        });


                    });
                    break;
                default:
                    break;
            }
            await args.Request.SendResponseAsync(new ValueSet());
            messageDeferral.Complete();

            // we no longer need the connection
            App.AppServiceDeferral.Complete();
            App.Connection = null;
        }

    }
}
