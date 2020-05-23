//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Microsoft.IdentityModel.Clients.ActiveDirectory;
//using System.Globalization;

//using System.Net;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using System.Threading;
//using uk.co.silversands.servicemanager.library.viewmodels;
//using System.Net.Http.Headers;
//using Newtonsoft.Json;
//using ZomBotDice.LUIS;

//namespace ScribeBotV4
//{ 
//    public class ScribeFunctions
//    {

//        private readonly HttpClient _httpClient;

//        public ScribeFunctions(HttpClient httpClient)
//        {
//             authority = String.Format(CultureInfo.InvariantCulture, this.aadInstance, tenant);
//            _httpClient = httpClient; 
//        }

//        private string aadInstance = "https://login.microsoftonline.com/{0}";
//        private string tenant = "7a52d502-7a9b-41a1-9e67-457f7968e2be";

//        private string authority;

//        private string clientId = "c8f16450-be38-49e2-ba20-fbb57a2dbaea"; //Externalise to KeyVault
//        private string appKey = "YFYu3fbppT4PnG5w5UFECYD0Ha/wOAl0Pujp+bJ6XIw=";
//        private string ScribeAPIResourceID = "703e6833-4210-4469-8063-296eb29213f4";

//        private AuthenticationContext authContext = null;
//        private ClientCredential clientCredential = null;

//        public async Task<AuthenticationResult> DoAuth()
//        {
//            AuthenticationResult result = null;
//            int retryCount = 0;
//            bool retry = false;

//            do
//            {
//                retry = false;
//                try
//                {

//                    authContext = new AuthenticationContext(authority);
//                    clientCredential = new ClientCredential(clientId, appKey);


//                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
//                    result = await authContext.AcquireTokenAsync(ScribeAPIResourceID, clientCredential);
//                }
//                catch (AdalException ex)
//                {
//                    if (ex.ErrorCode == "temporarily_unavailable")
//                    {
//                        retry = true;
//                        retryCount++;
//                        Thread.Sleep(3000);
//                    }

//                    Console.WriteLine(
//                        String.Format("An error occurred while acquiring a token\nTime: {0}\nError: {1}\nRetry: {2}\n",
//                        DateTime.Now.ToString(),
//                        ex.ToString(),
//                        retry.ToString()));
//                }

//            } while ((retry == true) && (retryCount < 3));

//            if (result == null)
//            {
//                Console.WriteLine("Canceling attempt to contact To Do list service.\n");
//                return null;
//            }
//            else
//            {
//                return result;
//            }
//        }

