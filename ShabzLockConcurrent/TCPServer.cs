using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ShabzLockConcurrent
{
    class TCPServer
    {
        static void Main(string[] args)
        {
            TcpListener serverSocket = new TcpListener(IPAddress.Any, 9576);
            serverSocket.Start();
            Console.WriteLine("Server started.");

            while (true)
            {
                TcpClient connectionSocket = serverSocket.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                ShabzLockService shabzLockService = new ShabzLockService(connectionSocket);
                Task.Run(() => shabzLockService.clientConnection());
            }

        }
    }
}
