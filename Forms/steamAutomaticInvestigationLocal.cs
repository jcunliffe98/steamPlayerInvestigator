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
using System.IO;

namespace steamPlayerInvestigator.Forms
{
    public partial class steamAutomaticInvestigationLocal : Form
    {
        Player steamUser { get; set; }
        PlayerBans steamUserBans { get; set; }
        SummaryRoot steamFriendsSummary { get; set; }
        List<Player> sortedBannedPlayers { get; set; }
        List<List<Player>> playerInstances { get; set; }

        List<string> uniqueSteamIds { get; set; }
        Player highestScore { get; set; }

        public steamAutomaticInvestigationLocal()
        {
            InitializeComponent();
        }

        public void Main(List<Player> pSteamUser, List<PlayerBans> pSteamUserBans, List<SummaryRoot> pSteamFriendsSummary, List<List<Player>> sortedBannedPlayers)
        {
            highestScore = new Player();
            highestScore.similarityscore = 0;

            playerInstances = new List<List<Player>>();

            uniqueSteamIds = new List<string>();

            // Gets all unique ids and puts them in a list
            for(int i = 0; i < sortedBannedPlayers.Count; i++)
            {
                for(int a = 0; a < sortedBannedPlayers[i].Count; a++)
                {
                    if(uniqueSteamIds.Contains(sortedBannedPlayers[i][a].steamid))
                    {
                        continue;
                    }
                    else
                    {
                        uniqueSteamIds.Add(sortedBannedPlayers[i][a].steamid);
                    }
                }
            }

            List<Player> bannedPlayersSingleList = new List<Player>();

            for (int i = 0; i < sortedBannedPlayers.Count; i++)
            {
                for (int a = 0; a < sortedBannedPlayers[i].Count; a++)
                {
                    bannedPlayersSingleList.Add(sortedBannedPlayers[i][a]);
                }
            }

            if (sortedBannedPlayers.Count == 1)
            {
                for(int i = 0; i < bannedPlayersSingleList.Count; i++)
                {
                    List<Player> tempList = new List<Player>();
                    tempList.Add(bannedPlayersSingleList[i]);
                    playerInstances.Add(tempList);
                }
            }
            else
            {
                for (int a = 0; a < uniqueSteamIds.Count; a++)
                {
                    List<Player> tempList = new List<Player>();
                    for (int i = 0; i < bannedPlayersSingleList.Count; i++)
                    {
                        if(uniqueSteamIds[a] == bannedPlayersSingleList[i].steamid)
                        {
                            tempList.Add(bannedPlayersSingleList[i]);
                        }
                    }
                    playerInstances.Add(tempList);
                }
            }

            DateTime averageLogOffUser;
            List<double> userLogOffs = new List<double>();

            for (int i = 0; i < pSteamUser.Count; i++)
            {
                averageLogOffUser = UnixTimeToDateTime(pSteamUser[i].lastlogoff);
                string temp = averageLogOffUser.ToString("HH:mm");
                double seconds = TimeSpan.Parse(temp).TotalSeconds;
                userLogOffs.Add(seconds);
            }

            double totalTimeSeconds = 0;

            for(int i = 0; i < userLogOffs.Count; i++)
            {
                totalTimeSeconds += userLogOffs[i];
            }

            double averageTimeSeconds = totalTimeSeconds / userLogOffs.Count;
            averageTimeSeconds = Math.Round(averageTimeSeconds);
            TimeSpan timeUserLogOff = TimeSpan.FromSeconds(averageTimeSeconds);

            steamUser = pSteamUser[pSteamUser.Count - 1];
            steamUser.averageLogOffLocal = timeUserLogOff;

            if(averageTimeSeconds == 0)
            {
                steamUserAccountLogoffLabel.Text = "Average Logoff Time: Unavailable";
            }
            else
            {
                steamUserAccountLogoffLabel.Text = "Average Logoff Time: " + steamUser.averageLogOffLocal.ToString();
            }



            userLogOffs.Clear();

            for(int i = 0; i < playerInstances.Count; i++)
            {
                totalTimeSeconds = 0;
                for(int a = 0; a < playerInstances[i].Count; a++)
                {
                    averageLogOffUser = UnixTimeToDateTime(playerInstances[i][a].lastlogoff);
                    string temp = averageLogOffUser.ToString("HH:mm");
                    double seconds = TimeSpan.Parse(temp).TotalSeconds;
                    userLogOffs.Add(seconds);
                    totalTimeSeconds += seconds;
                }
                averageTimeSeconds = totalTimeSeconds / playerInstances[i].Count;
                averageTimeSeconds = Math.Round(averageTimeSeconds);
                timeUserLogOff = TimeSpan.FromSeconds(averageTimeSeconds);
                userLogOffs.Clear();
                for (int a = 0; a < playerInstances[i].Count; a++)
                {
                    playerInstances[i][a].averageLogOffLocal = timeUserLogOff;
                }
            }

            for (int i = 0; i < pSteamUser.Count; i++)
            {
                List<Player> sortedBannedPlayersList = new List<Player>();

                sortedBannedPlayersList = sortedBannedPlayers[i];

                for(int b = 0; b < sortedBannedPlayersList.Count; b++)
                {
                    if(sortedBannedPlayersList[b].similarityscore > highestScore.similarityscore)
                    {
                        highestScore = sortedBannedPlayersList[b];
                    }
                }
            }
            steamUserAvatar.ImageLocation = steamUser.avatarfull;
            steamUserNameLabel.Text = "Steam Name: " + steamUser.personaname;
            steamUserUrlLabel.Text = "Profile Url: " + steamUser.profileurl;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + steamUser.profileurl;

            if (highestScore.averageLogOffLocal.ToString() == "00:00:00")
            {
                mostSimilarAccountLogoffLabel.Text = "Average Logoff Time: Unavailable";
            }
            else
            {
                mostSimilarAccountLogoffLabel.Text = "Average Logoff Time: " + highestScore.averageLogOffLocal.ToString();
            }

            if (steamUser.personastate == 0)
            {
                steamUser.personastatestring = "Offline";
                steamUserStatusLabel.Text = "Current Status: Offline";
            }
            else if (steamUser.personastate == 1)
            {
                steamUser.personastatestring = "Online";
                steamUserStatusLabel.Text = "Current Status: Online";
            }
            else if (steamUser.personastate == 2)
            {
                steamUser.personastatestring = "Busy";
                steamUserStatusLabel.Text = "Current Status: Busy";
            }
            else if (steamUser.personastate == 3)
            {
                steamUser.personastatestring = "Away";
                steamUserStatusLabel.Text = "Current Status: Away";
            }
            else if (steamUser.personastate == 4)
            {
                steamUser.personastatestring = "Snooze";
                steamUserStatusLabel.Text = "Current Status: Snooze";
            }
            else if (steamUser.personastate == 5)
            {
                steamUser.personastatestring = "Looking to trade";
                steamUserStatusLabel.Text = "Current Status: Looking to trade";
            }
            else if (steamUser.personastate == 6)
            {
                steamUser.personastatestring = "Looking to play";
                steamUserStatusLabel.Text = "Current Status: Looking to play";
            }

            if (steamUser.timecreated == 0)
            {
                steamUserAccountCreatedLabel.Text = "Account Created: Unknown";
            }
            else
            {
                steamUserAccountCreatedLabel.Text = "Account Created: " + UnixTimeToDateTime(steamUser.timecreated);
            }

            steamUserClanLabel.Text = "Primary Clan ID: " + steamUser.primaryclanid;

            if (steamUser.loccountrycode == "" || steamUser.loccountrycode == null)
            {
                steamUserCountryCodeLabel.Text = "Country Code: Unknown";
            }
            else
            {
                steamUserCountryCodeLabel.Text = "Country Code: " + steamUser.loccountrycode;
            }

            steamUserBanCountLabel.Text = "Number of bans: " + (steamUser.NumberOfGameBans + steamUser.NumberOfVACBans);
            steamUserDaysSinceLastBanLabel.Text = "Days since last ban: " + steamUser.DaysSinceLastBan;

            steamSimilarAccountAvatar.ImageLocation = highestScore.avatarfull;
            steamSimilarAccountNameLabel.Text = "Steam Name: " + highestScore.personaname;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + highestScore.profileurl;

            if (highestScore.personastate == 0)
            {
                highestScore.personastatestring = "Offline";
                steamSimilarAccountStatusLabel.Text = "Current Status: Offline";
            }
            else if (highestScore.personastate == 1)
            {
                highestScore.personastatestring = "Online";
                steamSimilarAccountStatusLabel.Text = "Current Status: Online";
            }
            else if (highestScore.personastate == 2)
            {
                highestScore.personastatestring = "Busy";
                steamSimilarAccountStatusLabel.Text = "Current Status: Busy";
            }
            else if (highestScore.personastate == 3)
            {
                highestScore.personastatestring = "Away";
                steamSimilarAccountStatusLabel.Text = "Current Status: Away";
            }
            else if (highestScore.personastate == 4)
            {
                highestScore.personastatestring = "Snooze";
                steamSimilarAccountStatusLabel.Text = "Current Status: Snooze";
            }
            else if (highestScore.personastate == 5)
            {
                highestScore.personastatestring = "Looking to trade";
                steamSimilarAccountStatusLabel.Text = "Current Status: Looking to trade";
            }
            else if (highestScore.personastate == 6)
            {
                highestScore.personastatestring = "Looking to play";
                steamSimilarAccountStatusLabel.Text = "Current Status: Looking to play";
            }

            if (highestScore.timecreated == 0)
            {
                steamSimilarAccountCreatedLabel.Text = "Account Created: Unknown";
            }
            else
            {
                steamSimilarAccountCreatedLabel.Text = "Account Created: " + UnixTimeToDateTime(highestScore.timecreated);
            }

            steamSimilarAccountClanLabel.Text = "Primary Clan ID: " + highestScore.primaryclanid;

            if (highestScore.loccountrycode == "" || highestScore.loccountrycode == null)
            {
                steamSimilarAccountCountryCodeLabel.Text = "Country Code: Unknown";
            }
            else
            {
                steamSimilarAccountCountryCodeLabel.Text = "Country Code: " + highestScore.loccountrycode;
            }

            similarAccountBanCountLabel.Text = "Number of bans: " + (highestScore.NumberOfGameBans + highestScore.NumberOfVACBans);
            similarAccountDaysSinceLastBanLabel.Text = "Days since last ban: " + highestScore.DaysSinceLastBan;

            steamNamesLabel.Text = "Steam names are " + Math.Round(highestScore.levDistancePersona * 100, 2) + "% similar";

            if (highestScore.levDistanceUrl == -1)
            {
                steamUrlLabel.Text = "A user doesn't have a custom URL set so a comparison isn't possible";
                steamNameEffectLabel.Text = "+" + (Math.Round(highestScore.levDistancePersona * 100, 2)).ToString();
            }
            else
            {
                steamUrlLabel.Text = "Profile urls are " + Math.Round(highestScore.levDistanceUrl) * 100 + "% similar";
                steamNameEffectLabel.Text = "+" + Math.Round((highestScore.levDistancePersona + highestScore.levDistanceUrl) * 100).ToString();
            }

            if (steamUser.personastatestring == highestScore.personastatestring)
            {
                label17.Text = "Both users are " + steamUser.personastatestring + " at the same time";
                steamStatusEffectLabel.Text = "-10";
            }
            else
            {
                label17.Text = "User is " + steamUser.personastatestring + " while similar account is " + highestScore.personastatestring;
                steamStatusEffectLabel.Text = "+10";
            }

            if (highestScore.createdAfter == true)
            {
                timeCreatedLabel.Text = "Similar account was created after user account";
                steamTimeCreatedEffectLabel.Text = "+5";
            }
            else
            {
                timeCreatedLabel.Text = "User account was created before similar account";
                steamTimeCreatedEffectLabel.Text = "-5";
            }

            if (steamUser.primaryclanid == highestScore.primaryclanid)
            {
                clanIdLabel.Text = "Primary clans are the same";
                steamPrimaryClanEffectLabel.Text = "+5";
            }
            else
            {
                clanIdLabel.Text = "Primary clans are not the same";
                steamPrimaryClanEffectLabel.Text = "-5";
            }

            if (steamUser.loccountrycode == null || highestScore.loccountrycode == null)
            {
                countryCodeLabel.Text = "Comparison can't be made";
                steamCountryCodeEffectLabel.Text = "0";
            }
            else if (steamUser.loccountrycode == highestScore.loccountrycode)
            {
                countryCodeLabel.Text = "Both accounts share same country code";
                steamCountryCodeEffectLabel.Text = "+5";
            }
            else
            {
                countryCodeLabel.Text = "Both accounts don't share same country code";
                steamCountryCodeEffectLabel.Text = "-5";
            }

            similarityScoreLabel.Text = "Overall Similarity Score: " + Math.Round(highestScore.similarityscore, 2);
            instanceCountLabel.Text = "Number of local instances: " + pSteamUser.Count;
            highestScore.mutualPercent = highestScore.mutualPercent * 100;
            mostSimilarAccountFriendSimilarityLabel.Text = "Friend Similarity: " + Math.Round(highestScore.mutualPercent).ToString() + "%";

            Show();
            Console.ReadLine();
        }