//        public async Task<VMIncident> GetIncident(string id)
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/GetIncident?code=c/JwAfGBrZTQBaebmjC5rTBcYlmWF8eqVw4ZblKhg8qGoZYfnB5tcw==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            string jsonbase = @"{""irnumber"":""" + id + @"""}";
//            //jsonbase = String.Format(jsonbase, id);
//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if(response.IsSuccessStatusCode)
//            {
//                string responsestring = await response.Content.ReadAsStringAsync();
//                //string subresponsestring = responsestring.Substring(1, responsestring.Length - 2);
//                VMIncident vmincident = new VMIncident();
//                try
//                {
//                    JsonSerializerSettings settings = new JsonSerializerSettings();

//                    //vmincident = JsonConvert.DeserializeObject<VMIncident>(subresponsestring, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});
//                    dynamic dincident;

//                    dincident = JsonConvert.DeserializeObject(responsestring);
//                    vmincident = JsonConvert.DeserializeObject<VMIncident>(dincident);

//                    int i = 0;

//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);

//                }
//                return vmincident;

//            }
//            else
//            {
//                return null;
//            }

//        }

//        public async Task<VMIncident[]> GetUserActiveIncidents(string user)
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/GetUserActiveIncidents?code=MSxEBZ4tVPhO00JZULdE7RaLjU/1yixeQZghR1f7Q219joBx3ELoCA==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            string jsonbase = @"{""displayName"":""" + user + @"""}";
            
//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {
//                string responsestring = await response.Content.ReadAsStringAsync();
//                //string subresponsestring = responsestring.Substring(1, responsestring.Length - 2);
//                VMIncident[] vmincident = null;
//                try
//                {
//                    JsonSerializerSettings settings = new JsonSerializerSettings();

//                    //vmincident = JsonConvert.DeserializeObject<VMIncident>(subresponsestring, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});
//                    dynamic dincident;

//                    dincident = JsonConvert.DeserializeObject(responsestring);
//                    vmincident = JsonConvert.DeserializeObject<VMIncident[]>(dincident);

//                    int i = 0;

//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);

//                }
//                return vmincident;

//            }
//            else
//            {
//                return null;
//            }

//        }

//        public async Task<Dictionary<String, List<VMIncident>>> GetNewAssignments()
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/GetNewAssignments?code=cpJxt4Pn2V1l1Uht2KOBesbjNvSwtD744MRYW/nk5xN/790aFEE/5A==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            string jsonbase = @"{}";

//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {
//                string responsestring = await response.Content.ReadAsStringAsync();
//                //string subresponsestring = responsestring.Substring(1, responsestring.Length - 2);
//                Dictionary<String, List<VMIncident>> newAssignments = null;
//                try
//                {
//                    JsonSerializerSettings settings = new JsonSerializerSettings();

//                    //vmincident = JsonConvert.DeserializeObject<VMIncident>(subresponsestring, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore});
//                    dynamic dincident;

//                    dincident = JsonConvert.DeserializeObject(responsestring);
//                    newAssignments = JsonConvert.DeserializeObject<Dictionary<String, List<VMIncident>>>(dincident);

//                    int i = 0;

//                }
//                catch (Exception e)
//                {
//                    Console.WriteLine(e);

//                }
//                return newAssignments;

//            }
//            else
//            {
//                return null;
//            }

//        }

//        public async Task<bool> AddNote(string id, string addedBy, string body)
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/AddNote?code=0Z07HfLEpoVkPuXPvearXyyjxB7kxb9QhG0UwDcgauearm1DC6uloQ==&clientId=default&client=ScribeBot";

//            AuthenticationResult auth = await DoAuth();

//            VMResolution resolution = new VMResolution(body, addedBy, "", id);
//            string jsonBase = JsonConvert.SerializeObject(resolution);
//            //jsonbase = String.Format(jsonbase, id);
//            HttpContent content = new StringContent(jsonBase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }

//        }

//        public async Task<bool> Resolve(string id, string addedBy, string body)
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/ResolveIncident?code=tsfi4D/PqIOhTbKpx/oma3sT4rUQbGecZP87QVK2w1U5tZnXsQ6y7Q==&clientId=default&client=ScribeBot";

//            AuthenticationResult auth = await DoAuth();

//            VMResolution resolution = new VMResolution(body, addedBy, "", id);
//            string jsonBase = JsonConvert.SerializeObject(resolution);
//            //jsonbase = String.Format(jsonbase, id);
//            HttpContent content = new StringContent(jsonBase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }

//        }

//        public async Task<bool> SetStatus(string id, ScribeIntents.Intent intent)
//        {
//            // Generic set status function

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/SetIncidentStatus?code=AQ8FQgZv0TzVex/GnS/nU8fdXOjtyDGQVVZMIkt8DHPipRa9agtHUQ==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            VMTicketStatus newStatus = null;

//            switch (intent)
//            {
//                case ScribeIntents.Intent.Await_Customer:
//                    newStatus = new VMTicketStatus("AwaitingCustomer", id);
//                    break;
//                case ScribeIntents.Intent.place_on_hold:
//                    newStatus = new VMTicketStatus("OnHold", id);
//                    break;
//                case ScribeIntents.Intent.Set_Active:
//                    newStatus = new VMTicketStatus("Active", id);
//                    break;
//                default:
//                    return false; // intent not recognised as new incident status
//            }
            
//            string jsonbase = JsonConvert.SerializeObject(newStatus);

//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }

//        }

//        public async Task<bool> SetOnHoldDate(string id, string newOnHoldDate)
//        {
//            // Generic set status function

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/SetOnHoldDate?code=Kys0GdgSv8nP368XWzKwNfekhHOAK7f6fTudPRPYTMxRfnlA3fvEhQ==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            VMTicketStatus newStatus = null;

//            newStatus = new VMTicketStatus("OnHold", id, newOnHoldDate);

//            string jsonbase = JsonConvert.SerializeObject(newStatus);

//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }

//        }

//        public async Task<bool> SFR(string id)
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/SetFirstResponse?code=1UImCn6LkLi9Rbhv9WJFoSK/FhQfxxy788Z/YMKa5BYi9Zfcr79Jxg==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            string jsonbase = @"{""irnumber"":""" + id + @"""}";
            
//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }

//        }

//        public async Task<bool> AC(string id)
//        {

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/SetIncidentStatus?code=AQ8FQgZv0TzVex/GnS/nU8fdXOjtyDGQVVZMIkt8DHPipRa9agtHUQ==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            VMTicketStatus newStatus = new VMTicketStatus("AwaitingCustomer", id);
//            string jsonbase = JsonConvert.SerializeObject(newStatus);

//            HttpContent content = new StringContent(jsonbase);
//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }

//        }

//        public async Task<string> ResolveAssignee(string namepartial)
//        {
//            // Resolve username using Azure Search index
//            // https://scribesearch.search.windows.net/indexes/scribeusers-index/docs?api-version=2019-05-06&search=james*&queryType=full&%24orderby=search.score()%20desc&%24top=1

//            string serviceBase = "scribesearch";
//            string index = "scribeusers-index";
//            string apiVersion = "2019-05-06";
//            string query = String.Format("{0}{1}", namepartial.Replace(' ', '*'), "*");
//            string apiKey = "CD78CE41C1B6784090E31DF51A5D2192";
//            string uri = String.Format("https://{0}.search.windows.net/indexes/{1}/docs?api-version={2}&search={3}&queryType=full&%24orderby=search.score()%20desc&%24top=1&api-key={4}", serviceBase, index, apiVersion, query, apiKey);


//            try
//            {
//                HttpResponseMessage response = await _httpClient.GetAsync(uri);
//                string json = response.Content.ReadAsStringAsync().Result;


//                dynamic result = JsonConvert.DeserializeObject(json);
//                dynamic too = result["value"];
//                string three = too[0]["UserPrincipalName"];
//                return three;
//            }
//            catch (Exception e)
//            {
//                return "";
//            }

//        }

//        public async Task<bool> Reassign(string id, string newAssignee)
//        {
//            // Generic set status function

//            string endpoint = "https://smscribefunctions.azurewebsites.net/api/Reassign?code=aBrurp/lVrNHqHDH4vJxaRKImktfaj4bceEMe5ii9baIRv8bS5f5uw==&clientId=default";

//            AuthenticationResult auth = await DoAuth();

//            HttpContent content = new StringContent("{ \"irnumber\" : \"" + id+ "\" , \"newassignee\" : \"" + newAssignee + "\" }");

//            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

//            HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content);

//            if (response.IsSuccessStatusCode)
//            {

//                return true;

//            }
//            else
//            {
//                return false;
//            }
//        }

//    }
//}