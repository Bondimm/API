using CoinpaprikaAPI.Entity;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CryptoBot.Models
{
    public class Value
    {
        [JsonProperty("value")]
        public List<CoinInfo> value { get; set; }
        public int count { get; set; }
    }
    public class Tickers
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("total_supply")]
        public long TotalSupply { get; set; }

        [JsonProperty("max_supply")]
        public long MaxSupply { get; set; }

        [JsonProperty("quotes")]
        public Dictionary<string, MyQuotes> Quotes { get; set; }
    }
    public class MyQuotes
    {
        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("percent_change_1h")]
        public decimal PercentChange1H { get; set; }

        [JsonProperty("percent_change_12h")]
        public decimal PercentChange12H { get; set; }

        [JsonProperty("percent_change_24h")]
        public decimal PercentChange24H { get; set; }
    }
    public class CoinInformation
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("is_new")]
        public bool Is_new { get; set; }
        [JsonProperty("is_active")]
        public bool Is_active { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
    }
    public class UserCurrency
    {
        public string crypto { get; set; }
    }
    public class UserCurrencyToChange
    {
        public string real { get; set; }
        public string crypto { get; set; }
        public string crypto_to_change { get; set; }
    }
    //public class UserCurrencyToDelete
    //{
    //    public string real { get; set; }
    //    public string crypto { get; set; }
    //}
    public class UserId
    {
        public string Id { get; set; }
    }
    public class UserInfo
    {
        public string Id { get; set; }
        public string Real { get; set; }
        public List<UserMoney> Currency { get; set; }
    }
    public class UserList
    {
        public List<UserInfo> users { get; set; }
    }
    public class UserMoney
    {
        public string Crypto { get; set; }
    }
    public class UserShow
    {
        public List<Tickers> Ticker = new List<Tickers>();
    }
}
