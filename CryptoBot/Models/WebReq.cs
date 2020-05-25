using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CryptoBot.Models
{
    public class WebReq
    {
        public async Task<string> InfoById(string id)
        {
            using var client = new HttpClient();
            string content = await client.GetStringAsync("https://api.coinpaprika.com/v1/coins/" + id);
            return content;
        }
        public async Task<HttpStatusCode> RealCheck(string r)
        {
            try
            {
                using var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync("https://api.coinpaprika.com/v1/tickers/btc-bitcoin?quotes=" + r);
                response.EnsureSuccessStatusCode();

            }
            catch (HttpRequestException)
            {
                return HttpStatusCode.BadRequest;
            }
            return HttpStatusCode.OK;
        }
        public async Task<UserShow> GetTickers(UserList dBUserItemsDBFind, int ind, string real)
        {
            UserShow userShow = new UserShow();
            List<string> cryptos = new List<string>();
            for (int i = 0; i < dBUserItemsDBFind.users[ind].Currency.Count; i++)
            {
                cryptos.Add(dBUserItemsDBFind.users[ind].Currency[i].Crypto);
            }
            for (int i = 0; i < cryptos.Count; i++)
            {
                using var client = new HttpClient();
                string content = await client.GetStringAsync("https://api.coinpaprika.com/v1/tickers/" + cryptos[i] + "?quotes=" + real);
                Tickers json = JsonConvert.DeserializeObject<Tickers>(content);
                userShow.Ticker.Add(json);
            }
            return userShow;
        }
    }
}
