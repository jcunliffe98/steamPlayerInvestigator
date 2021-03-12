using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steamPlayerInvestigator
{
    public class Player
    {
        public string steamid { get; set; }
        public int mutualFriendCount { get; set; }
        public double mutualPercent { get; set; }
        public string personastatestring { get; set; }
        public double similarityscore { get; set; }
        public int communityvisibilitystate { get; set; }
        public int profilestate { get; set; }
        public string personaname { get; set; }
        public string profileurl { get; set; }
        public string avatar { get; set; }
        public string avatarmedium { get; set; }
        public string avatarfull { get; set; }
        public string avatarhash { get; set; }
        public long lastlogoff { get; set; }
        public int personastate { get; set; }
        public bool onlineAtSameTime { get; set; }
        public string primaryclanid { get; set; }
        public long timecreated { get; set; }
        public bool createdAfter { get; set; }
        public int personastateflags { get; set; }
        public string loccountrycode { get; set; }
        public bool sameCountry { get; set; }
        public bool CommunityBanned { get; set; }
        public bool VACBanned { get; set; }
        public int NumberOfVACBans { get; set; }
        public int DaysSinceLastBan { get; set; }
        public int NumberOfGameBans { get; set; }
        public string EconomyBan { get; set; }
        public string relationship { get; set; }
        public int friend_since { get; set; }
        public double levDistancePersona { get; set; }
        public double levDistanceUrl { get; set; }
        public FriendsRoot friendsOfFriends { get; set; }
    }
}
