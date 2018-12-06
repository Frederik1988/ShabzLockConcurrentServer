using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using ShabzLockConcurrent.Model;

namespace ShabzLockConcurrent
{
    class ShabzLockService
    {
        public TcpClient connectionSocket { get; set; }

        public ShabzLockService(TcpClient connectionSocket)
        {
            this.connectionSocket = connectionSocket;
        }

        public void clientConnection()
        {

            string doorLock = "unlocked";
            int counter = 1;

            while (true)
            {
                Stream networkStream = connectionSocket.GetStream();

                StreamWriter streamWriter = new StreamWriter(networkStream);
                streamWriter.AutoFlush = true;

                var shabzLock = ShabzConsumer.GetOneLockAsync(15).Result;

                int lockId = shabzLock.Id;

                string status = shabzLock.Status.ToString();

                string name = shabzLock.Name;

                string accessCode = shabzLock.AccessCode;

                string dateRegistrered = shabzLock.DateRegistered;

                if (counter == 1)
                {
                    if (status == "True")
                    {
                        doorLock = "locked";
                    }
                     counter++;
                }
                

                if (status == "False" && doorLock == "unlocked")
                {
                    

                    if (counter > 1)
                    {
                        streamWriter.WriteLine("o");
                        doorLock = "locked";
                        byte[] buffer = new byte[connectionSocket.ReceiveBufferSize];
                        int bytesRead = networkStream.Read(buffer, 0, connectionSocket.ReceiveBufferSize);

                        string dataRecieved = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Shabzlock says: " + dataRecieved);
                    }
                    counter++;

                    var lockAutomatically = Task.Factory.StartNew(() =>
                    {
                        return Task.Factory.StartNew(() =>
                        {
                            Task.Delay(15000).Wait();
                            if (status == "False" && doorLock == "locked")
                            {
                                streamWriter.WriteLine("l");
                                doorLock = "unlocked";
                                Task.Run(() => ShabzConsumer.UpdateLockAsync(new Lock(name, accessCode, true, dateRegistrered), lockId));
                                
                                byte[] buffer = new byte[connectionSocket.ReceiveBufferSize];
                                int bytesRead = networkStream.Read(buffer, 0, connectionSocket.ReceiveBufferSize);

                                string dataRecieved = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                                Console.WriteLine("Shabzlock says: " + dataRecieved);
                            }
                        });
                    });
                    lockAutomatically.Wait();
                }


                if (status == "True" && doorLock == "locked")
                {
                    
                    streamWriter.WriteLine("l");
                    doorLock = "unlocked";

                    byte[] buffer = new byte[connectionSocket.ReceiveBufferSize];
                    int bytesRead = networkStream.Read(buffer, 0, connectionSocket.ReceiveBufferSize);

                    string dataRecieved = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Shabzlock says: " + dataRecieved);
                  
                }
                
            }
        }
    }
}
