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

namespace steamPlayerInvestigator.Forms
{
    public partial class steamAutomaticInvestigation : Form
    {

        public steamAutomaticInvestigation(Player pSteamUser, PlayerBans pSteamUserBans, SummaryRoot pSteamFriendsSummary)
        {
            InitializeComponent();

            pSteamUser.CommunityBanned = pSteamUserBans.CommunityBanned;
            pSteamUser.DaysSinceLastBan = pSteamUserBans.DaysSinceLastBan;
            pSteamUser.EconomyBan = pSteamUserBans.EconomyBan;
            pSteamUser.NumberOfGameBans = pSteamUserBans.NumberOfGameBans;
            pSteamUser.NumberOfVACBans = pSteamUserBans.NumberOfVACBans;
            pSteamUser.VACBanned = pSteamUserBans.VACBanned;

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
                if(bannedPlayers[i].steamid == pSteamUser.steamid)
                {
                    bannedPlayers.Remove(bannedPlayers[i]);
                }
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

                bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore * 100;

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

                if(bannedPlayers[i].personastate == pSteamUser.personastate)
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

                if (bannedPlayers[i].primaryclanid == pSteamUser.primaryclanid)
                {
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore + 5;
                }
                else
                {
                    bannedPlayers[i].similarityscore = bannedPlayers[i].similarityscore - 5;
                }
            }

            List<Player> sortedBannedPlayers = bannedPlayers.OrderByDescending(o => o.similarityscore).ToList();

            steamUserAvatar.ImageLocation = pSteamUser.avatarfull;
            steamUserNameLabel.Text = "Steam Name: " + pSteamUser.personaname;
            steamUserUrlLabel.Text = "Profile Url: " + pSteamUser.profileurl;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + pSteamUser.profileurl;

