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


        private async Task getSteamPlayerSummary(string pInputSteamId, string button)
        {
            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://api.steampowered.com");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
            else if(steamUser.response.players[0].communityvisibilitystate != 3)
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

            if(button == "manual")
            {
                steamPlayerSummary steamPlayerSummaryForm = new steamPlayerSummary(steamUser.response.players[0], steamUserBans.players[0], steamUserFriends.friendslist, steamUserFriendsSummary, currentSummaryCount);
                steamPlayerSummaryForm.Show();
            }
            else if(button == "automatic")
            {
                steamAutomaticInvestigation steamAutomaticInvestigationForm = new steamAutomaticInvestigation(steamUser.response.players[0], steamUserBans.players[0], steamUserFriends.friendslist, steamUserFriendsSummary, currentSummaryCount);
                steamAutomaticInvestigationForm.Show();
            }
        }

        private async Task getSteamInputAsync(string button)
        {
            string inputSteamId = inputTextBox.Text;
            await getSteamPlayerSummary(inputSteamId, button);
        }

        private async void automaticConfirmButton_ClickAsync(object sender, EventArgs e)
        {
            string button = "automatic";
            await getSteamInputAsync(button);
        }
    }
}
