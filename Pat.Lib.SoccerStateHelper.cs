using System;
using System.Collections.Generic;
using System.Text;
//the namespace must be PAT.Lib, the class and method names can be arbitrary
namespace PAT.Lib
{
    /// <summary>
    /// The math library that can be used in your model.
    /// all methods should be declared as public static.
    /// 
    /// The parameters must be of type "int", or "int array"
    /// The number of parameters can be 0 or many
    /// 
    /// The return type can be bool, int or int[] only.
    /// 
    /// The method name will be used directly in your model.
    /// e.g. call(max, 10, 2), call(dominate, 3, 2), call(amax, [1,3,5]),
    /// 
    /// Note: method names are case sensetive
    /// </summary>
    public class SoccerStateHelper
    {
       public static bool isTeamInPossessionOfBall(int team, int possession)
        {
	        return team == possession;
        }
        
        public static bool canZoneAct(int numOfPlayersInZone, int time)
        {
        	return numOfPlayersInZone > 0 && time > 0;
        }
        
        public static bool doesTeamHaveBallInZone(int team, int zone, int teamInPossession, int ballInZone)
        {
        	return teamInPossession == team && ballInZone == zone;
        }
        
        public static bool doesTeamHavePlayersInZone(int team, int zone, int[] numOfTeam0PlayersInZone, int[] numOfTeam1PlayersInZone)
        {
        	if (team == 0)
        	{
        		return numOfTeam0PlayersInZone[zone] > 0;
        	}
        	else
        	{
        		return numOfTeam1PlayersInZone[zone] > 0;
        	}
        }
        
        public static bool canZoneTeamRun(int zone, int team, int teamInPossession, int ballInZone, int[] numOfTeam0PlayersInZone, int[] numOfTeam1PlayersInZone)
        {
			// Terminal zone. team 0 running to the right cannot run past the last index of zone. similar for team 1  
        	if (zone == 1 && team == 0) { return false; }
        	if (zone == 0 && team == 1) { return false; }
        	bool moreThanOne = true;
        	if (team == 0)
        	{
        		moreThanOne = numOfTeam0PlayersInZone[zone] > 1;	
        	}
        	else
        	{
        		moreThanOne = numOfTeam1PlayersInZone[zone] > 1;	
        	}
        	return 
        		(SoccerStateHelper.doesTeamHavePlayersInZone(team, zone, numOfTeam0PlayersInZone, numOfTeam1PlayersInZone) 
        		&& !SoccerStateHelper.doesTeamHaveBallInZone(team, zone, teamInPossession, ballInZone))
        		|| moreThanOne;
        }
    }
}
