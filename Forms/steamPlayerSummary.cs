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
    public partial class steamPlayerSummary : Form
    {
        public steamPlayerSummary(Player pSteamUser, PlayerBans pSteamUserBans, FriendsList pSteamUserFriends, List<SummaryRoot> pSteamFriendsSummary, int currentSummaryCount)
        {
            InitializeComponent();

            steamIdLabel.Text = "Steam ID:  " + pSteamUser.steamid;
            communityVisibilityStateLabel.Text = "Community Visibility State: " + pSteamUser.communityvisibilitystate;

            if(pSteamUser.profilestate == 0)
            {
                profileStateLabel.Text = "Profile Configured: False";
            }
            else
            {
                profileStateLabel.Text = "Profile Configured: True";
            }
            personaNameLabel.Text = "Persona Name: " + pSteamUser.personaname;
            profileUrlLinkLabel.Text = pSteamUser.profileurl;

            if (pSteamUser.lastlogoff == 0)
            {
                lastLogoffLabel.Text = "Last Logoff: Unknown";
            }
            else
            {
                lastLogoffLabel.Text = "Last Logoff: " + UnixTimeToDateTime(pSteamUser.lastlogoff);
            }

            if(pSteamUser.personastate == 0)
            {
                personaStateLabel.Text = "Persona State: Offline";
            }
            else if(pSteamUser.personastate == 1)
            {
                personaStateLabel.Text = "Persona State: Online";
            }
            else if (pSteamUser.personastate == 2)
            {
                personaStateLabel.Text = "Persona State: Busy";
            }
            else if (pSteamUser.personastate == 3)
            {
                personaStateLabel.Text = "Persona State: Away";
            }
            else if (pSteamUser.personastate == 4)
            {
                personaStateLabel.Text = "Persona State: Snooze";
            }
            else if (pSteamUser.personastate == 5)
            {
                personaStateLabel.Text = "Persona State: Looking to trade";
            }
            else if (pSteamUser.personastate == 6)
            {
                personaStateLabel.Text = "Persona State: Looking to play";
            }

            primaryClanIdLabel.Text = "Primary Clan ID: " + pSteamUser.primaryclanid;
            timeCreatedLabel.Text = "Time Created: " + UnixTimeToDateTime(pSteamUser.timecreated);
            if(pSteamUser.communityvisibilitystate == 1)
            {
                communityVisibilityStateLabel.Text = "Community Visibility: Private";
            }
            else
            {
                communityVisibilityStateLabel.Text = "Community Visibility: Public";
            }
            locCountryCodeLabel.Text = "Loc Country Code: " + pSteamUser.loccountrycode;

            communityBannedLabel.Text = "Community Banned: " + pSteamUserBans.CommunityBanned;

            if(pSteamUserBans.CommunityBanned == true)
            {
                communityBannedLabel.ForeColor = Color.Red;
            }
            else
            {
                communityBannedLabel.ForeColor = Color.Green;
            }

            vacBannedLabel.Text = "Vac Banned: " + pSteamUserBans.VACBanned;

            if (pSteamUserBans.VACBanned == true)
            {
                vacBannedLabel.ForeColor = Color.Red;
            }
            else
            {
                vacBannedLabel.ForeColor = Color.Green;
            }

            numberOfVacBansLabel.Text = "Number of VAC Bans: " + pSteamUserBans.NumberOfVACBans;

            if (pSteamUserBans.NumberOfVACBans == 0)
            {
                numberOfVacBansLabel.ForeColor = Color.Green;
            }
            else
            {
                numberOfVacBansLabel.ForeColor = Color.Red;
            }

            daysSinceLastBanLabel.Text = "Days Since Last Ban: " + pSteamUserBans.DaysSinceLastBan;

            if (pSteamUserBans.DaysSinceLastBan == 0 && pSteamUserBans.NumberOfVACBans == 0 && pSteamUserBans.NumberOfGameBans == 0 && pSteamUserBans.CommunityBanned == false && pSteamUserBans.EconomyBan == "none")
            {
                daysSinceLastBanLabel.ForeColor = Color.Green;
            }
            else
            {
                daysSinceLastBanLabel.ForeColor = Color.Red;
            }

            numberOfGameBansLabel.Text = "Number of Game Bans: " + pSteamUserBans.NumberOfGameBans;

            if (pSteamUserBans.NumberOfGameBans == 0)
            {
                numberOfGameBansLabel.ForeColor = Color.Green;
            }
            else
            {
                numberOfGameBansLabel.ForeColor = Color.Red;
            }

            economyBanLabel.Text = "Economy Ban: " + pSteamUserBans.EconomyBan;

            if (pSteamUserBans.EconomyBan == "none")
            {
                economyBanLabel.ForeColor = Color.Green;
            }
            else
            {
                economyBanLabel.ForeColor = Color.Red;
            }

            for(int i = 0; i < pSteamFriendsSummary.Count; i++)
            {
                for(int o = 0; o < pSteamFriendsSummary[i].response.players.Count; o++)
                {
                    friendsListBox.Items.Add(pSteamFriendsSummary[i].response.players[o].personaname);
                }
            }

            steamAvatarPictureBox.ImageLocation = pSteamUser.avatarfull;

            selectFriendButton.Click += new EventHandler(async (sender, EventArgs) => await selectFriendButton_Click(sender, EventArgs, currentSummaryCount, pSteamFriendsSummary));
        }

        static public DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime newDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            newDateTime = newDateTime.AddSeconds(unixtime).ToLocalTime();
            return newDateTime;
        }

        private async Task selectFriendButton_Click(object sender, EventArgs e, int currentSummaryCount, List<SummaryRoot> pSteamFriendsSummary)
        {
            string selectedFriend = friendsListBox.SelectedItem.ToString();
            int selectedIndex = 0;
            for(int i = 0; i < pSteamFriendsSummary[currentSummaryCount].response.players.Count; i++)
            {
                if (selectedFriend == pSteamFriendsSummary[currentSummaryCount].response.players[i].personaname)
                {
                    selectedIndex = i;
                    break;
                }
            }
            await getSteamPlayerFriendSummary(pSteamFriendsSummary, currentSummaryCount, selectedIndex);
        }

        private async Task getSteamPlayerFriendSummary(List<SummaryRoot> pSteamFriendsSummary, int currentSummaryCount, int selectedIndex)
        {
            using var client = new HttpClient();

            client.BaseAddress = new Uri("https://api.steampowered.com");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            string url = "/ISteamUser/GetPlayerSummaries/v2/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pSteamFriendsSummary[currentSummaryCount].response.players[selectedIndex].steamid;
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respSummary = await response.Content.ReadAsStringAsync();
            SummaryRoot steamUser = JsonConvert.DeserializeObject<SummaryRoot>(respSummary);

            url = "/ISteamUser/GetPlayerBans/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamids=" + pSteamFriendsSummary[currentSummaryCount].response.players[selectedIndex].steamid;
            response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string respBans = await response.Content.ReadAsStringAsync();
            PlayerBansRoot steamUserBans = JsonConvert.DeserializeObject<PlayerBansRoot>(respBans);

            url = "/ISteamUser/GetFriendList/v1/?key=CF1AEABEB295AA2047B7D3BDFFE95DBE&steamid=" + pSteamFriendsSummary[currentSummaryCount].response.players[selectedIndex].steamid;
            response = await client.GetAsync(url);

            FriendsRoot steamUserFriends = new FriendsRoot();
            List<SummaryRoot> steamUserFriendsSummary = new List<SummaryRoot>();

            if (response.StatusCode.ToString() == "OK")
            {
                string respFriends = await response.Content.ReadAsStringAsync();
                steamUserFriends = JsonConvert.DeserializeObject<FriendsRoot>(respFriends);

                int noOfLoops = (steamUserFriends.friendslist.friends.Count / 100) + 1;
                steamUserFriendsSummary = new List<SummaryRoot>();

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

            steamPlayerSummary steamPlayerSummaryForm = new steamPlayerSummary(steamUser.response.players[0], steamUserBans.players[0], steamUserFriends.friendslist, steamUserFriendsSummary, currentSummaryCount);
            steamPlayerSummaryForm.Show();
        }

        private void profileUrlLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.profileUrlLinkLabel.LinkVisited = true;

            System.Diagnostics.Process.Start(profileUrlLinkLabel.Text);
        }
    }
}
