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
            for (int i = 0; i < sortedBannedPlayers.Count; i++)
            {
                for (int a = 0; a < sortedBannedPlayers[i].Count; a++)
                {
                    if (uniqueSteamIds.Contains(sortedBannedPlayers[i][a].steamid))
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
                for (int i = 0; i < bannedPlayersSingleList.Count; i++)
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
                        if (uniqueSteamIds[a] == bannedPlayersSingleList[i].steamid)
                        {
                            tempList.Add(bannedPlayersSingleList[i]);
                        }
                    }
                    playerInstances.Add(tempList);
                }
            }

            DateTime averageLogOffUser;
            List<double> userLogOffs = new List<double>();
            bool logOffAvailable = false;

            for (int i = 0; i < pSteamUser.Count; i++)
            {
                
                if (pSteamUser[i].lastlogoff == 0)
                {

                }
                else
                {
                    averageLogOffUser = UnixTimeToDateTime(pSteamUser[i].lastlogoff);
                    string temp = averageLogOffUser.ToString("HH:mm");
                    double seconds = TimeSpan.Parse(temp).TotalSeconds;
                    userLogOffs.Add(seconds);
                    logOffAvailable = true;
                }

            }

            double totalTimeSeconds = 0;

            for (int i = 0; i < userLogOffs.Count; i++)
            {
                totalTimeSeconds += userLogOffs[i];
            }

            double averageTimeSeconds = totalTimeSeconds / userLogOffs.Count;
            averageTimeSeconds = Math.Round(averageTimeSeconds);
            TimeSpan timeUserLogOff = TimeSpan.FromSeconds(averageTimeSeconds);

            steamUser = pSteamUser[pSteamUser.Count - 1];
            steamUser.averageLogOffLocal = timeUserLogOff;

            if(logOffAvailable == false)
            {
                steamUser.logOffAvailable = false;
            }
            else
            {
                steamUser.logOffAvailable = true;
            }

            if (averageTimeSeconds == 0)
            {
                steamUserAccountLogoffLabel.Text = "Average Logoff Time: Unavailable";
            }
            else
            {
                steamUserAccountLogoffLabel.Text = "Average Logoff Time: " + steamUser.averageLogOffLocal.ToString();
            }

            userLogOffs.Clear();

            for (int i = 0; i < playerInstances.Count; i++)
            {
                logOffAvailable = false;
                totalTimeSeconds = 0;
                for (int a = 0; a < playerInstances[i].Count; a++)
                {
                    if (playerInstances[i][a].lastlogoff == 0)
                    {

                    }
                    else
                    {
                        averageLogOffUser = UnixTimeToDateTime(playerInstances[i][a].lastlogoff);
                        string temp = averageLogOffUser.ToString("HH:mm");
                        double seconds = TimeSpan.Parse(temp).TotalSeconds;
                        userLogOffs.Add(seconds);
                        totalTimeSeconds += seconds;
                        logOffAvailable = true;
                    }
                }
                if(logOffAvailable == false)
                {
                    for (int a = 0; a < playerInstances[i].Count; a++)
                    {
                        playerInstances[i][a].logOffAvailable = false;
                    }
                }
                else
                {
                    averageTimeSeconds = totalTimeSeconds / playerInstances[i].Count;
                    averageTimeSeconds = Math.Round(averageTimeSeconds);
                    timeUserLogOff = TimeSpan.FromSeconds(averageTimeSeconds);
                    userLogOffs.Clear();
                    for (int a = 0; a < playerInstances[i].Count; a++)
                    {
                        playerInstances[i][a].averageLogOffLocal = timeUserLogOff;
                        playerInstances[i][a].logOffAvailable = true;
                    }
                }

            }

            List<Player> masterBannedPlayerList = new List<Player>();

            for(int i = 0; i < playerInstances.Count; i++)
            {
                masterBannedPlayerList.Add(playerInstances[i][playerInstances[i].Count() - 1]);

                double averageLevPersona = 0;
                double averageLevUrl = 0;
                double averageStatus = 0;
                double averageClanId = 0;
                double averageCountryCode = 0;
                double averageFriendSelf = 0;
                double averageFriendUser = 0;

                int averageLevPersonaCount = 0;
                int averageLevUrlCount = 0;
                int averageStatusCount = 0;
                int averageClanIdCount = 0;
                int averageCountryCodeCount = 0;
                int averageFriendSelfCount = 0;
                int averageFriendUserCount = 0;

                if (masterBannedPlayerList[i].logOffAvailable == true)
                {
                    masterBannedPlayerList[i].averageLogOffLocal = playerInstances[i][0].averageLogOffLocal;
                }

                if (playerInstances[i][0].timecreated == 0 || pSteamUser[0].timecreated == 0)
                {
                }
                else if (playerInstances[i][0].timecreated > pSteamUser[0].timecreated)
                {
                    masterBannedPlayerList[i].similarityscore = masterBannedPlayerList[i].similarityscore - 5;
                }
                else
                {
                    masterBannedPlayerList[i].similarityscore = masterBannedPlayerList[i].similarityscore + 5;
                }

                for (int a = 0; a < playerInstances[i].Count; a++)
                {
                    if (playerInstances[i][a].personastate == 0 && pSteamUser[a].personastate == 0)
                    {
                    }
                    else if (playerInstances[i][a].personastate == pSteamUser[a].personastate)
                    {
                        averageStatus = averageStatus + 10;
                        averageStatusCount++;
                    }
                    else
                    {
                        averageStatus = averageStatus - 10;
                        averageStatusCount++;
                    }

                    if(playerInstances[i][a].friendsOfFriends == null || playerInstances[i][a].friendsOfFriends.friendslist == null)
                    {

                    }
                    else
                    {
                        averageFriendSelf = averageFriendSelf + playerInstances[i][a].mutualPercentAgainstSelf;
                        averageFriendUser = averageFriendUser + playerInstances[i][a].mutualPercentAgainstUser;

                        averageFriendSelfCount++;
                        averageFriendUserCount++;
                    }

                    for (int b = 0; b < pSteamUser.Count; b++)
                    {
                         averageLevPersona = averageLevPersona + levenshteinPercentage(playerInstances[i][a].personaname, pSteamUser[b].personaname);
                         averageLevPersonaCount++;

                        if(playerInstances[i][a].levDistanceUrl == -1 || pSteamUser[b].levDistanceUrl == -1)
                        {

                        }
                        else
                        {
                            averageLevUrl = averageLevUrl + levenshteinPercentage(playerInstances[i][a].profileurl, pSteamUser[b].profileurl);
                            averageLevUrlCount++;
                        }

                        if (playerInstances[i][a].primaryclanid == null || pSteamUser[b].primaryclanid == null)
                        {
                        }
                        else if (playerInstances[i][a].primaryclanid == pSteamUser[b].primaryclanid)
                        {
                            averageClanId = averageClanId + 5;
                            averageClanIdCount++;
                        }
                        else
                        {
                            averageClanId = averageClanId - 5;
                            averageClanIdCount++;
                        }

                        if (playerInstances[i][a].loccountrycode == null || pSteamUser[b].loccountrycode == null)
                        {
                        }
                        else if (playerInstances[i][a].loccountrycode == pSteamUser[b].loccountrycode)
                        {
                            averageCountryCode = averageCountryCode + 5;
                            averageCountryCodeCount++;
                        }
                        else
                        {
                            averageCountryCode = averageCountryCode - 5;
                            averageCountryCodeCount++;
                        }
                    }
                }
                averageLevPersona = averageLevPersona / averageLevPersonaCount;
                masterBannedPlayerList[i].levDistancePersona = averageLevPersona;

                if(averageStatusCount != 0)
                {
                    averageStatus = averageStatus / averageStatusCount;
                }
                if (averageClanIdCount != 0)
                {
                    averageClanId = averageClanId / averageClanIdCount;
                }
                if (averageCountryCodeCount != 0)
                {
                    averageCountryCode = averageCountryCode / averageCountryCodeCount;
                }
                if (averageFriendSelfCount != 0)
                {
                    averageFriendSelf = (averageFriendSelf / averageFriendSelfCount) * 100;
                }
                if (averageFriendUserCount != 0)
                {
                    averageFriendUser = (averageFriendUser / averageFriendUserCount) * 100;
                }

                if (averageLevUrlCount == 0)
                {
                    masterBannedPlayerList[i].levDistanceUrl = -1;
                }
                else
                {
                    averageLevUrl = averageLevUrl / averageLevUrlCount;
                    masterBannedPlayerList[i].levDistanceUrl = averageLevUrl;
                }

                masterBannedPlayerList[i].similarityscore = 0;
                masterBannedPlayerList[i].similarityscore += Math.Round(averageLevPersona * 100);

                if (masterBannedPlayerList[i].levDistanceUrl == -1)
                {

                }
                else
                {
                    masterBannedPlayerList[i].similarityscore += Math.Round(averageLevUrl * 100);
                }

                masterBannedPlayerList[i].similarityscore += averageStatus;
                masterBannedPlayerList[i].similarityscore += averageClanId;
                masterBannedPlayerList[i].similarityscore += averageCountryCode;
                masterBannedPlayerList[i].similarityscore += averageFriendSelf;
                masterBannedPlayerList[i].similarityscore += averageFriendUser;

                if (steamUser.logOffAvailable == false || masterBannedPlayerList[i].logOffAvailable == false)
                {

                }
                else
                {
                    int hours = (steamUser.averageLogOffLocal - masterBannedPlayerList[i].averageLogOffLocal).Hours;
                    if (hours == 0)
                    {
                        masterBannedPlayerList[i].similarityscore = masterBannedPlayerList[i].similarityscore + 10;
                    }
                    else
                    {
                        masterBannedPlayerList[i].similarityscore = masterBannedPlayerList[i].similarityscore - 10;
                    }
                }
            }

            masterBannedPlayerList = masterBannedPlayerList.OrderByDescending(o => o.similarityscore).ToList();
            SetLabels(pSteamUser, masterBannedPlayerList);

            Show();
            Console.ReadLine();
        }

        private void SetLabels(List<Player> pSteamUser, List<Player> masterBannedPlayerList)
        {
            steamUserAvatar.ImageLocation = steamUser.avatarfull;
            steamUserNameLabel.Text = "Steam Name: " + steamUser.personaname;
            steamUserUrlLabel.Text = "Profile Url: " + steamUser.profileurl;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + steamUser.profileurl;

            if (masterBannedPlayerList[0].logOffAvailable == false)
            {
                mostSimilarAccountLogoffLabel.Text = "Average Logoff Time: Unavailable";
            }
            else
            {
                mostSimilarAccountLogoffLabel.Text = "Average Logoff Time: " + masterBannedPlayerList[0].averageLogOffLocal.ToString();
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

            steamSimilarAccountAvatar.ImageLocation = masterBannedPlayerList[0].avatarfull;
            steamSimilarAccountNameLabel.Text = "Steam Name: " + masterBannedPlayerList[0].personaname;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + masterBannedPlayerList[0].profileurl;

            if (masterBannedPlayerList[0].personastate == 0)
            {
                masterBannedPlayerList[0].personastatestring = "Offline";
                steamSimilarAccountStatusLabel.Text = "Current Status: Offline";
            }
            else if (masterBannedPlayerList[0].personastate == 1)
            {
                masterBannedPlayerList[0].personastatestring = "Online";
                steamSimilarAccountStatusLabel.Text = "Current Status: Online";
            }
            else if (masterBannedPlayerList[0].personastate == 2)
            {
                masterBannedPlayerList[0].personastatestring = "Busy";
                steamSimilarAccountStatusLabel.Text = "Current Status: Busy";
            }
            else if (masterBannedPlayerList[0].personastate == 3)
            {
                masterBannedPlayerList[0].personastatestring = "Away";
                steamSimilarAccountStatusLabel.Text = "Current Status: Away";
            }
            else if (masterBannedPlayerList[0].personastate == 4)
            {
                masterBannedPlayerList[0].personastatestring = "Snooze";
                steamSimilarAccountStatusLabel.Text = "Current Status: Snooze";
            }
            else if (masterBannedPlayerList[0].personastate == 5)
            {
                masterBannedPlayerList[0].personastatestring = "Looking to trade";
                steamSimilarAccountStatusLabel.Text = "Current Status: Looking to trade";
            }
            else if (masterBannedPlayerList[0].personastate == 6)
            {
                masterBannedPlayerList[0].personastatestring = "Looking to play";
                steamSimilarAccountStatusLabel.Text = "Current Status: Looking to play";
            }

            if (masterBannedPlayerList[0].timecreated == 0)
            {
                steamSimilarAccountCreatedLabel.Text = "Account Created: Unknown";
            }
            else
            {
                steamSimilarAccountCreatedLabel.Text = "Account Created: " + UnixTimeToDateTime(masterBannedPlayerList[0].timecreated);
            }

            steamSimilarAccountClanLabel.Text = "Primary Clan ID: " + masterBannedPlayerList[0].primaryclanid;

            if (masterBannedPlayerList[0].loccountrycode == "" || masterBannedPlayerList[0].loccountrycode == null)
            {
                steamSimilarAccountCountryCodeLabel.Text = "Country Code: Unknown";
            }
            else
            {
                steamSimilarAccountCountryCodeLabel.Text = "Country Code: " + masterBannedPlayerList[0].loccountrycode;
            }

            similarAccountBanCountLabel.Text = "Number of bans: " + (masterBannedPlayerList[0].NumberOfGameBans + masterBannedPlayerList[0].NumberOfVACBans);
            similarAccountDaysSinceLastBanLabel.Text = "Days since last ban: " + masterBannedPlayerList[0].DaysSinceLastBan;

            steamNamesLabel.Text = "Steam names are " + Math.Round(masterBannedPlayerList[0].levDistancePersona * 100, 2) + "% similar";

            if (masterBannedPlayerList[0].levDistanceUrl == -1)
            {
                steamUrlLabel.Text = "A user doesn't have a custom URL set so a comparison isn't possible";
                steamNameEffectLabel.Text = "+" + (Math.Round(masterBannedPlayerList[0].levDistancePersona * 100, 2)).ToString();
            }
            else
            {
                steamUrlLabel.Text = "Profile urls are " + Math.Round(masterBannedPlayerList[0].levDistanceUrl) * 100 + "% similar";
                steamNameEffectLabel.Text = "+" + Math.Round((masterBannedPlayerList[0].levDistancePersona + masterBannedPlayerList[0].levDistanceUrl) * 100).ToString();
            }

            if (steamUser.personastatestring == masterBannedPlayerList[0].personastatestring)
            {
                label17.Text = "Both users are " + steamUser.personastatestring + " at the same time";
                steamStatusEffectLabel.Text = "-10";
            }
            else
            {
                label17.Text = "User is " + steamUser.personastatestring + " while similar account is " + masterBannedPlayerList[0].personastatestring;
                steamStatusEffectLabel.Text = "+10";
            }

            if (masterBannedPlayerList[0].createdAfter == true)
            {
                timeCreatedLabel.Text = "Similar account was created after user account";
                steamTimeCreatedEffectLabel.Text = "+5";
            }
            else
            {
                timeCreatedLabel.Text = "User account was created before similar account";
                steamTimeCreatedEffectLabel.Text = "-5";
            }

            if (steamUser.primaryclanid == masterBannedPlayerList[0].primaryclanid)
            {
                clanIdLabel.Text = "Primary clans are the same";
                steamPrimaryClanEffectLabel.Text = "+5";
            }
            else
            {
                clanIdLabel.Text = "Primary clans are not the same";
                steamPrimaryClanEffectLabel.Text = "-5";
            }

            if (steamUser.loccountrycode == null || masterBannedPlayerList[0].loccountrycode == null)
            {
                countryCodeLabel.Text = "Comparison can't be made";
                steamCountryCodeEffectLabel.Text = "0";
            }
            else if (steamUser.loccountrycode == masterBannedPlayerList[0].loccountrycode)
            {
                countryCodeLabel.Text = "Both accounts share same country code";
                steamCountryCodeEffectLabel.Text = "+5";
            }
            else
            {
                countryCodeLabel.Text = "Both accounts don't share same country code";
                steamCountryCodeEffectLabel.Text = "-5";
            }

            similarityScoreLabel.Text = "Overall Similarity Score: " + Math.Round(masterBannedPlayerList[0].similarityscore, 2);
            instanceCountLabel.Text = "Number of local instances: " + pSteamUser.Count;
            masterBannedPlayerList[0].mutualPercentAgainstUser = masterBannedPlayerList[0].mutualPercentAgainstUser * 100;
            mostSimilarAccountFriendSimilarityLabel.Text = "Friend Similarity: " + Math.Round(masterBannedPlayerList[0].mutualPercentAgainstUser).ToString() + "%";
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
    }
}
