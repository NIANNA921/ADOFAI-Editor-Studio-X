using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ADOFAIEditorStudio.Multiplayer
{
    /// <summary>局域网里发现的一个房间。</summary>
    internal sealed class LanRoom
    {
        public string Name;
        public string Address;
        public int Port;
        public string Chart;
        public string Version;
        public DateTime SeenAt;
    }

    /// <summary>
    /// 局域网房间发现：主机每秒发一条 UDP 广播，客户端在面板打开时监听。
    ///
    /// **只用于局域网。** 跨公网/内网穿透时广播过不去，加入方仍然手动填
    /// `公网IP:端口`。这里不做任何服务器、账号或中继。
    ///
    /// 广播包只是「有人在 192.168.x.x:21587 开房」的提示，真正的谱面数据
    /// 走 <see cref="NetLink"/> 的 TCP 连接。UDP 包丢了最多是晚一秒发现，
    /// 不影响联机正确性。
    /// </summary>
    internal static class LanDiscovery
    {
        internal const int DiscoveryPort = 21588;
        private const string Magic = "AESX-MP";
        private const int RoomLifetimeSeconds = 1;
        private const int RoomExpirySeconds = 4;

        private sealed class Packet
        {
            public string magic;
            public int protocol;
            public string name;
            public int port;
            public string chart;
            public string version;
        }

        private static readonly ConcurrentDictionary<string, LanRoom> rooms =
            new ConcurrentDictionary<string, LanRoom>();

        private static UdpClient listener;
        private static Thread listenerThread;
        private static volatile bool listening;
        /// <summary>绑定失败后的重试时间，避免每帧重新建 socket。</summary>
        private static DateTime nextListenAttempt;

        private static UdpClient broadcaster;
        private static Thread broadcasterThread;
        private static volatile bool broadcasting;
        private static string broadcastName;
        private static string broadcastChart;
        private static string broadcastVersion;
        private static int broadcastPort;

        // ---------------------------------------------------------------- 发现

        internal static void StartListening()
        {
            if (listening || DateTime.UtcNow < nextListenAttempt)
            {
                return;
            }
            try
            {
                var client = new UdpClient();
                client.Client.SetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                client.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
                listener = client;
                listening = true;
                listenerThread = new Thread(() => ListenLoop(client))
                {
                    IsBackground = true,
                    Name = "AES-MP-Discovery"
                };
                listenerThread.Start();
            }
            catch
            {
                // 端口被占用 / 防火墙拒绝时不弹错：手动填 IP 仍然能联机。
                listener = null;
                listening = false;
                nextListenAttempt = DateTime.UtcNow.AddSeconds(3);
            }
        }

        internal static void StopListening()
        {
            listening = false;
            try
            {
                if (listener != null)
                {
                    listener.Close();
                }
            }
            catch
            {
            }
            listener = null;
            listenerThread = null;
            nextListenAttempt = DateTime.MinValue;
            rooms.Clear();
        }

        private static void ListenLoop(UdpClient client)
        {
            while (listening && ReferenceEquals(client, listener))
            {
                try
                {
                    IPEndPoint remote = null;
                    byte[] data = client.Receive(ref remote);
                    if (remote == null || data == null || data.Length == 0)
                    {
                        continue;
                    }
                    Packet packet = JsonConvert.DeserializeObject<Packet>(
                        Encoding.UTF8.GetString(data));
                    if (packet == null
                        || !string.Equals(packet.magic, Magic, StringComparison.Ordinal)
                        || packet.protocol != MultiplayerSession.ProtocolVersion
                        || packet.port < 1 || packet.port > 65535)
                    {
                        continue;
                    }

                    string address = remote.Address.ToString();
                    LanRoom room = new LanRoom
                    {
                        Name = string.IsNullOrWhiteSpace(packet.name) ? "未命名主机" : packet.name,
                        Address = address,
                        Port = packet.port,
                        Chart = packet.chart,
                        Version = packet.version,
                        SeenAt = DateTime.UtcNow
                    };
                    rooms[address + ":" + packet.port] = room;
                }
                catch
                {
                    // 关 listener 时 Receive 会抛；其余坏包直接忽略。
                    if (!listening || !ReferenceEquals(client, listener))
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>最近还在广播的房间；没有就返回 null。主线程调用。</summary>
        internal static LanRoom BestRoom()
        {
            LanRoom best = null;
            DateTime now = DateTime.UtcNow;
            foreach (KeyValuePair<string, LanRoom> pair in rooms)
            {
                LanRoom room = pair.Value;
                if ((now - room.SeenAt).TotalSeconds > RoomExpirySeconds)
                {
                    LanRoom removed;
                    rooms.TryRemove(pair.Key, out removed);
                    continue;
                }
                if (best == null || room.SeenAt > best.SeenAt)
                {
                    best = room;
                }
            }
            return best;
        }

        internal static int RoomCount
        {
            get
            {
                DateTime now = DateTime.UtcNow;
                int count = 0;
                foreach (LanRoom room in rooms.Values)
                {
                    if ((now - room.SeenAt).TotalSeconds <= RoomExpirySeconds)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        // ---------------------------------------------------------------- 广播

        internal static void StartBroadcast(string name, int port, string chartPath)
        {
            StopBroadcast();
            if (port < 1 || port > 65535)
            {
                return;
            }
            try
            {
                broadcastName = string.IsNullOrWhiteSpace(name) ? "主机" : name;
                broadcastChart = string.IsNullOrEmpty(chartPath)
                    ? string.Empty
                    : System.IO.Path.GetFileNameWithoutExtension(chartPath);
                broadcastVersion = Main.Mod == null ? string.Empty : Main.Mod.Version.ToString();
                broadcastPort = port;

                var client = new UdpClient();
                client.EnableBroadcast = true;
                broadcaster = client;
                broadcasting = true;
                broadcasterThread = new Thread(() => BroadcastLoop(client))
                {
                    IsBackground = true,
                    Name = "AES-MP-Broadcast"
                };
                broadcasterThread.Start();
            }
            catch
            {
                broadcaster = null;
                broadcasting = false;
            }
        }

        internal static void StopBroadcast()
        {
            broadcasting = false;
            try
            {
                if (broadcaster != null)
                {
                    broadcaster.Close();
                }
            }
            catch
            {
            }
            broadcaster = null;
            broadcasterThread = null;
        }

        private static void BroadcastLoop(UdpClient client)
        {
            while (broadcasting && ReferenceEquals(client, broadcaster))
            {
                try
                {
                    Packet packet = new Packet
                    {
                        magic = Magic,
                        protocol = MultiplayerSession.ProtocolVersion,
                        name = broadcastName,
                        port = broadcastPort,
                        chart = broadcastChart,
                        version = broadcastVersion
                    };
                    byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet));
                    client.Send(data, data.Length,
                        new IPEndPoint(IPAddress.Broadcast, DiscoveryPort));
                    SendToSubnetBroadcasts(client, data);
                }
                catch
                {
                    // 网卡切换 / 防火墙瞬断：下一轮重试，不影响 TCP 会话。
                }
                Thread.Sleep(RoomLifetimeSeconds * 1000);
            }
        }

        /// <summary>
        /// 255.255.255.255 在部分多网卡 / VPN 环境里只走默认网卡。把每个
        /// 网段的定向广播也发一遍，插网线、开热点或挂 VPN 时更容易被发现。
        /// </summary>
        private static void SendToSubnetBroadcasts(UdpClient client, byte[] data)
        {
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up
                        || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    foreach (UnicastIPAddressInformation unicast
                        in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily != AddressFamily.InterNetwork
                            || IPAddress.IsLoopback(unicast.Address)
                            || unicast.IPv4Mask == null)
                        {
                            continue;
                        }
                        byte[] ip = unicast.Address.GetAddressBytes();
                        byte[] mask = unicast.IPv4Mask.GetAddressBytes();
                        if (ip.Length != 4 || mask.Length != 4)
                        {
                            continue;
                        }
                        byte[] broadcast = new byte[4];
                        for (int i = 0; i < 4; i++)
                        {
                            broadcast[i] = (byte)(ip[i] | (byte)~mask[i]);
                        }
                        client.Send(data, data.Length,
                            new IPEndPoint(new IPAddress(broadcast), DiscoveryPort));
                    }
                }
            }
            catch
            {
                // 某个网卡取不到掩码就跳过；255.255.255.255 已经发过了。
            }
        }

        // ---------------------------------------------------------------- 本机地址

        private static string cachedLocalAddress;
        private static DateTime cachedLocalAddressAt;

        /// <summary>给主机面板显示的本机局域网 IPv4；取不到时返回 127.0.0.1。</summary>
        internal static string LocalIPv4()
        {
            if (!string.IsNullOrEmpty(cachedLocalAddress)
                && (DateTime.UtcNow - cachedLocalAddressAt).TotalSeconds < 10)
            {
                return cachedLocalAddress;
            }
            try
            {
                foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (ni.OperationalStatus != OperationalStatus.Up
                        || ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    foreach (UnicastIPAddressInformation unicast
                        in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily != AddressFamily.InterNetwork
                            || IPAddress.IsLoopback(unicast.Address))
                        {
                            continue;
                        }
                        string text = unicast.Address.ToString();
                        if (text.StartsWith("169.254.", StringComparison.Ordinal))
                        {
                            continue;
                        }
                        cachedLocalAddress = text;
                        cachedLocalAddressAt = DateTime.UtcNow;
                        return text;
                    }
                }
            }
            catch
            {
            }
            cachedLocalAddress = "127.0.0.1";
            cachedLocalAddressAt = DateTime.UtcNow;
            return cachedLocalAddress;
        }
    }
}
