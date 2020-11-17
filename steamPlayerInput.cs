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

namespace steamPlayerInvestigator
{
    public partial class steamPlayerInput : Form
    {
        public steamPlayerInput()
        {
            InitializeComponent();
        }

        private async void inputButton_ClickAsync(object sender, EventArgs e)
        {
            await getSteamInputAsync();
        }


        private async Task getSteamPlayerSummary(string pInputSteamId)
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

            url = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pInputSteamId;
            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respBans = await response.Content.ReadAsStringAsync();
            PlayerBansRoot steamUserBans = JsonConvert.DeserializeObject<PlayerBansRoot>(respBans);

            url = "/ISteamUser/GetFriendList/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamid=" + pInputSteamId;
            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respFriends = await response.Content.ReadAsStringAsync();
            FriendsRoot steamUserFriends = JsonConvert.DeserializeObject<FriendsRoot>(respFriends);

            url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

            for (int i = 0; i < steamUserFriends.friendslist.friends.Count; i++)
            {
                url += steamUserFriends.friendslist.friends[i].steamid + "?";
            }

            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respFriendsSummary = await response.Content.ReadAsStringAsync();
            SummaryRoot steamUserFriendsSummary = JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary);

            steamPlayerSummary steamPlayerSummaryForm = new steamPlayerSummary(steamUser.response.players[0], steamUserBans.players[0], steamUserFriends.friendslist, steamUserFriendsSummary.response);
            steamPlayerSummaryForm.Show();
            
        }

        private async Task getSteamInputAsync()
        {
            string inputSteamId = inputTextBox.Text;
            await getSteamPlayerSummary(inputSteamId);
        }
    }
}
