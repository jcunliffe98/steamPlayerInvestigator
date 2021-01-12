using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using steamPlayerInvestigator.Forms;

namespace steamPlayerInvestigator
{
    public partial class steamPlayerInput : Form
    {
        public steamPlayerInput()
        {
            InitializeComponent();
        }

        private async void manualConfirmButton_ClickAsync(object sender, EventArgs e)
        {
            string button = "manual";
            await getSteamInputAsync(button);
        }


        private async Task getManualSteamPlayerSummary(string pInputSteamId)
        {
            HttpClient client = getClient();

            UrlRoot foundSteamId = new UrlRoot();

            if (pInputSteamId.Contains("https://steamcommunity.com/id/"))
            {
                string[] steamUrlSplit = pInputSteamId.Split('/');
                string steamVanityUrl = "/ISteamUser/ResolveVanityURL/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&vanityurl=" + steamUrlSplit[4];
                HttpResponseMessage steamIdResponse = await client.GetAsync(steamVanityUrl);
                steamIdResponse.EnsureSuccessStatusCode();

                string urlResponse;
                urlResponse = await steamIdResponse.Content.ReadAsStringAsync();
                foundSteamId = JsonConvert.DeserializeObject<UrlRoot>(urlResponse);
                pInputSteamId = foundSteamId.response.steamid;

                if (foundSteamId.response.success != 1)
                {
                    MessageBox.Show("Steam URL not found");
                    inputTextBox.Clear();
                    return;
                }
            }
            else if (pInputSteamId.Contains("https://steamcommunity.com/profiles/"))
            {
                string[] steamUrlSplit = pInputSteamId.Split('/');
                pInputSteamId = steamUrlSplit[4];
            }

            string url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pInputSteamId;
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respSummary = await response.Content.ReadAsStringAsync();
            SummaryRoot steamUser = JsonConvert.DeserializeObject<SummaryRoot>(respSummary);

            if (!respSummary.Contains("steamid"))
            {
                MessageBox.Show("Steam ID not found");
                inputTextBox.Clear();
                return;
            }
            else if (steamUser.response.players[0].communityvisibilitystate != 3)
            {
                MessageBox.Show("This profile is private");
                inputTextBox.Clear();
                return;
            }

            url = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pInputSteamId;
            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respBans = await response.Content.ReadAsStringAsync();
            PlayerBansRoot steamUserBans = JsonConvert.DeserializeObject<PlayerBansRoot>(respBans);

            url = "/ISteamUser/GetFriendList/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamid=" + pInputSteamId;
            response = await client.GetAsync(url);

            FriendsRoot steamUserFriends = new FriendsRoot();
            List<SummaryRoot> steamUserFriendsSummary = new List<SummaryRoot>();

            if (response.StatusCode.ToString() == "OK")
            {
                string respFriends = await response.Content.ReadAsStringAsync();
                steamUserFriends = JsonConvert.DeserializeObject<FriendsRoot>(respFriends);

                int noOfLoops = (steamUserFriends.friendslist.friends.Count / 100) + 1;

                if (steamUserFriends.friendslist.friends.Count <= 100)
                {
                    url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";
                    for (int i = 0; i < steamUserFriends.friendslist.friends.Count; i++)
                    {
                        url += steamUserFriends.friendslist.friends[i].steamid + "?";
                    }
                    response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string respFriendsSummary = await response.Content.ReadAsStringAsync();
                    steamUserFriendsSummary.Add(JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary));
                }
                else
                {
                    int overallCount = 0;

                    for (int a = 0; a < noOfLoops; a++)
                    {
                        url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                        for (int i = a * 100; i < (a + 1) * 100; i++)
                        {
                            overallCount++;
                            url += steamUserFriends.friendslist.friends[i].steamid + "?";
                            if (overallCount == steamUserFriends.friendslist.friends.Count)
                            {
                                break;
                            }
                        }

                        response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string respFriendsSummary = await response.Content.ReadAsStringAsync();
                        steamUserFriendsSummary.Add(JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary));
                    }
                }
            }
            else
            {
                MessageBox.Show("Friends List can't be retrieved");
            }

            int currentSummaryCount = 0;

