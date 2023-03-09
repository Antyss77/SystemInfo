using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        var processor = GetProcessorModel();
        var gpu = GetGpuModel();
        var disks = GetDiskModels();
        var ram = GetRamInfo();
        var temps = GetTemperatures();

        var data = new Dictionary<string, object>()
        {
            { "processor", processor },
            { "gpu", gpu },
            { "disks", disks },
            { "ram", ram },
            { "temperatures", temps }
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText("data.json", json);
    }

    static string GetProcessorModel()
    {
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
        foreach (var obj in searcher.Get())
        {
            return obj["Name"].ToString();
        }
        return "";
    }

    static string GetGpuModel()
    {
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
        foreach (var obj in searcher.Get())
        {
            return obj["Name"].ToString();
        }
        return "";
    }

    static List<Dictionary<string, object>> GetDiskModels()
    {
        var disks = new List<Dictionary<string, object>>();
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
        foreach (var obj in searcher.Get())
        {
            var disk = new Dictionary<string, object>();
            disk["model"] = obj["Model"].ToString();
            disk["size"] = long.Parse(obj["Size"].ToString()) / (1024 * 1024 * 1024); // Convert to GB
            disks.Add(disk);
        }
        return disks;
    }

    static Dictionary<string, object> GetRamInfo()
    {
        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
        long capacityBytes = 0;
        foreach (var obj in searcher.Get())
        {
            capacityBytes += long.Parse(obj["Capacity"].ToString());
        }
        long capacityGb = capacityBytes / (1024 * 1024 * 1024); // Convert to GB
        return new Dictionary<string, object>()
        {
            { "model", "RAM" },
            { "size", capacityGb }
        };
    }

    static Dictionary<string, object> GetTemperatures()
    {
        var temps = new Dictionary<string, object>();
        using var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
        foreach (var obj in searcher.Get())
        {
            string name = obj["InstanceName"].ToString();
            double temp = double.Parse(obj["CurrentTemperature"].ToString()) / 10.0 - 273.15; // Convert from Kelvin to Celsius
            temps[name] = temp;
        }
        return temps;
    }
}
