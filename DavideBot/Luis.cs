using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace DavideBot
{
    public class LUISDavideBot
    {
        public static async Task<DavideBotLUIS> ParseUserInput(string strInput)
        {
            try
            {
                string strRet = string.Empty;
                string strEscaped = Uri.EscapeDataString(strInput);

                using (var client = new HttpClient())
                {
                    string uri = "<LUIS url here>" + strEscaped;
                    HttpResponseMessage msg = await client.GetAsync(uri);

                    if (msg.IsSuccessStatusCode)
                    {
                        var jsonResponse = await msg.Content.ReadAsStringAsync();
                        var _Data = JsonConvert.DeserializeObject<DavideBotLUIS>(jsonResponse);
                        return _Data;
                    }
                }
            }
            catch
            {

            }

            return null;
        }
    }


    public class DavideBotLUIS
    {
        public string query { get; set; }
        public Topscoringintent topScoringIntent { get; set; }
        public Intent[] intents { get; set; }
        public Entity[] entities { get; set; }
        public Dialog dialog { get; set; }
    }

    public class Topscoringintent
    {
        public string intent { get; set; }
        public float score { get; set; }
        public Action[] actions { get; set; }
    }

    public class Action
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public object[] parameters { get; set; }
    }

    public class Dialog
    {
        public string contextId { get; set; }
        public string status { get; set; }
    }

    public class Intent
    {
        public string intent { get; set; }
        public float score { get; set; }
        public Action1[] actions { get; set; }
    }

    public class Action1
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public object[] parameters { get; set; }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public float score { get; set; }
    }
}