using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dashboard
{
    public class TfsConnection
    {
        private readonly HttpClient client;

        public TfsConnection(string username, string password, HttpClient client)
        {
            this.client = client;

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", username, password))));
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            return client.GetAsync(url);
        }

        public Task<HttpResponseMessage> PostAsync(string url, KeyValuePair<string, string> formData)
        {
            var content = string.Format("{{\"{0}\":\"{1}\"}}", formData.Key, formData.Value);

            var json = JsonConvert.SerializeObject(new {query = formData.Value});

            return client.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));
        }
    }
}