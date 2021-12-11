using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DevIO.IntegrationTests.Helpers
{
    public static class ContentHelper
    {
        public static HttpContent CreateJsonContent<T>(T dataObject)
        {
            string serializedObj = JsonConvert.SerializeObject(dataObject);

            return new StringContent(serializedObj, Encoding.UTF8, "application/json");
        }

        public async static Task<T> ExtractObject<T>(HttpContent content)
        {
            string strContent = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(strContent);
        }
    }
}
