using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace CodicExtension.Model
{
    public class CodicApi
    {

        public CodicApi()
        {

        }


        public async Task<List<Project>> GetUserProjectsAsync(string accessToken)
        {
            var data = Utils.BuildQuerystring(new Dictionary<string, string>());

            JArray json = await PostJsonArrayAsync(accessToken,
                "https://api.codic.jp/v1/user_projects.json", data);
            
            List<Project> projects = new List<Project>();
            foreach (JToken projectToken in json) {
                var project = new Project();
                project.Id = projectToken["id"].ToString();
                project.Name = projectToken["name"].ToString();
                projects.Add(project);
            }

            return projects;
        }

        public Task<TranslationResult> TranslateAsync(string accessToken, string projectId, string text)
        {
            return TranslateAsync(accessToken, projectId, text, null);
        }

        public async Task<TranslationResult> TranslateAsync(string accessToken, string projectId, string text, string letterCase)
        {
            var data = Utils.BuildQuerystring(new Dictionary<string, string>()
            {
                { "text", text},
                { "project_id", projectId},
                { "casing", letterCase}
            });

            JArray json = await PostJsonArrayAsync(accessToken,
    "https://api.codic.jp/v1.1/engine/translate.json", data);

            TranslationResult result = new TranslationResult();
            List<Translation> translations = new List<Translation>();
            foreach (JToken translationToken in json)
            {
                var translation = new Translation();
                translation.TranslatedText = translationToken["translated_text_in_casing"].ToString();
                List<Word> words = new List<Word>();
                foreach (JToken wordToken in translationToken["words"]) {
                    Word word = new Word();
                    List<string> candidates = new List<string>();
                    foreach (JToken candidateToken in wordToken["candidates"])
                    {
                        candidates.Add(candidateToken["text_in_casing"].ToString());
                    }
                    word.Successful = wordToken["successful"].ToObject<bool>();
                    word.Candidates = candidates;
                    words.Add(word);
                }
                translation.Words = words;
                translations.Add(translation);
            }

            result.Translations = translations;
            return result;
        }

        private async Task<JArray> PostJsonArrayAsync(string accessToken, string url, string data)
        {
            var client = new ConfigurableClient();
            client.Proxy = null;
            client.Timeout = 1000;
            client.Headers.Add("User-Agent", "Codic VS Extension/1.0");
            client.Headers.Add("Pragma", "no-cache");
            client.Headers.Add("Authorization", "Bearer " + accessToken);
            client.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=utf-8");

            try
            {
                Debug.WriteLine("Request : " + url + "?" + data);
                byte[] rawData = await client.UploadDataTaskAsync(url, Encoding.UTF8.GetBytes(data));
                return JArray.Parse(Encoding.UTF8.GetString(rawData));
                //return Encoding.UTF8.GetString(rawData);
            }
            catch (WebException e)
            {
                var responseStream = e.Response.GetResponseStream();
                string responseText = null;
                using (var reader = new StreamReader(responseStream))
                {
                    responseText = await reader.ReadToEndAsync();
                }
                JObject json = JObject.Parse(responseText);
                JToken error = json["errors"].Children().First();
                throw new ApiException(int.Parse(error["code"].ToString()), error["message"].ToString());
            }
        }
    }

    
}