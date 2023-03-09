using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

class Program {
    static void Main(string[] args) {
        Console.Title = "Informations système";
        Console.WriteLine("Adresse IP : " + GetIPAddress());
        Console.WriteLine("Nom de l'OS : " + GetOSName());
        Console.WriteLine("Modèle de la carte graphique : " + GetGraphicCardModel());
        Console.WriteLine("Modèle de processeur : " + GetProcessorModel());
        Console.WriteLine("Quantité de RAM : " + GetRAMSize());
        Console.WriteLine("Espace disque libre : " + GetFreeDiskSpace());
        Console.WriteLine("Température du CPU / GPU : ");
        Console.WriteLine(GetTemperatureInfo());
        Console.WriteLine("Ports ouverts : ");

        foreach (var port in GetOpenPorts())
        {
            Console.WriteLine(" * " + port);
        }

        SaveDataToJSON();
        Console.Read();
    }

    static string GetTemperatureInfo() {
        var searcher = new ManagementObjectSearcher("root\\cimv2", "SELECT * FROM Win32_TemperatureProbe");
        var temps = searcher.Get();
        var tempInfo = new StringBuilder();
        foreach (var temp in temps)
        {
            var name = temp.GetPropertyValue("Name")?.ToString() ?? "Unknown";
            var reading = temp.GetPropertyValue("CurrentReading")?.ToString() ?? "Unknown";
            tempInfo.AppendLine($"{name}: {reading}°C");
        }

        return tempInfo.ToString();
    }


    static int[] GetOpenPorts() {
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] endPoints = properties.GetActiveTcpListeners();
        return endPoints.Select(endPoint => endPoint.Port).ToArray();
    }

    static string GetFreeDiskSpace() {
        DriveInfo drive = new DriveInfo("C");
        return drive.TotalFreeSpace / (1024 * 1024 * 1024) + " Go";
    }

    static string GetIPAddress() {
        string ip = new WebClient().DownloadString("http://checkip.dyndns.org/");
        ip = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
            .Matches(ip)[0].ToString();
        return ip;
    }

    static string GetOSName() {
        var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get()
                .Cast<ManagementObject>()
            select x.GetPropertyValue("Caption")).FirstOrDefault();
        return name != null ? name.ToString() : "Unknown";
    }

    static string GetGraphicCardModel() {
        var name = (from x in new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController").Get()
                .Cast<ManagementObject>()
            select x.GetPropertyValue("Name")).FirstOrDefault();
        return name != null ? name.ToString() : "Unknown";
    }

    static string GetProcessorModel() {
        var name = (from x in new ManagementObjectSearcher("SELECT Name FROM Win32_Processor").Get()
                .Cast<ManagementObject>()
            select x.GetPropertyValue("Name")).FirstOrDefault();
        return name != null ? name.ToString() : "Unknown";
    }

    static string GetRAMSize() {
        var name = (from x in new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem")
                .Get().Cast<ManagementObject>()
            select x.GetPropertyValue("TotalVisibleMemorySize")).FirstOrDefault();
        double size = Convert.ToDouble(name) / (1024 * 1024);
        return size.ToString("0.00") + " Go";
    }

    static void SaveDataToJSON() {
        try
        {
            string filePath = "data.json";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
                Console.WriteLine("Le fichier de sauvegarde a été créé");
            }

            using (StreamWriter sw = new StreamWriter(filePath)) {
                sw.WriteLine("Adresse IP : {0}", GetIPAddress());
                sw.WriteLine("Nom de l'OS : {0}", GetOSName());
                sw.WriteLine("Modèle de la carte Graphique : {0}", GetGraphicCardModel());
                sw.WriteLine("Modèle du processeur : {0}", GetProcessorModel());
                sw.WriteLine("Quantité de ram : {0}", GetRAMSize());
                sw.WriteLine("Espace disque libre : {0}", GetFreeDiskSpace());
                sw.WriteLine("Température du CPU / GPU : ");
                sw.WriteLine(GetTemperatureInfo());
                sw.WriteLine("Ports ouverts : ");
                foreach (var port in GetOpenPorts())
                {
                    sw.WriteLine(" * {0}", port);
                }
            }

            Console.WriteLine("Les données ont été écrites dans le fichier de sauvegarde");
        }
        catch (Exception e)
        {
            StackTrace stackTrace = new StackTrace(e, true);
            Console.WriteLine(stackTrace.ToString());
        }
    }
}