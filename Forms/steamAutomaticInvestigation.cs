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

                if (bannedPlayers[i].timecreated > pSteamUser.timecreated)
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
            }
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
    }
}
