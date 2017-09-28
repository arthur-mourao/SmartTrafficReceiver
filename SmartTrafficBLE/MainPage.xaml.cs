using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using System.Diagnostics;
using SmartTrafficBLE.Assets;


namespace SmartTrafficBLE
{
    /// <summary>
    /// Main.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly BluetoothLEAdvertisementWatcher _watcher;

        public MainPage()
        {
            this.InitializeComponent();

            _watcher = new BluetoothLEAdvertisementWatcher();
            _watcher.Received += OnAdvertisementReceived;
            _watcher.ScanningMode = BluetoothLEScanningMode.Active;
            _watcher.Start();

            System.Diagnostics.Debug.WriteLine("\nBeacon Start");

        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            // Do whatever you want with the advertisement
            Debug.WriteLine("\nBeacon Receveid with RSSI:" + eventArgs.RawSignalStrengthInDBm);

            if (eventArgs.Advertisement.ManufacturerData.Any())
            {
                foreach (var manufacturerData in eventArgs.Advertisement.ManufacturerData)
                {
                    // Print the company ID + the raw data in hex format
                    var manufacturerDataString = $"0x{manufacturerData.CompanyId.ToString("X")}: {BitConverter.ToString(manufacturerData.Data.ToArray())}";
                    //Debug.WriteLine("Manufacturer data: " + manufacturerDataString);

                    var manufacturerDataArry = manufacturerData.Data.ToArray();

                    if (IsProximityBeaconPayload(manufacturerData.CompanyId, manufacturerDataArry))
                    {
                        Debug.WriteLine("iBeacon Frame: " + BitConverter.ToString(manufacturerDataArry));

                        var beaconFrame = new ProximityBeaconFrame(manufacturerDataArry);
                        Debug.WriteLine("iBeacon UUID: " + ((ProximityBeaconFrame)beaconFrame).UuidAsString);
                        Debug.WriteLine("iBeacon Major: " + ((ProximityBeaconFrame)beaconFrame).Major);
                        Debug.WriteLine("iBeacon Minor: " + ((ProximityBeaconFrame)beaconFrame).Minor);

                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            // Display these information on the Screen
                            BeaconInfo.Text = "Last Beacon: " + eventArgs.BluetoothAddress
                                + "\nRSSI: " + eventArgs.RawSignalStrengthInDBm;
                        });

                    }

                }
            }
        }

        public static bool IsProximityBeaconPayload(ushort companyId, byte[] manufacturerData)
        {
            return companyId == 0x004C &&
                   manufacturerData.Length >= 23 &&
                   manufacturerData[0] == 0x02 &&
                   manufacturerData[1] == 0x15;
        }

    }
}
