using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
                #region Connection
                Stream networkStream = connectionSocket.GetStream();

                StreamWriter streamWriter = new StreamWriter(networkStream);
                streamWriter.AutoFlush = true;

                var shabzLock = ShabzLockConsumer.GetOneLockAsync(26).Result;

                int lockId = shabzLock.Id;

                string status = shabzLock.Status.ToString();

                string name = shabzLock.Name;
                
                string accessCode = shabzLock.AccessCode;

                string dateRegistrered = shabzLock.DateRegistered;

                if (counter == 1) //Sørger for at sætte doorLock korrekt hvis låsens status er true ved første gennemløb
                {
                    if (status == "True")
                    {
                        doorLock = "locked";
                    }
                    counter++;
                }
                #endregion

                #region Unlock 

                if (status == "False" && doorLock == "unlocked") //Startes kun hvis låsens status ændres
                {
                #region Greeting of sortering

                    var accountId = (from shabzLog in ShabzLockConsumer.GetLogAsync().Result
                        orderby shabzLog.Date.Last()
                        select shabzLog.AccountId).Last();

                    var logDate = (from shabzLog in ShabzLockConsumer.GetLogAsync().Result
                        orderby shabzLog.Date.Last()
                        select shabzLog.Date).Last();

                    var shabzAccount = ShabzLockConsumer.GetOneAccountAsync(accountId).Result;
                    string[] split = shabzAccount.Name.Split(' ');
                    DateTime christmas = new DateTime(DateTime.Now.Year, 12, 1, 0, 0, 0);

                    DateTime christmasOver = new DateTime(DateTime.Now.Year, 12, 26, 23, 59, 59);

                    DateTime logDateTime =
                        DateTime.ParseExact(logDate, "M/dd/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                    //DateTime logDateTime = new DateTime(DateTime.Now.Year, 12,31,21,30,0); // Til test

                    //DateTime logDateTime = new DateTime(DateTime.Now.Year, 2, 28, 21, 30, 0); // Til test

                    DateTime newYear = new DateTime(DateTime.Now.Year, 12, 31, 0, 0, 0);

                    DateTime newYearOver = new DateTime(DateTime.Now.Year, 1, 1, 23, 59, 59);

                    string welcomeGreeting = "WELCOME HOME " + split[0].ToUpper();

                    string christmasGreeting = "MERRY CHRISTMAS " + split[0].ToUpper();

                    string newYearGreeting = "HAPPY NEWYEAR " + split[0].ToUpper();



                    if (logDateTime > christmas && logDateTime < christmasOver || logDateTime > newYear || logDateTime < newYearOver)
                    {
                        if (logDateTime > christmas && logDateTime < christmasOver)
                        {
                            streamWriter.WriteLine("o" + christmasGreeting);
                            doorLock = "locked";
                        }
                        else
                        {
                            streamWriter.WriteLine("o" + newYearGreeting);
                            doorLock = "locked";
                        }
                    }
                    else
                    {
                        streamWriter.WriteLine("o" + welcomeGreeting);
                        doorLock = "locked";
                    }
                #endregion

                    Task.Run(() =>
                    {
                        return Task.Run(() =>
                        {
                            Task.Delay(20000).Wait(); //Låser døren automatisk hvis brugeren ikke selv gør det
                            var shabzAutomatic = ShabzLockConsumer.GetOneLockAsync(lockId).Result;

                            string shabzAutomaticStatus = shabzAutomatic.Status.ToString();

                            if (shabzAutomaticStatus == "False" && doorLock == "locked")
                            {
                                streamWriter.WriteLine("l");
                                Task.Run(() =>
                                    ShabzLockConsumer.UpdateLockAsync(new Lock(name, accessCode, true, dateRegistrered),
                                        lockId)); //Opdaterer låsens status
                                Task.Run(() => ShabzLockConsumer.AddLogAsync(new Log(47,
                                    string.Format("{0:hh:mm:ss tt}", DateTime.Now), true, lockId))); //Laver en log med ID 47(serverens ID)
                            }
                            Task.Delay(1000).Wait(); //Giver webservicen tid til at uploade ændringer
                            doorLock = "unlocked";
                        });
                    });
                }

                #endregion
                
                #region Lock
                if (status == "True" && doorLock == "locked") //Låser døren hvis brugeren selv trykker på låseknappen
                {
                    streamWriter.WriteLine("l");
                    doorLock = "unlocked";
                }
                #endregion

                #region Joystick
                Task.Run(() =>
                {
                    return Task.Run(() =>
                    {
                        byte[] buffer = new byte[connectionSocket.ReceiveBufferSize];
                        int bytesRead = networkStream.Read(buffer, 0, connectionSocket.ReceiveBufferSize);

                        string dataRecieved = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                        if (dataRecieved.Contains("joystick"))//Clienten sender besked indeholdende joystick 
                        {
                            if (dataRecieved.Contains("is locked"))//Låser døren hvis brugeren trykker på joysticket
                            {
                                Console.WriteLine("Shabzlock says: " + dataRecieved);
                                Task.Run(() => ShabzLockConsumer.UpdateLockAsync(new Lock(name, accessCode, true, dateRegistrered), lockId));//Opdaterer låsens status

                                Task.Run(() => ShabzLockConsumer.AddLogAsync(new Log(46,
                                    string.Format("{0:hh:mm:ss tt}", DateTime.Now), true, lockId)));//Laver en log med ID 46(joystickets ID)
                            }

                            if (dataRecieved.Contains("is unlocked"))//Åbner døren hvis brugeren trykker på joysticket
                            {
                                Console.WriteLine("Shabzlock says: " + dataRecieved);
                                doorLock = "joystick";
                                Task.Run(() => ShabzLockConsumer.UpdateLockAsync(new Lock(name, accessCode, false, dateRegistrered), lockId));//Opdaterer låsens status
                                Task.Run(() => ShabzLockConsumer.AddLogAsync(new Log(46,
                                    string.Format("{0:hh:mm:ss tt}", DateTime.Now), false, lockId)));//Laver en log med ID 46(joystickets ID)
                                
                                
                                Task.Delay(25000).Wait();//Låser døren automatisk ved tryk på joystick hvis brugeren ikke selv gør det
                                var shabzAutomatic = ShabzLockConsumer.GetOneLockAsync(lockId).Result;
                                string shabzAutomaticStatus = shabzAutomatic.Status.ToString();

                                if (shabzAutomaticStatus == "False" && doorLock == "joystick")
                                {
                                    streamWriter.WriteLine("l");
                                    Task.Run(() => ShabzLockConsumer.UpdateLockAsync(new Lock(name, accessCode, true, dateRegistrered), lockId));//Opdaterer låsens status
                                    Task.Run(() => ShabzLockConsumer.AddLogAsync(new Log(47,
                                        string.Format("{0:hh:mm:ss tt}", DateTime.Now), true, lockId)));//Laver en log med ID 47(serverens ID)
                                }
                                Task.Delay(1000).Wait(); //Giver webservicen tid til at uploade ændringer
                                doorLock = "unlocked";
                            }
                        }
                        else
                        {
                            Console.WriteLine("Shabzlock says: " + dataRecieved);
                        }
                        
                    });
                });
                #endregion
            }
        }
    }
}
