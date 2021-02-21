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

namespace steamPlayerInvestigator.Forms
{
    public partial class steamAutomaticInvestigation : Form
    {

        public steamAutomaticInvestigation(Player pSteamUser, PlayerBans pSteamUserBans, SummaryRoot pSteamFriendsSummary)
        {
            InitializeComponent();


            List<Player> bannedPlayers = new List<Player>();
            string shortUrlPlayer = "";
            string shortUrlFriend;
            bool shortUrlBool = false;
            string[] steamUrlSplit;

            if (pSteamUser.profileurl.Contains("id"))
            {
                steamUrlSplit = pSteamUser.profileurl.Split('/');
                shortUrlPlayer = steamUrlSplit[4];
                shortUrlBool = true;
            }

            for(int i = 0; i < pSteamFriendsSummary.response.players.Count; i++)
            {
                if(pSteamFriendsSummary.response.players[i].VACBanned == true || pSteamFriendsSummary.response.players[i].NumberOfGameBans != 0)
                {
                    bannedPlayers.Add(pSteamFriendsSummary.response.players[i]);
                }
                if(pSteamFriendsSummary.response.players[i].friendsOfFriends == null || pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist == null)
                {
                    continue;
                }
                for(int y = 0; y < pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends.Count; y++)
                {
                    if (pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].VACBanned == true ||
                        pSteamFriendsSummary.response.players[i].friendsOfFriends.friendslist.friends[y].NumberOfGameBans != 0)
                    {
                        Player tempPlayer = getPlayer(pSteamFriendsSummary, i, y);

                        bannedPlayers.Add(tempPlayer);
                    }
                }
            }

            bannedPlayers = removeDuplicates(bannedPlayers);

            for (int i = 0; i < bannedPlayers.Count; i++)
            {
                if(bannedPlayers[i].personaname == null)
                {
                    continue;
                }
                bannedPlayers[i].levDistancePersona = levenshteinPercentage(pSteamUser.personaname, bannedPlayers[i].personaname);
                bannedPlayers[i].levDistanceUrl = -1;
            }

            if (shortUrlBool == true)
            {
                for (int i = 0; i < bannedPlayers.Count; i++)
                {
                    if (bannedPlayers[i].profileurl == null)
                    {
                        continue;
                    }
                    else if (bannedPlayers[i].profileurl.Contains(bannedPlayers[i].steamid))
                    {
                        bannedPlayers[i].levDistanceUrl = -1;
                    }
                    else
                    {
                        steamUrlSplit = bannedPlayers[i].profileurl.Split('/');
                        shortUrlFriend = steamUrlSplit[4];
                        if (bannedPlayers[i].profileurl == null)
                        {
                            continue;
                        }
                        bannedPlayers[i].levDistanceUrl = levenshteinPercentage(shortUrlPlayer, shortUrlFriend);
                    }
                }
            }

            for (int i = 0; i < bannedPlayers.Count; i++)
            {
                if(bannedPlayers[i].levDistanceUrl == -1)
                {
                    bannedPlayers[i].similarityscore = bannedPlayers[i].levDistancePersona;
                }
                else
                {
                    bannedPlayers[i].similarityscore = (bannedPlayers[i].levDistancePersona + bannedPlayers[i].levDistanceUrl) / 2;
                }

                if (bannedPlayers[i].timecreated == 0 || pSteamUser.timecreated == 0)
                {
                    continue;
                }
                else if (bannedPlayers[i].timecreated > pSteamUser.timecreated)
                {
                    bannedPlayers[i].createdAfter = true;

                    // Will need to adjust later
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore + 5;
                }
                else
                {
                    bannedPlayers[i].createdAfter = false;

                    // Will need to adjust later
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore - 5;
                }

                if (bannedPlayers[i].loccountrycode == null || pSteamUser.loccountrycode == null)
                {
                    continue;
                }
                else if (bannedPlayers[i].loccountrycode == pSteamUser.loccountrycode)
                {
                    bannedPlayers[i].sameCountry = true;

                    // Will need to adjust later
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore + 5;
                }
                else
                {
                    bannedPlayers[i].sameCountry = false;

                    // Will need to adjust later
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore - 5;
                }

                if(bannedPlayers[i].personastate == 1 && pSteamUser.personastate == 1)
                {
                    // Will need to adjust later
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore - 10;
                    bannedPlayers[i].onlineAtSameTime = true;
                }
                else
                {
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore + 10;
                    bannedPlayers[i].onlineAtSameTime = false;
                }
            }

            steamUserAvatar.ImageLocation = pSteamUser.avatarfull;
            steamUserNameLabel.Text = "Steam Name: " + pSteamUser.personaname;
            steamUserUrlLabel.Text = "Profile Url: " + pSteamUser.profileurl;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + pSteamUser.profileurl;

            if (pSteamUser.personastate == 0)
            {
                steamUserStatusLabel.Text = "Current Status: Offline";
            }
            else if (pSteamUser.personastate == 1)
            {
                steamUserStatusLabel.Text = "Current Status: Online";
            }
            else if (pSteamUser.personastate == 2)
            {
                steamUserStatusLabel.Text = "Current Status: Busy";
            }
            else if (pSteamUser.personastate == 3)
            {
                steamUserStatusLabel.Text = "Current Status: Away";
            }
            else if (pSteamUser.personastate == 4)
            {
                steamUserStatusLabel.Text = "Current Status: Snooze";
            }
            else if (pSteamUser.personastate == 5)
            {
                steamUserStatusLabel.Text = "Current Status: Looking to trade";
            }
            else if (pSteamUser.personastate == 6)
            {
                steamUserStatusLabel.Text = "Current Status: Looking to play";
            }

            if(pSteamUser.timecreated == 0)
            {
                steamUserAccountCreatedLabel.Text = "Account Created: Unknown";
            }
            else
            {
                steamUserAccountCreatedLabel.Text = "Account Created: " + UnixTimeToDateTime(pSteamUser.timecreated);
            }

            steamUserClanLabel.Text = "Primary Clan ID: " + pSteamUser.primaryclanid;

            if(pSteamUser.loccountrycode == "" || pSteamUser.loccountrycode == null)
            {
                steamUserCountryCodeLabel.Text = "Country Code: Unknown";
            }
            else
            {
                steamUserCountryCodeLabel.Text = "Country Code: " + pSteamUser.loccountrycode;
            }

            steamUserBanCountLabel.Text = "Number of bans: " + (pSteamUser.NumberOfGameBans + pSteamUser.NumberOfVACBans);
            steamUserDaysSinceLastBanLabel.Text = "Days since last ban: " + pSteamUser.DaysSinceLastBan;

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
                    if(friendName[y - 1] == playerName[i - 1])
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



        public List<Player> removeDuplicates(List<Player> bannedPlayers)
        {
            List<Player> removedDuplicatesBannedPlayers = new List<Player>();
            for (int i = 0; i < bannedPlayers.Count; i++)
            {
                bool duplicateFound = false;
                for (int y = 0; y < i; y++)
                {
                    if (bannedPlayers[i].steamid == bannedPlayers[y].steamid)
                    {
                        duplicateFound = true;
                        break;
                    }
                }
                if (!duplicateFound)
                {
                    removedDuplicatesBannedPlayers.Add(bannedPlayers[i]);
                }
            }
            return removedDuplicatesBannedPlayers;
        }

        static public DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime newDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            newDateTime = newDateTime.AddSeconds(unixtime).ToLocalTime();
            return newDateTime;
        }
    }
}
