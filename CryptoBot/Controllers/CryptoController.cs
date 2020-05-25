using CryptoBot.Models;
using CryptoBot.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CryptoBot.Controllers
{
    [Produces("application/json")]
    [Route("[controller]")]
    public class CryptoController : Controller
    {
        private const string path = @"DB.json";
        private static CoinpaprikaAPI.Client client;
        [HttpGet]
        [Route("coinslist")]
        public async Task<Dictionary<string, string>> CoinsList()
        {
            client = new CoinpaprikaAPI.Client();
            var coins = await client.GetCoinsAsync();
            string s = JsonConvert.SerializeObject(coins);
            Value st = JsonConvert.DeserializeObject<Value>(s);
            Dictionary<string, string> names = new Dictionary<string, string>();
            int lenght = st.value.Count;
            for (int i = 0; i < lenght; i++)
            {
                string low = st.value[i].Name.ToLower();
                names.Add(st.value[i].Id, low);
            }
            return names;
        }
        [HttpGet]
        [Route("coins")]
        public async Task<List<string>> Coins()
        {
            client = new CoinpaprikaAPI.Client();
            var coins = await client.GetCoinsAsync();
            string s = JsonConvert.SerializeObject(coins);
            Value st = JsonConvert.DeserializeObject<Value>(s);
            List<string> names = new List<string>();
            int lenght;
            if (st.value == null)
            {
                return null;
            }
            lenght = st.value.Count;
            //int lenght = st.value.Count;
            for (int i = 0; i < lenght; i++)
            {
                names.Add(st.value[i].Name);
            }
            return names;
        }

        [HttpGet]
        [Route("coins/info/{id}")]
        public async Task<JsonResult> GetById(string id)
        {
            WebReq req = new WebReq();
            string response = await req.InfoById(id);
            CoinInformation ci = JsonConvert.DeserializeObject<CoinInformation>(response);
            ci.Description = Regex.Replace(ci.Description, @"\t|\n|\r", "");
            return Json(ci);
        }

        [HttpPost]
        [Route("adduser")]
        public async Task<ObjectResult> AddUser([FromBody] UserId item1)
        {
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            bool cont = false;
            UserMoney p2 = new UserMoney
            {
                Crypto = "btc-bitcoin"
            };
            List<UserMoney> users2 = new List<UserMoney>();
            users2.Add(p2);
            if (dBUserItemsDBFind == null)
            {
                UserInfo p1 = new UserInfo
                {
                    Id = item1.Id,
                    Real = "usd",
                    Currency = users2
                };
                List<UserInfo> users = new List<UserInfo>();
                users.Add(p1);
                UserList info = new UserList
                {
                    users = new List<UserInfo>(users)
                };

                await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(info, Formatting.Indented));
                dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind.users.Any(x => x.Id == item1.Id))
            {
                cont = true;
                Response.StatusCode = 409;
                return new ObjectResult(null);
            }
            if (cont == false)
            {
                dBUserItemsDBFind.users.Add(new UserInfo { Id = item1.Id, Real = "usd", Currency = users2 });
                await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
                return new ObjectResult(null);
            }
            return new ObjectResult(null);
        }

        [HttpPost]
        [Route("useraddcrypto/{id}")]
        public async Task<ObjectResult> AddCrypto([FromBody] UserCurrency item, string id)
        {
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, id);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            client = new CoinpaprikaAPI.Client();
            var coins = await client.GetCoinsAsync();
            string s = JsonConvert.SerializeObject(coins);
            Value st = JsonConvert.DeserializeObject<Value>(s);
            Dictionary<string, string> names = new Dictionary<string, string>();
            int lenght = st.value.Count;
            for (int i = 0; i < lenght; i++)
            {
                st.value[i].Name = st.value[i].Name.ToLower();
                names.Add(st.value[i].Id, st.value[i].Name);
            }
            item.crypto = item.crypto.ToLower();
            item.crypto = names.FirstOrDefault(x => x.Value == item.crypto).Key;
            string item1 = item.crypto;
            if (item1 == null)
            {
                Response.StatusCode = 400;
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind.users[comm.Index].Currency == null)
            {
                UserMoney p1 = new UserMoney
                {
                    Crypto = item1,
                };
                List<UserMoney> users = new List<UserMoney>();
                users.Add(p1);
                dBUserItemsDBFind.users[comm.Index].Currency = users;
                await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
            }
            else
            {
                for (int i = 0; i < dBUserItemsDBFind.users[comm.Index].Currency.Count; i++)
                {
                    if (dBUserItemsDBFind.users[comm.Index].Currency[i].Crypto == item1)
                    {
                        Response.StatusCode = 409;
                        item1 = null;
                        return new ObjectResult(null);

                    }
                }
                dBUserItemsDBFind.users[comm.Index].Currency.Add(new UserMoney() { Crypto = item1 });
            }
            if (dBUserItemsDBFind.users[comm.Index].Real == null)
            {
                dBUserItemsDBFind.users[comm.Index].Real = "usd";
            }
            await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
            return new ObjectResult(null);
        }

        [HttpPut]
        [Route("useredit/{ids}")]
        public async Task<ObjectResult> UserEdit([FromBody] UserCurrencyToChange item, string ids)
        {
            WebReq req = new WebReq();
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, ids);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind == null)
            {
                Response.StatusCode = 404;
            }
            //bool cont = false;
            //int comm.Index = 0;
            //for (int i = 0; i < dBUserItemsDBFind.users.Count; i++)
            //{
            //    if (dBUserItemsDBFind.users[i].Id == ids)
            //    {
            //        comm = i;
            //        cont = true;
            //        break;
            //    }
            //}
            //if (cont == false)
            //{
            //    Response.StatusCode = 404;
            //    return new ObjectResult(null);
            //}
            //if(item.real != null)
            //{
            //    item.real = comm.InputCheckReal(item.real);
            //}
            //if (item.crypto != null)
            //{
            //    item.crypto = comm.InputCheckCrypto(item.crypto);
            //}
            //if (item.crypto_to_change != null)
            //{
            //    item.crypto_to_change = comm.InputCheckCrypto(item.crypto_to_change);
            //}
            client = new CoinpaprikaAPI.Client();
            var coins = await client.GetCoinsAsync();
            string s = JsonConvert.SerializeObject(coins);
            Value st = JsonConvert.DeserializeObject<Value>(s);
            Dictionary<string, string> names = new Dictionary<string, string>();
            int lenght = st.value.Count;
            for (int i = 0; i < lenght; i++)
            {
                st.value[i].Name = st.value[i].Name.ToLower();
                names.Add(st.value[i].Id, st.value[i].Name);
            }
            string temp = names.FirstOrDefault(x => x.Value == item.crypto_to_change).Key;
            string temp2 = names.FirstOrDefault(x => x.Value == item.crypto).Key;
            if (temp != null)
            {
                temp = temp.ToLower();
            }
            if (temp2 != null)
            {
                temp2 = temp2.ToLower();
            }
            item.crypto = temp;
            if (item.real != null)
            {
                item.real = item.real.ToLower();
            }
            string item2 = item.real;
            var status = await req.RealCheck(item2);
            if (status == HttpStatusCode.BadRequest)
            {
                Response.StatusCode = 400;
                return new ObjectResult(null);
            }
            if (item2 != null)
            {
                string[] cur = dBUserItemsDBFind.users[comm.Index].Real.Split(new char[] { ',' });
                List<string> list = new List<string>(cur);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == item2)
                    {
                        Response.StatusCode = 409;
                        return new ObjectResult(null);
                    }
                }
                list.Add(item2);
                string output = string.Join(",", list);
                dBUserItemsDBFind.users[comm.Index].Real = output;
                await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
                if (temp == null || temp2 == null)
                {
                    return new ObjectResult(null);
                }

            }
            if (temp != null && temp2 != null)
            {
                for (int i = 0; i < dBUserItemsDBFind.users[comm.Index].Currency.Count; i++)
                {
                    if (dBUserItemsDBFind.users[comm.Index].Currency[i].Crypto == temp)
                    {
                        if (temp == temp2)
                        {
                            Response.StatusCode = 409;
                            return new ObjectResult(null);
                        }
                        for (int j = 0; j < dBUserItemsDBFind.users[comm.Index].Currency.Count; j++)
                        {
                            if (temp2 == dBUserItemsDBFind.users[comm.Index].Currency[j].Crypto)
                            {
                                Response.StatusCode = 409;
                                return new ObjectResult(null);
                            }
                        }
                        if (temp != temp2)
                        {
                            dBUserItemsDBFind.users[comm.Index].Currency[i].Crypto = temp2;
                            await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
                            return new ObjectResult(null);
                        }
                    }
                }
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            else
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
        }

        //[HttpDelete]
        //[Route("useredit/{ids}")]
        //public async Task<ObjectResult> UserEditDelete([FromBody] UserCurrencyToDelete item, string ids)
        //{
        //    Commands comm = new Commands();
        //    UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
        //    if (dBUserItemsDBFind == null)
        //    {
        //        Response.StatusCode = 404;
        //    }
        //    bool cont = false;
        //    int ind = 0;
        //    for (int i = 0; i < dBUserItemsDBFind.users.Count; i++)
        //    {
        //        if (dBUserItemsDBFind.users[i].Id == ids)
        //        {
        //            ind = i;
        //            cont = true;
        //            break;
        //        }
        //    }
        //    if (cont == false)
        //    {
        //        Response.StatusCode = 404;
        //        return new ObjectResult(null);
        //    }
        //    //if (item.real != null)
        //    //{
        //    //    item.real = comm.InputCheckReal(item.real);
        //    //}
        //    //if (item.crypto != null)
        //    //{
        //    //    item.crypto = comm.InputCheckCrypto(item.crypto);
        //    //}
        //    client = new CoinpaprikaAPI.Client();
        //    var coins = await client.GetCoinsAsync();
        //    string s = JsonConvert.SerializeObject(coins);
        //    Value st = JsonConvert.DeserializeObject<Value>(s);
        //    Dictionary<string, string> names = new Dictionary<string, string>();
        //    int lenght = st.value.Count;
        //    for (int i = 0; i < lenght; i++)
        //    {
        //        names.Add(st.value[i].Id, st.value[i].Name);
        //    }
        //    string temp2 = names.FirstOrDefault(x => x.Value == item.crypto).Key;
        //    item.crypto = temp2;
        //    string item1 = item.crypto;
        //    string item2 = item.real;
        //    string[] cur = dBUserItemsDBFind.users[ind].Real.Split(new char[] { ',' });
        //    List<string> list = new List<string>(cur);
        //    if (item2 != null)
        //    {
        //        for (int i = 0; i < list.Count; i++)
        //        {
        //            if (list.Count == 1)
        //            {
        //                Response.StatusCode = 405;
        //                return new ObjectResult(null);
        //            }
        //            for (int j = 0; j < list.Count; j++)
        //            {
        //                if (item2 == list[j])
        //                {
        //                    list.Remove(item2);
        //                    string output = string.Join(",", list);
        //                    dBUserItemsDBFind.users[ind].Real = output;
        //                    await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
        //                    return new ObjectResult(null);
        //                }
        //            }  
        //        }
        //        Response.StatusCode = 404;
        //        return new ObjectResult(null);
        //    }
        //    if (item1 != null)
        //    {
        //        for (int i = 0; i < dBUserItemsDBFind.users[ind].Currency.Count; i++)
        //        {
        //            if (dBUserItemsDBFind.users[ind].Currency.Count == 1)
        //            {
        //                Response.StatusCode = 405;
        //                return new ObjectResult(null);
        //            }
        //            if (dBUserItemsDBFind.users[ind].Currency[i].Crypto == item1)
        //            {
        //                dBUserItemsDBFind.users[ind].Currency.RemoveAt(i);
        //            }
        //        }
        //    }
        //    await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
        //    return new ObjectResult(null);
        //}
        [HttpDelete]
        [Route("useredit/{ids}/deletecrypto/{crypto}")]
        public async Task<ObjectResult> UserEditDeleteCrypto(string ids, string crypto)
        {

            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, ids);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind == null)
            {
                Response.StatusCode = 404;
            }
            //if (dBUserItemsDBFind == null)
            //{
            //    Response.StatusCode = 404;
            //}
            //bool cont = false;
            //int ind = 0;
            //for (int i = 0; i < dBUserItemsDBFind.users.Count; i++)
            //{
            //    if (dBUserItemsDBFind.users[i].Id == ids)
            //    {
            //        ind = i;
            //        cont = true;
            //        break;
            //    }
            //}
            //if (cont == false)
            //{
            //    Response.StatusCode = 404;
            //    return new ObjectResult(null);
            //}
            client = new CoinpaprikaAPI.Client();
            var coins = await client.GetCoinsAsync();
            string s = JsonConvert.SerializeObject(coins);
            Value st = JsonConvert.DeserializeObject<Value>(s);
            Dictionary<string, string> names = new Dictionary<string, string>();
            int lenght = st.value.Count;
            for (int i = 0; i < lenght; i++)
            {
                st.value[i].Name = st.value[i].Name.ToLower();
                names.Add(st.value[i].Id, st.value[i].Name);
            }
            string low = crypto.ToLower();
            string cid = names.FirstOrDefault(x => x.Value == low).Key;

            if (cid != null)
            {
                bool ex = false;
                for (int i = 0; i < dBUserItemsDBFind.users[comm.Index].Currency.Count; i++)
                {
                    if (dBUserItemsDBFind.users[comm.Index].Currency[i].Crypto == cid)
                    {
                        if (dBUserItemsDBFind.users[comm.Index].Currency.Count == 1)
                        {
                            Response.StatusCode = 405;
                            return new ObjectResult(null);
                        }
                        ex = true;
                        dBUserItemsDBFind.users[comm.Index].Currency.RemoveAt(i);
                        await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
                        return new ObjectResult(null);
                    }
                }
                if (ex == false)
                {
                    Response.StatusCode = 404;
                    return new ObjectResult(null);
                }

            }
            if (cid == null)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            return new ObjectResult(null);
        }
        [HttpDelete]
        [Route("useredit/{ids}/deletereal/{real}")]
        public async Task<ObjectResult> UserEditDeleteReal(string ids, string real)
        {
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, ids);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind == null)
            {
                Response.StatusCode = 404;
            }
            string realm = real.ToLower();
            string[] cur = dBUserItemsDBFind.users[comm.Index].Real.Split(new char[] { ',' });
            List<string> list = new List<string>(cur);
            if (realm != null)
            {
                bool ex = false;
                for (int i = 0; i < list.Count; i++)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (realm == list[j])
                        {
                            ex = true;
                            if (list.Count == 1)
                            {
                                Response.StatusCode = 405;
                                return new ObjectResult(null);
                            }
                            list.Remove(realm);
                            string output = string.Join(",", list);
                            dBUserItemsDBFind.users[comm.Index].Real = output;
                            await System.IO.File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dBUserItemsDBFind, Formatting.Indented));
                            return new ObjectResult(null);
                        }
                    }
                    if (ex == false)
                    {
                        Response.StatusCode = 404;
                        return new ObjectResult(null);
                    }
                }
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            if (realm == null)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            return new ObjectResult(null);

        }
        [HttpGet]
        [Route("user/{id}")]
        public async Task<ObjectResult> FullInformation(string id)
        {
            WebReq req = new WebReq();
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, id);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind == null)
            {
                Response.StatusCode = 404;
            }
            string real = dBUserItemsDBFind.users[comm.Index].Real;
            //List<string> cryptos = new List<string>();
            UserShow userShow = await req.GetTickers(dBUserItemsDBFind, comm.Index, real);
            return new ObjectResult(userShow.Ticker);
        }
        [HttpGet]
        [Route("user/{id}/{cryptoname}")]
        public async Task<ObjectResult> Information(string id, string cryptoname)
        {
            WebReq req = new WebReq();
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, id);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            if (dBUserItemsDBFind == null)
            {
                Response.StatusCode = 404;
            }
            client = new CoinpaprikaAPI.Client();
            var coins = await client.GetCoinsAsync();
            string s = JsonConvert.SerializeObject(coins);
            Value st = JsonConvert.DeserializeObject<Value>(s);
            Dictionary<string, string> names = new Dictionary<string, string>();
            int lenght = st.value.Count;
            for (int i = 0; i < lenght; i++)
            {
                st.value[i].Name = st.value[i].Name.ToLower();
                names.Add(st.value[i].Id, st.value[i].Name);
            }
            string temp2 = names.FirstOrDefault(x => x.Value == cryptoname).Key;
            if (temp2 == null)
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
            string real = dBUserItemsDBFind.users[comm.Index].Real;
            List<string> cryptos = new List<string>();
            UserShow userShow = await req.GetTickers(dBUserItemsDBFind, comm.Index, real);
            int cryind = 0;
            bool exist = false;
            for (int i = 0; i < userShow.Ticker.Count; i++)
            {
                string t = userShow.Ticker[i].Name.ToLower();
                if (t == cryptoname)
                {
                    cryind = i;
                    exist = true;
                }
            }
            if (exist == true)
            {
                return new ObjectResult(userShow.Ticker[cryind]);
            }
            else
            {
                Response.StatusCode = 404;
                return new ObjectResult(null);
            }
        }
        [HttpGet]
        [Route("user/realget/{id}")]
        public async Task<string> GetReal(string id)
        {
            UserList dBUserItemsDBFind = JsonConvert.DeserializeObject<UserList>(System.IO.File.ReadAllText(path));
            Commands comm = new Commands();
            comm.DBCheck(comm, dBUserItemsDBFind, id);
            if (comm.Exist == false)
            {
                Response.StatusCode = 404;
                return null;
            }
            if (dBUserItemsDBFind == null)
            {
                Response.StatusCode = 404;
            }
            string real = dBUserItemsDBFind.users[comm.Index].Real;
            return real;
        }
    }
}