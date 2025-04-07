using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;


public class DisplayIPAddr : MonoBehaviour
{
    public TMP_Text ipAddrText;

    void Start()
    {
        ipAddrText.text = "HostIP: " + getIp();
    }

    private string getIp()
    {
        string ip = "";

        foreach (IPAddress addr in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                ip = addr.ToString();
                break;
            }
        }

        return ip;
    }
}
