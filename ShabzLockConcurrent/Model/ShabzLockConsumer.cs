using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ShabzLockConcurrent.Model
{
    public class ShabzLockConsumer
    {
        private static string uri = "https://shabzsmartlock.azurewebsites.net/api/lock/";

        private static string uriLog = "https://shabzsmartlock.azurewebsites.net/api/log/";

        private static string uriAccount = "https://shabzsmartlock.azurewebsites.net/api/account/";

        public static async Task<IList<Log>> GetLogAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                Thread.Sleep(400);
                string content = await client.GetStringAsync(uriLog);

                IList<Log> shabzLog = JsonConvert.DeserializeObject<IList<Log>>(content);

                return shabzLog;
            }

        }
        public static async Task<Lock> GetOneLockAsync(int id)
        {
            Thread.Sleep(20);

            string requestUri = uri + id;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(requestUri);

                string str = await response.Content.ReadAsStringAsync();
                Lock l = JsonConvert.DeserializeObject<Lock>(str);
                return l;
            }
        }

        public static async Task<Account> GetOneAccountAsync(int id)
        {
            string requestUri = uriAccount + id;
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(requestUri);

                string str = await response.Content.ReadAsStringAsync();
                Account a = JsonConvert.DeserializeObject<Account>(str);
                return a;
            }
        }


        public static async Task<Lock> UpdateLockAsync(Lock newLock, int id)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUri = uri + id;
                var jsonString = JsonConvert.SerializeObject(newLock);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(requestUri, content);
                string str = await response.Content.ReadAsStringAsync();
                Lock updateLock = JsonConvert.DeserializeObject<Lock>(str);
                return updateLock;
            }
        }

        public static async Task<Log> AddLogAsync(Log newLog)
        {
            using (HttpClient client = new HttpClient())
            {
                var jsonString = JsonConvert.SerializeObject(newLog);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(uriLog, content);
                
                string str = await response.Content.ReadAsStringAsync();
                Log copyOfNewLog = JsonConvert.DeserializeObject<Log>(str);
                return copyOfNewLog;
            }
        }
    }
}
