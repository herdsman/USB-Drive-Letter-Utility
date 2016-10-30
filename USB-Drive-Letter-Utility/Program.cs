using System;
using System.Diagnostics;
using System.Management;

namespace USB_Drive_Letter_Utility
{
    internal class Program
    {
        private static readonly string _driveLetter = "W";


        private static void Main(string[] args)
        {
            //Create a New INI file to store or load data
            //IniFile ini = new IniFile(@"C:\users\USBDriveLetter.ini");
            //ini.IniWriteValue("Definitions", "DriveLetter", "W");


            try
            {
                //var ini = new IniFile(@"C:\users\USBDriveLetter.ini");
                //_driveLetter = ini.IniReadValue("Definitions", "DriveLetter");
            }
            catch (Exception)
            {
                // ignored
            }


            //Hello

            //http://stackoverflow.com/questions/3331043/get-list-of-connected-usb-devices

            var watcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
            watcher.EventArrived += watcher_EventArrived;
            watcher.Query = query;
            watcher.Start();


            do
            {
                watcher.WaitForNextEvent();
            } while (true);




            // ReSharper disable once FunctionNeverReturns
        }

        private static void watcher_EventArrived(object j, EventArrivedEventArgs e)
        {
            //var currentLetter = e.NewEvent.Properties["DriveName"].Value.ToString();

            DetectDriveAndChangeLetter();

        }

        private static void DetectDriveAndChangeLetter()
        {
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_Volume"))
                collection = searcher.Get();


            foreach (var device in collection)
            {
                if (device["DriveLetter"] != null &&
                   device["DriveType"].ToString() == "2") //2 - removable drive 
                {
                    if (!string.Equals(device["DriveLetter"].ToString().Substring(0, 1), _driveLetter.Trim(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        DiskPart(device["DriveLetter"].ToString().Substring(0,1));
                    }
                }
            }
        }



        //private static
        //    void DetectDriveAndChangeLetter
        //    (string
        //        driveLetter)
        //{
        //    //Console.WriteLine("DetectDriveAndChangeLetter");
        //    // Use the Storage management scope
        //    var scope = new ManagementScope(@"\\localhost\ROOT\Microsoft\Windows\Storage");
        //    // Define the query for volumes
        //    var queryName = new ObjectQuery("SELECT * FROM MSFT_Volume");
        //    // create the search for volumes
        //    var searcher = new ManagementObjectSearcher(scope, queryName);
        //    // Get the volumes
        //    var allVolumes = searcher.Get();
        //    //Console.WriteLine("{0}",allVolumes.Count);
        //    // Loop through all volumes
        //    foreach (ManagementObject oneVolume in allVolumes)
        //    {
        //        //Show volume information
        //        //Console.WriteLine("Volume '{0}' has {1} bytes total, {2} bytes available {3} drive type ",
        //        //        oneVolume["DriveLetter"],
        //        //        oneVolume["Size"], oneVolume["SizeRemaining"], oneVolume["DriveType"]);

        //        if (oneVolume["DriveLetter"] != null &&
        //            oneVolume["DriveType"].ToString() == "2") //2 - removable drive 
        //        {
        //            if (oneVolume["DriveLetter"].ToString().ToUpper() != driveLetter.Trim())
        //            {
        //                DiskPart(oneVolume, _driveLetter);
        //            }
        //        }
        //    }
        //}

        private static void DiskPart(string currentdriveletter)
        {
            var p = new Process
            {
                StartInfo =
                {
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    FileName = @"diskpart.exe",
                    CreateNoWindow = true,
                    RedirectStandardInput = true
                }
            };
            p.Start();
            p.StandardInput.WriteLine("select volume {0}", currentdriveletter);
            p.StandardInput.WriteLine("assign letter " + _driveLetter.Trim());
            p.StandardInput.WriteLine("exit");
            var output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }
    }
}