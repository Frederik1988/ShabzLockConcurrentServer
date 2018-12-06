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
    public class ShabzConsumer
    {
        private static string uri = "https://shabzsmartlock.azurewebsites.net/api/lock/";

        public static async Task<IList<Lock>> GetLockStatusAsync()
        {

            using (HttpClient client = new HttpClient())
            {

                Thread.Sleep(1000);

                string content = await client.GetStringAsync(uri);

                IList<Lock> shabzLock = JsonConvert.DeserializeObject<IList<Lock>>(content);

                return shabzLock;

            }

        }
        public static async Task<Lock> GetOneLockAsync(int id)
        {
            string requestUri = uri + 1;
            using (HttpClient client = new HttpClient())
            {
                Thread.Sleep(1000);

                HttpResponseMessage response = await client.GetAsync(requestUri);

                string str = await response.Content.ReadAsStringAsync();
                Lock l = JsonConvert.DeserializeObject<Lock>(str);
                return l;

            }
        }


        public static async Task<Lock> UpdateLockAsync(Lock newLock, int id)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUri = uri + 1;
                var jsonString = JsonConvert.SerializeObject(newLock);
                StringContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PutAsync(requestUri, content);
                string str = await response.Content.ReadAsStringAsync();
                Lock updateLock = JsonConvert.DeserializeObject<Lock>(str);
                return updateLock;
            }

        }

    }

}
