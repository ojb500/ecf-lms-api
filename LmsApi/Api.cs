using Ojb500.EcfLms.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ojb500.EcfLms
{
    public interface IApi
    {
        T[] Get<T>(string file, string org, string name);
    }


    public class Api : IApi
    {
        public static Api Default = new Api();


        public Api() : this("http://ecflms.org.uk/lms/lmsrest/league/")
        {
        }
        public Api(string baseAddress)
        {
            _baseAddress = baseAddress;
            _hc = CreateHttpClient(baseAddress);
            _js = JsonSerializer.CreateDefault();
            _js.Converters.Add(new LeagueTableEntryApiConverter());
            _js.Converters.Add(new PointsApiConverter());
            _js.Converters.Add(new EventApiConverter());
        }


        private async Task<Stream> GetJson(string file, string org, string name)
        {
            var result = await _hc.PostAsync(file, new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                new KeyValuePair<string, string>("org", org),
                new KeyValuePair<string, string>("name", name)
                })).ConfigureAwait(false);

            return await result.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }
        
        private readonly string _baseAddress;
        private readonly HttpClient _hc;
        private readonly JsonSerializer _js;

        private HttpClient CreateHttpClient(string baseAddress)
        {
            var hc = new HttpClient()
            {
                BaseAddress = new Uri(baseAddress)
            };

            hc.DefaultRequestHeaders.Accept.Clear();
            hc.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            return hc;
        }
        private T Deserialise<T>(Stream s)
        {
            var str = ReadString(s);

            using (var sr = new StringReader(str))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    return _js.Deserialize<T>(jtr);
                }
            }
        }

        private string ReadString(Stream s)
        {
            using (var sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        T[] IApi.Get<T>(string file, string org, string name)
        {
            return Deserialise<T[]>(GetJson(file, org, name).Result);
        }
    }
}