            steamPlayerSummary steamPlayerSummaryForm = new steamPlayerSummary(steamUser.response.players[0], steamUserBans.players[0], steamUserFriends.friendslist, steamUserFriendsSummary, currentSummaryCount);
            steamPlayerSummaryForm.Show();
        }

        private static HttpClient getClient()
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri("https://api.steampowered.com");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private async Task getSteamInputAsync(string button)
        {
            string inputSteamId = inputTextBox.Text;
            if(button == "manual")
            {
                await getManualSteamPlayerSummary(inputSteamId);
            }
            else if(button == "automatic")
            {
                await getAutomaticSteamPlayerSummary(inputSteamId);
            }
        }

        private async Task getAutomaticSteamPlayerSummary(string pInputSteamId)
        {
            HttpClient client = getClient();

            UrlRoot foundSteamId = new UrlRoot();

            if (pInputSteamId.Contains("https://steamcommunity.com/id/"))
            {
                string[] steamUrlSplit = pInputSteamId.Split('/');
                string steamVanityUrl = "/ISteamUser/ResolveVanityURL/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&vanityurl=" + steamUrlSplit[4];
                HttpResponseMessage steamIdResponse = await client.GetAsync(steamVanityUrl);
                steamIdResponse.EnsureSuccessStatusCode();

                string urlResponse;
                urlResponse = await steamIdResponse.Content.ReadAsStringAsync();
                foundSteamId = JsonConvert.DeserializeObject<UrlRoot>(urlResponse);
                pInputSteamId = foundSteamId.response.steamid;

                if (foundSteamId.response.success != 1)
                {
                    MessageBox.Show("Steam URL not found");
                    inputTextBox.Clear();
                    return;
                }
            }
            else if (pInputSteamId.Contains("https://steamcommunity.com/profiles/"))
            {
                string[] steamUrlSplit = pInputSteamId.Split('/');
                pInputSteamId = steamUrlSplit[4];
            }

            string url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pInputSteamId;
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respSummary = await response.Content.ReadAsStringAsync();
            SummaryRoot steamUser = JsonConvert.DeserializeObject<SummaryRoot>(respSummary);

            if (!respSummary.Contains("steamid"))
            {
                MessageBox.Show("Steam ID not found");
                inputTextBox.Clear();
                return;
            }
            else if (steamUser.response.players[0].communityvisibilitystate != 3)
            {
                MessageBox.Show("This profile is private");
                inputTextBox.Clear();
                return;
            }

            url = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pInputSteamId;
            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respBans = await response.Content.ReadAsStringAsync();
            PlayerBansRoot steamUserBans = JsonConvert.DeserializeObject<PlayerBansRoot>(respBans);

            url = "/ISteamUser/GetFriendList/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamid=" + pInputSteamId;
            response = await client.GetAsync(url);

            FriendsRoot steamUserFriends = new FriendsRoot();
            List<SummaryRoot> steamUserFriendsSummary = new List<SummaryRoot>();

            if (response.StatusCode.ToString() == "OK")
            {
                string respFriends = await response.Content.ReadAsStringAsync();
                steamUserFriends = JsonConvert.DeserializeObject<FriendsRoot>(respFriends);

                int noOfLoops = (steamUserFriends.friendslist.friends.Count / 100) + 1;

                if (steamUserFriends.friendslist.friends.Count <= 100)
                {
                    url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                    for (int i = 0; i < steamUserFriends.friendslist.friends.Count; i++)
                    {
                        url += steamUserFriends.friendslist.friends[i].steamid + "?";
                        string urlFriend = "/ISteamUser/GetFriendList/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamid=";
                        urlFriend += steamUserFriends.friendslist.friends[i].steamid;
                        response = await client.GetAsync(urlFriend);
                        string respFriendsOfFriends = await response.Content.ReadAsStringAsync();
                        FriendsRoot friends;
                        friends = JsonConvert.DeserializeObject<FriendsRoot>(respFriendsOfFriends);
                        steamUserFriends.friendslist.friends[i].friendsOfFriends = friends;
                    }
                    response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string respFriendsSummary = await response.Content.ReadAsStringAsync();
                    steamUserFriendsSummary.Add(JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary));
                }
                else
                {
                    int overallCount = 0;
                    int count = 0;

                    for (int a = 0; a < noOfLoops; a++)
                    {
                        url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";
                        string urlBan = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                        for (int i = a * 100; i < (a + 1) * 100; i++)
                        {
                            overallCount++;
                            url += steamUserFriends.friendslist.friends[i].steamid + "?";
                            urlBan += steamUserFriends.friendslist.friends[i].steamid + "?";
                            string urlFriend = "/ISteamUser/GetFriendList/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamid=";
                            urlFriend += steamUserFriends.friendslist.friends[i].steamid;
                            response = await client.GetAsync(urlFriend);
                            if (response.StatusCode.ToString() == "Unauthorized")
                            {
                                continue;
                            }
                            string respFriendsOfFriends = await response.Content.ReadAsStringAsync();
                            FriendsRoot friends;
                            friends = JsonConvert.DeserializeObject<FriendsRoot>(respFriendsOfFriends);
                            steamUserFriends.friendslist.friends[i].friendsOfFriends = friends;
                            if (overallCount == steamUserFriends.friendslist.friends.Count)
                            {
                                break;
                            }
                        }

                        response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string respFriendsSummary = await response.Content.ReadAsStringAsync();
                        steamUserFriendsSummary.Add(JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary));

                        response = await client.GetAsync(urlBan);
                        response.EnsureSuccessStatusCode();
                        string respFriendsBan = await response.Content.ReadAsStringAsync();
                        SummaryResponse responseBans = (JsonConvert.DeserializeObject<SummaryResponse>(respFriendsBan));
                        for (int i = 0; i < steamUserFriendsSummary[count].response.players.Count; i++)
                        {
                            for (int b = 0; b < steamUserFriendsSummary[count].response.players.Count; b++)
                            {
                                if (steamUserFriendsSummary[count].response.players[i].steamid == responseBans.players[b].steamid)
                                {
                                    steamUserFriendsSummary[count].response.players[i].CommunityBanned = responseBans.players[b].CommunityBanned;
                                    steamUserFriendsSummary[count].response.players[i].VACBanned = responseBans.players[b].VACBanned;
                                    steamUserFriendsSummary[count].response.players[i].NumberOfVACBans = responseBans.players[b].NumberOfVACBans;
                                    steamUserFriendsSummary[count].response.players[i].DaysSinceLastBan = responseBans.players[b].DaysSinceLastBan;
                                    steamUserFriendsSummary[count].response.players[i].NumberOfGameBans = responseBans.players[b].NumberOfGameBans;
                                    steamUserFriendsSummary[count].response.players[i].EconomyBan = responseBans.players[b].EconomyBan;
                                    break;
                                }
                            }
                            for (int b = 0; b < steamUserFriends.friendslist.friends.Count; b++)
                            {
                                if (steamUserFriendsSummary[count].response.players[i].steamid == steamUserFriends.friendslist.friends[b].steamid)
                                {
                                    steamUserFriendsSummary[count].response.players[i].friend_since = steamUserFriends.friendslist.friends[b].friend_since;
                                    steamUserFriendsSummary[count].response.players[i].relationship = steamUserFriends.friendslist.friends[b].relationship;
                                    steamUserFriendsSummary[count].response.players[i].friendsOfFriends = steamUserFriends.friendslist.friends[b].friendsOfFriends;
                                }
                            }
                        }
                            count++;
                    }
                }
            }
            else
            {
                MessageBox.Show("Friends List can't be retrieved");
            }

            SummaryRoot summaryFriends = steamUserFriendsSummary[0];
            for(int a = 1; a < steamUserFriendsSummary.Count; a++)
            {
                for(int b = 0; b < steamUserFriendsSummary[a].response.players.Count; b++)
                {
                    summaryFriends.response.players.Add(steamUserFriendsSummary[a].response.players[b]);
                }
            }

            steamAutomaticInvestigation steamAutomaticInvestigationForm = new steamAutomaticInvestigation(steamUser.response.players[0], steamUserBans.players[0], steamUserFriends.friendslist, steamUserFriendsSummary);
            steamAutomaticInvestigationForm.Show();
        }

        private async void automaticConfirmButton_ClickAsync(object sender, EventArgs e)
        {
            string button = "automatic";
            await getSteamInputAsync(button);
        }
    }
}
