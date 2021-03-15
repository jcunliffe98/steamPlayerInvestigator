﻿using System;
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
using System.IO;

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
            if (button == "manual")
            {
                await getManualSteamPlayerSummary(inputSteamId);
            }
            else if (button == "automatic")
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
                    string urlBan = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                    for (int i = 0; i < steamUserFriends.friendslist.friends.Count; i++)
                    {
                        url += steamUserFriends.friendslist.friends[i].steamid + "?";
                        urlBan += steamUserFriends.friendslist.friends[i].steamid + "?";

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

                    response = await client.GetAsync(urlBan);
                    response.EnsureSuccessStatusCode();
                    string respFriendsBan = await response.Content.ReadAsStringAsync();
                    SummaryResponse responseBans = (JsonConvert.DeserializeObject<SummaryResponse>(respFriendsBan));

                    for (int i = 0; i < steamUserFriendsSummary[0].response.players.Count; i++)
                    {
                        for (int b = 0; b < steamUserFriendsSummary[0].response.players.Count; b++)
                        {
                            if (steamUserFriendsSummary[0].response.players[i].steamid == responseBans.players[b].steamid)
                            {
                                steamUserFriendsSummary[0].response.players[i].CommunityBanned = responseBans.players[b].CommunityBanned;
                                steamUserFriendsSummary[0].response.players[i].VACBanned = responseBans.players[b].VACBanned;
                                steamUserFriendsSummary[0].response.players[i].NumberOfVACBans = responseBans.players[b].NumberOfVACBans;
                                steamUserFriendsSummary[0].response.players[i].DaysSinceLastBan = responseBans.players[b].DaysSinceLastBan;
                                steamUserFriendsSummary[0].response.players[i].NumberOfGameBans = responseBans.players[b].NumberOfGameBans;
                                steamUserFriendsSummary[0].response.players[i].EconomyBan = responseBans.players[b].EconomyBan;
                                break;
                            }
                        }
                        for (int b = 0; b < steamUserFriends.friendslist.friends.Count; b++)
                        {
                            if (steamUserFriendsSummary[0].response.players[i].steamid == steamUserFriends.friendslist.friends[b].steamid)
                            {
                                steamUserFriendsSummary[0].response.players[i].friend_since = steamUserFriends.friendslist.friends[b].friend_since;
                                steamUserFriendsSummary[0].response.players[i].relationship = steamUserFriends.friendslist.friends[b].relationship;
                                steamUserFriendsSummary[0].response.players[i].friendsOfFriends = steamUserFriends.friendslist.friends[b].friendsOfFriends;
                            }
                        }
                    }
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
                return;
            }

            SummaryRoot summaryFriends = steamUserFriendsSummary[0];
            for (int a = 1; a < steamUserFriendsSummary.Count; a++)
            {
                for (int b = 0; b < steamUserFriendsSummary[a].response.players.Count; b++)
                {
                    summaryFriends.response.players.Add(steamUserFriendsSummary[a].response.players[b]);
                }
            }

            for (int a = 0; a < summaryFriends.response.players.Count; a++)
            {
                url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";
                string urlBan = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";
                int loopCount = 0;
                SummaryRoot tempSummary = new SummaryRoot();
                PlayerBansRoot tempBans = new PlayerBansRoot();

                if (summaryFriends.response.players[a].friendsOfFriends == null || summaryFriends.response.players[a].friendsOfFriends.friendslist == null)
                {
                    continue;
                }
                for (int b = 0; b < summaryFriends.response.players[a].friendsOfFriends.friendslist.friends.Count; b++)
                {
                    url += summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[b].steamid + "?";
                    urlBan += summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[b].steamid + "?";
                    loopCount++;

                    if (loopCount == 100)
                    {
                        response = await client.GetAsync(url);
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        string respFriendsSummary = await response.Content.ReadAsStringAsync();
                        tempSummary = JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary);

                        response = await client.GetAsync(urlBan);
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        response.EnsureSuccessStatusCode();
                        string respFriendsBan = await response.Content.ReadAsStringAsync();
                        tempBans = (JsonConvert.DeserializeObject<PlayerBansRoot>(respFriendsBan));

                        urlBan = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";
                        url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                        for (int z = 0; z < tempSummary.response.players.Count; z++)
                        {
                            for (int y = 0; y < summaryFriends.response.players[a].friendsOfFriends.friendslist.friends.Count; y++)
                            {
                                if (tempSummary.response.players[z].steamid == summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].steamid)
                                {
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].communityvisibilitystate = tempSummary.response.players[z].communityvisibilitystate;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].profilestate = tempSummary.response.players[z].profilestate;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].personaname = tempSummary.response.players[z].personaname;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].profileurl = tempSummary.response.players[z].profileurl;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatar = tempSummary.response.players[z].avatar;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatarmedium = tempSummary.response.players[z].avatarmedium;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatarfull = tempSummary.response.players[z].avatarfull;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatarhash = tempSummary.response.players[z].avatarhash;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].lastlogoff = tempSummary.response.players[z].lastlogoff;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].personastate = tempSummary.response.players[z].personastate;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].primaryclanid = tempSummary.response.players[z].primaryclanid;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].timecreated = tempSummary.response.players[z].timecreated;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].personastateflags = tempSummary.response.players[z].personastateflags;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].loccountrycode = tempSummary.response.players[z].loccountrycode;
                                }
                            }
                        }
                        for (int z = 0; z < tempBans.players.Count; z++)
                        {
                            for (int y = 0; y < summaryFriends.response.players[a].friendsOfFriends.friendslist.friends.Count; y++)
                            {
                                if (tempBans.players[z].SteamId == summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].steamid)
                                {
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].CommunityBanned = tempBans.players[z].CommunityBanned;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].VACBanned = tempBans.players[z].VACBanned;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].NumberOfVACBans = tempBans.players[z].NumberOfVACBans;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].DaysSinceLastBan = tempBans.players[z].DaysSinceLastBan;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].NumberOfGameBans = tempBans.players[z].NumberOfGameBans;
                                    summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].EconomyBan = tempBans.players[z].EconomyBan;
                                }
                            }
                        }
                        loopCount = 0;
                    }
                }
                if (loopCount != 0)
                {
                    response = await client.GetAsync(url);
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    string respFriendsSummary = await response.Content.ReadAsStringAsync();
                    tempSummary = JsonConvert.DeserializeObject<SummaryRoot>(respFriendsSummary);
                    url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                    response = await client.GetAsync(urlBan);
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    string respFriendsBan = await response.Content.ReadAsStringAsync();
                    tempBans = (JsonConvert.DeserializeObject<PlayerBansRoot>(respFriendsBan));
                    urlBan = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=";

                    for (int z = 0; z < tempSummary.response.players.Count; z++)
                    {
                        for (int y = 0; y < summaryFriends.response.players[a].friendsOfFriends.friendslist.friends.Count; y++)
                        {
                            if (tempSummary.response.players[z].steamid == summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].steamid)
                            {
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].communityvisibilitystate = tempSummary.response.players[z].communityvisibilitystate;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].profilestate = tempSummary.response.players[z].profilestate;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].personaname = tempSummary.response.players[z].personaname;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].profileurl = tempSummary.response.players[z].profileurl;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatar = tempSummary.response.players[z].avatar;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatarmedium = tempSummary.response.players[z].avatarmedium;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatarfull = tempSummary.response.players[z].avatarfull;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].avatarhash = tempSummary.response.players[z].avatarhash;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].lastlogoff = tempSummary.response.players[z].lastlogoff;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].personastate = tempSummary.response.players[z].personastate;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].primaryclanid = tempSummary.response.players[z].primaryclanid;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].timecreated = tempSummary.response.players[z].timecreated;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].personastateflags = tempSummary.response.players[z].personastateflags;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].loccountrycode = tempSummary.response.players[z].loccountrycode;
                            }
                        }
                    }
                    for (int z = 0; z < tempBans.players.Count; z++)
                    {
                        for (int y = 0; y < summaryFriends.response.players[a].friendsOfFriends.friendslist.friends.Count; y++)
                        {
                            if (tempBans.players[z].SteamId == summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].steamid)
                            {
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].CommunityBanned = tempBans.players[z].CommunityBanned;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].VACBanned = tempBans.players[z].VACBanned;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].NumberOfVACBans = tempBans.players[z].NumberOfVACBans;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].DaysSinceLastBan = tempBans.players[z].DaysSinceLastBan;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].NumberOfGameBans = tempBans.players[z].NumberOfGameBans;
                                summaryFriends.response.players[a].friendsOfFriends.friendslist.friends[y].EconomyBan = tempBans.players[z].EconomyBan;
                            }
                        }
                    }
                }
            }

            steamAutomaticInvestigation steamAutomaticInvestigationForm = new steamAutomaticInvestigation();
            await steamAutomaticInvestigationForm.Main(steamUser.response.players[0], steamUserBans.players[0], summaryFriends, client);
        }

        private async void automaticConfirmButton_ClickAsync(object sender, EventArgs e)
        {
            string button = "automatic";
            await getSteamInputAsync(button);
        }

        private async void button1_ClickAsync(object sender, EventArgs e)
        {
            HttpClient client = getClient();
            List<Player> steamPlayerInstances = new List<Player>();
            List<PlayerBans> steamPlayerBansInstaces = new List<PlayerBans>();
            List<SummaryRoot> steamSummaryInstances = new List<SummaryRoot>();
            List<List<Player>> sortedBannedPlayerInstances = new List<List<Player>>();

            string steamId = inputTextBox.Text;

            if (steamId.Contains("https://steamcommunity.com/id/"))
            {
                string[] steamUrlSplit = steamId.Split('/');
                string steamVanityUrl = "/ISteamUser/ResolveVanityURL/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&vanityurl=" + steamUrlSplit[4];
                HttpResponseMessage steamIdResponse = await client.GetAsync(steamVanityUrl);
                steamIdResponse.EnsureSuccessStatusCode();

                string urlResponse;
                urlResponse = await steamIdResponse.Content.ReadAsStringAsync();
                UrlRoot foundSteamId = JsonConvert.DeserializeObject<UrlRoot>(urlResponse);
                steamId = foundSteamId.response.steamid;

                if (foundSteamId.response.success != 1)
                {
                    MessageBox.Show("Steam URL not found");
                    inputTextBox.Clear();
                    return;
                }
            }
            else if (steamId.Contains("https://steamcommunity.com/profiles/"))
            {
                string[] steamUrlSplit = steamId.Split('/');
                steamId = steamUrlSplit[4];
            }

            using (StreamReader inputFile = new StreamReader(steamId + ".txt"))
            {

                string nextLine = "";

                while (inputFile.Peek() >= 0)
                {
                    string lineContents = inputFile.ReadLine();

                    if(lineContents.Contains("Steam User:"))
                    {
                        nextLine = "Steam User";
                    }
                    else if(nextLine == "Steam User")
                    {
                        steamPlayerInstances.Add(JsonConvert.DeserializeObject<Player>(lineContents));
                        nextLine = "";
                    }

                    else if (lineContents.Contains("Steam User Bans:"))
                    {
                        nextLine = "Steam User Bans";
                    }
                    else if (nextLine == "Steam User Bans")
                    {
                        steamPlayerBansInstaces.Add(JsonConvert.DeserializeObject<PlayerBans>(lineContents));
                        nextLine = "";
                    }

                    else if (lineContents.Contains("Steam User Friends Summary:"))
                    {
                        nextLine = "Steam User Friends Summary";
                    }
                    else if (nextLine == "Steam User Friends Summary")
                    {
                        steamSummaryInstances.Add(JsonConvert.DeserializeObject<SummaryRoot>(lineContents));
                        nextLine = "";
                    }

                    else if (lineContents.Contains("Sorted Banned Players:"))
                    {
                        nextLine = "Sorted Banned Players";
                    }
                    else if(nextLine == "Sorted Banned Players")
                    {
                        sortedBannedPlayerInstances.Add(JsonConvert.DeserializeObject<List<Player>>(lineContents));
                    }
                }
            }

            steamAutomaticInvestigationLocal steamAutomaticInvestigationForm = new steamAutomaticInvestigationLocal();
            steamAutomaticInvestigationForm.Main(steamPlayerInstances, steamPlayerBansInstaces, steamSummaryInstances, sortedBannedPlayerInstances);
        }
    }
}
