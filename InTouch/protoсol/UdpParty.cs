using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace InTouchLibrary.protoсol
{
    public static class UdpParty
    {
        public static async Task SendBinaryAsync(byte[] message, string ip, int port)
        {
            using var sender = new UdpClient();
            await sender.SendAsync(message, message.Length, ip, port);
        }

        public static async Task<UdpReceiveResult> ReceiveBinaryAsync(int port)
        {
            using var receiver = new UdpClient(port);
            return await receiver.ReceiveAsync();
        }
    }
}