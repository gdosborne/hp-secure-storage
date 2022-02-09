//to stop using hard-coded key, comment out this compiler directive
#define UseHardcodedKey

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

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
                //comment the directive and put the code to 
                // get Azure Secret (the Bing Auth Key)
#if UseHardcodedKey
                var client = new WebClient();

                //just some call to a service right now
                //this is just to demonstrate an http call

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

                    result = (T)(object)sampleAuthKey;
                }
#endif
            }
            catch (Exception ex) {
                throw;
            }

            return result;
        }
    }
}