            if (pSteamUser.personastate == 0)
            {
                pSteamUser.personastatestring = "Offline";
                steamUserStatusLabel.Text = "Current Status: Offline";
            }
            else if (pSteamUser.personastate == 1)
            {
                pSteamUser.personastatestring = "Online";
                steamUserStatusLabel.Text = "Current Status: Online";
            }
            else if (pSteamUser.personastate == 2)
            {
                pSteamUser.personastatestring = "Busy";
                steamUserStatusLabel.Text = "Current Status: Busy";
            }
            else if (pSteamUser.personastate == 3)
            {
                pSteamUser.personastatestring = "Away";
                steamUserStatusLabel.Text = "Current Status: Away";
            }
            else if (pSteamUser.personastate == 4)
            {
                pSteamUser.personastatestring = "Snooze";
                steamUserStatusLabel.Text = "Current Status: Snooze";
            }
            else if (pSteamUser.personastate == 5)
            {
                pSteamUser.personastatestring = "Looking to trade";
                steamUserStatusLabel.Text = "Current Status: Looking to trade";
            }
            else if (pSteamUser.personastate == 6)
            {
                pSteamUser.personastatestring = "Looking to play";
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

            steamSimilarAccountAvatar.ImageLocation = sortedBannedPlayers[0].avatarfull;
            steamSimilarAccountNameLabel.Text = "Steam Name: " + sortedBannedPlayers[0].personaname;
            steamSimilarAccountUrlLabel.Text = "Profile Url: " + sortedBannedPlayers[0].profileurl;

            if (sortedBannedPlayers[0].personastate == 0)
            {
                sortedBannedPlayers[0].personastatestring = "Offline";
                steamSimilarAccountStatusLabel.Text = "Current Status: Offline";
            }
            else if (sortedBannedPlayers[0].personastate == 1)
            {
                sortedBannedPlayers[0].personastatestring = "Online";
                steamSimilarAccountStatusLabel.Text = "Current Status: Online";
            }
            else if (sortedBannedPlayers[0].personastate == 2)
            {
                sortedBannedPlayers[0].personastatestring = "Busy";
                steamSimilarAccountStatusLabel.Text = "Current Status: Busy";
            }
            else if (sortedBannedPlayers[0].personastate == 3)
            {
                sortedBannedPlayers[0].personastatestring = "Away";
                steamSimilarAccountStatusLabel.Text = "Current Status: Away";
            }
            else if (sortedBannedPlayers[0].personastate == 4)
            {
                sortedBannedPlayers[0].personastatestring = "Snooze";
                steamSimilarAccountStatusLabel.Text = "Current Status: Snooze";
            }
            else if (sortedBannedPlayers[0].personastate == 5)
            {
                sortedBannedPlayers[0].personastatestring = "Looking to trade";
                steamSimilarAccountStatusLabel.Text = "Current Status: Looking to trade";
            }
            else if (sortedBannedPlayers[0].personastate == 6)
            {
                sortedBannedPlayers[0].personastatestring = "Looking to play";
                steamSimilarAccountStatusLabel.Text = "Current Status: Looking to play";
            }

            if (sortedBannedPlayers[0].timecreated == 0)
            {
                steamSimilarAccountCreatedLabel.Text = "Account Created: Unknown";
            }
            else
            {
                steamSimilarAccountCreatedLabel.Text = "Account Created: " + UnixTimeToDateTime(sortedBannedPlayers[0].timecreated);
            }

            steamSimilarAccountClanLabel.Text = "Primary Clan ID: " + sortedBannedPlayers[0].primaryclanid;

            if (sortedBannedPlayers[0].loccountrycode == "" || sortedBannedPlayers[0].loccountrycode == null)
            {
                steamSimilarAccountCountryCodeLabel.Text = "Country Code: Unknown";
            }
            else
            {
                steamSimilarAccountCountryCodeLabel.Text = "Country Code: " + sortedBannedPlayers[0].loccountrycode;
            }

            similarAccountBanCountLabel.Text = "Number of bans: " + (sortedBannedPlayers[0].NumberOfGameBans + sortedBannedPlayers[0].NumberOfVACBans);
            similarAccountDaysSinceLastBanLabel.Text = "Days since last ban: " + sortedBannedPlayers[0].DaysSinceLastBan;

            steamNamesLabel.Text = "Steam names are " + Math.Round(sortedBannedPlayers[0].levDistancePersona * 100, 2) + "% similar";

            if(sortedBannedPlayers[0].levDistanceUrl == -1)
            {
                steamUrlLabel.Text = "A user doesn't have a custom URL set so a comparison isn't possible";
                steamNameEffectLabel.Text = "+" + (Math.Round(sortedBannedPlayers[0].levDistancePersona * 100, 2)).ToString();
            }
            else
            {
                steamUrlLabel.Text = "Profile urls are " + Math.Round(sortedBannedPlayers[0].levDistanceUrl) * 100 + "% similar";
                steamNameEffectLabel.Text = "+" + Math.Round((sortedBannedPlayers[0].levDistancePersona + sortedBannedPlayers[0].levDistanceUrl) * 100).ToString();
            }

            if(pSteamUser.personastatestring == sortedBannedPlayers[0].personastatestring)
            {
                timeCreatedLabel.Text = "Both users are" + pSteamUser.personastatestring + "at the same time";
                steamStatusEffectLabel.Text = "-10";
            }
            else
            {
                timeCreatedLabel.Text = "User is " + pSteamUser.personastatestring + " while similar account is " + sortedBannedPlayers[0].personastatestring;
                steamStatusEffectLabel.Text = "+10";
            }

            if (sortedBannedPlayers[0].createdAfter == true)
            {
                timeCreatedLabel.Text = "Similar account was created after user account";
                steamTimeCreatedEffectLabel.Text = "+5";
            }
            else
            {
                timeCreatedLabel.Text = "User account was created before similar account";
                steamTimeCreatedEffectLabel.Text = "-5";
            }

            if(pSteamUser.primaryclanid == sortedBannedPlayers[0].primaryclanid)
            {
                clanIdLabel.Text = "Primary clans are the same";
                steamPrimaryClanEffectLabel.Text = "+5";
            }    
            else
            {
                clanIdLabel.Text = "Primary clans are not the same";
                steamPrimaryClanEffectLabel.Text = "-5";
            }

            if (pSteamUser.loccountrycode == sortedBannedPlayers[0].loccountrycode)
            {
                countryCodeLabel.Text = "Both accounts share same country code";
                steamCountryCodeEffectLabel.Text = "+5";
            }
            else
            {
                countryCodeLabel.Text = "Both accounts don't share same country code";
                steamCountryCodeEffectLabel.Text = "-5";
            }

            similarityScoreLabel.Text = "Overall Similarity Score: " + Math.Round(sortedBannedPlayers[0].similarityscore, 2);

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
