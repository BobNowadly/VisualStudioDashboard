using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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

        public Task<HttpResponseMessage> PostAsync(string url, params KeyValuePair<string, string>[] formData)
        {
            var content = new FormUrlEncodedContent(formData);
            return client.PostAsync(url, content);
        }
    }
}