        public Player getPlayer(SummaryRoot pSteamFriendsSummary, int i, int y)
        {
            Player tempPlayer = new Player();
            tempPlayer.avatar = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatar;
            tempPlayer.avatarfull = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatarfull;
            tempPlayer.avatarhash = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatarhash;
            tempPlayer.avatarmedium = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].avatarmedium;
            tempPlayer.CommunityBanned = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].CommunityBanned;
            tempPlayer.communityvisibilitystate = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].communityvisibilitystate;
            tempPlayer.DaysSinceLastBan = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].DaysSinceLastBan;
            tempPlayer.EconomyBan = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].EconomyBan;
            tempPlayer.friendsOfFriends = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].friendsOfFriends;
            tempPlayer.friend_since = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].friend_since;
            tempPlayer.lastlogoff = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].lastlogoff;
            tempPlayer.loccountrycode = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].loccountrycode;
            tempPlayer.NumberOfGameBans = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].NumberOfGameBans;
            tempPlayer.NumberOfVACBans = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].NumberOfVACBans;
            tempPlayer.personaname = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].personaname;
            tempPlayer.personastate = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].personastate;
            tempPlayer.personastateflags = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].personastateflags;
            tempPlayer.primaryclanid = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].primaryclanid;
            tempPlayer.profilestate = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].profilestate;
            tempPlayer.profileurl = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].profileurl;
            tempPlayer.relationship = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].relationship;
            tempPlayer.steamid = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].steamid;
            tempPlayer.timecreated = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].timecreated;
            tempPlayer.VACBanned = pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].VACBanned;
            return tempPlayer;
        }

        public double levenshteinPercentage(string playerName, string friendName)
        {
            if (playerName == friendName)
            {
                return 1.0;
            }

            int stepsToSame = getLevenshteinDistance(playerName, friendName);
            double percentage = (1.0 - ((double)stepsToSame / (double)Math.Max(playerName.Length, friendName.Length)));
            return percentage;
        }

        public int getLevenshteinDistance(string playerName, string friendName)
        {
            int playerNameLength = playerName.Length;
            int friendNameLength = friendName.Length;

            int[,] lengthArray = new int[playerNameLength + 1, friendNameLength + 1];


            for (int i = 0; i <= playerNameLength; i++)
            {
                lengthArray[i, 0] = i;
            }

            for (int y = 0; y <= friendNameLength; lengthArray[0, y] = y++)
            {
                lengthArray[0, y] = y;
            }


            for (int i = 1; i <= playerNameLength; i++)
            {

                for (int y = 1; y <= friendNameLength; y++)
                {
                    int cost;
                    if (friendName[y - 1] == playerName[i - 1])
                    {
                        cost = 0;
                    }
                    else
                    {
                        cost = 1;
                    }

                    lengthArray[i, y] = Math.Min(Math.Min(lengthArray[i - 1, y] + 1, lengthArray[i, y - 1] + 1), lengthArray[i - 1, y - 1] + cost);
                }
            }

            return lengthArray[playerNameLength, friendNameLength];
        }

        static public DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime newDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            newDateTime = newDateTime.AddSeconds(unixtime).ToLocalTime();
            return newDateTime;
        }
        private List<Player> checkFriendsSimilarAccount(List<Player> bannedPlayers, SummaryRoot pSteamFriendsSummary)
        {
            for (int i = 0; i < bannedPlayers.Count; i++)
            {
                bannedPlayers[i].mutualFriendCount = 0;
                if (bannedPlayers[i].friendsOfFriends == null)
                {
                    continue;
                }
                for (int y = 0; y < bannedPlayers[i].friendsOfFriends.friendslist.friends.Count; y++)
                {
                    for (int z = 0; z < pSteamFriendsSummary.response.players.Count; z++)
                    {
                        if (bannedPlayers[i].friendsOfFriends.friendslist.friends[y].steamid == pSteamFriendsSummary.response.players[z].steamid)
                        {
                            bannedPlayers[i].mutualFriendCount++;
                        }
                    }
                }
                bannedPlayers[i].mutualPercent = (double)bannedPlayers[i].mutualFriendCount / (double)pSteamFriendsSummary.response.players.Count;
            }
            return bannedPlayers;
        }

        //private List<Player> checkFriendsSteamUser(Player pMostSimilarAccount, List<SummaryRoot> pSteamFriendsSummary)
        //{
            //for (int i = 0; i < pMostSimilarAccount.friendsOfFriends.friendslist.friends.Count; i++)
            //{
                //pMostSimilarAccount.mutualFriendCount = 0;
                //if (pMostSimilarAccount.friendsOfFriends == null)
                //{
                    //break;
                //}
                //for (int y = 0; y < pSteamFriendsSummary.Count; y++)
                //{
                    //for (int z = 0; z < pSteamFriendsSummary[y].response.players.Count; z++)
                    //{
                        //if (pSteamFriendsSummary[i].response.players[y].friendsOfFriends == pSteamFriendsSummary.response.players[z].steamid)
                        //{
                            //bannedPlayers[i].mutualFriendCount++;
                        //}
                    //}
                //}
                //bannedPlayers[i].mutualPercent = (double)bannedPlayers[i].mutualFriendCount / (double)pSteamFriendsSummary.response.players.Count;
            //}
            //return bannedPlayers;
        //}
    }
}
