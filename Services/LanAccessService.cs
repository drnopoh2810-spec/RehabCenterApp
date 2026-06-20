using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RehabCenterApp.Services;

public class LanAccessService
{
    private readonly DatabaseService _dbService;

    public LanAccessService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public List<string> GetLocalIpAddresses()
    {
        var ips = new List<string>();
        try
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                foreach (var addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        ips.Add(addr.Address.ToString());
                }
            }
        }
        catch
        {
            ips.Add("127.0.0.1");
        }
        return ips;
    }

    public bool IsAllowedAccess(string allowedSubnet)
    {
        if (string.IsNullOrWhiteSpace(allowedSubnet))
            return true;

        var localIps = GetLocalIpAddresses();
        return localIps.Any(ip => ip.StartsWith(allowedSubnet) || ip == "127.0.0.1");
    }

    public async Task<string> GetAllowedSubnetAsync()
    {
        return await _dbService.GetSettingAsync("AllowedSubnet") ?? "192.168.";
    }

    public string GetPrimaryLocalIp()
    {
        var ips = GetLocalIpAddresses();
        return ips.FirstOrDefault(ip => ip != "127.0.0.1") ?? "127.0.0.1";
    }
}
