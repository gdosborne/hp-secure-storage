using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;

namespace HP.Palette.Security {
    internal class APIKeyService {
        private string appID = "1d6e13f6";
        private string appKey = "2ddc8f35bd078e731b2363397d1f27e3";
        //private string location = "us,50322";
        private double latitude = 41.619549;
        private double longitude = -93.598022;
        private string sampleAuthKey = "{B786CC25-4A3B-44D8-8A65-1D7A607CB72A}";

        public T GetRemoteKey<T>(string keyName) {
            var result = default(T);

            try {
                var client = new WebClient();

                //just some call to a service right now

                client.Headers["Content-type"] = "application/json";

                var fullUrl = $"http://" +
                    $"api.weatherunlocked.com/api/trigger/{latitude},{longitude}/" +
                    $"forecast%20tomorrow%20weather%20eq%20anyprecip?app_id={appID}&app_key={appKey}";
                var data = client.DownloadData(fullUrl);

                using (var stream = new MemoryStream(data)) {
                    stream.Position = 0;
                    var sr = new StreamReader(stream);
                    var json = sr.ReadToEnd();
                    var res = JObject.Parse(json);

                    var responseText = json;
                    var conditionMatched = res["ConditionMatched"].ToString();
                    var conditionMatchedNum = res["ConditionMatchedNum"].ToString();

                    //this is hard-coded now - some point will call to get
                    result = (T)(object)sampleAuthKey;
                }

            }
            catch (Exception ex) {
                throw;
            }

            return result;
        }
    }
}
