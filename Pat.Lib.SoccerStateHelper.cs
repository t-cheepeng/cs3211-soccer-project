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

        public static bool
        canTeamZoneAct(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !doesTeamHavePlayersInZone(team,
                zone,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return false;
            }

            int totalRate =
                shootActionRate(team, zone, teamInPossession, ballInZone) +
                runActionRate(team,
                zone,
                teamInPossession,
                ballInZone,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone) +
                dribbleActionRate(team,
                zone,
                teamInPossession,
                ballInZone,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone);

            return totalRate > 0; // Total rate cannot be 0
        }

        public static bool
        doesTeamHaveBallInZone(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone
        )
        {
            return teamInPossession == team && ballInZone == zone;
        }

        public static bool
        doesTeamHavePlayersInZone(
            int team,
            int zone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
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

        // Guarenteed to have at least one team player in zone
        public static bool
        canZoneTeamRun(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            // Terminal zone. Team 0 running to the right cannot run past the last index of zone. similar for team 1
            if (team == 0 && zone == 1)
            {
                return false;
            }
            if (team == 1 && zone == 0)
            {
                return false;
            }

            bool moreThanOne =
                team == 0
                    ? numOfTeam0PlayersInZone[zone] > 1
                    : numOfTeam1PlayersInZone[zone] > 1;

            // More than one player case
            // If there are more than one player in the zone, the zone can run
            if (moreThanOne)
            {
                return true;
            }

            // One player case
            // Starting zone. The starting zone requires a goalkeeper
            if (team == 0 && zone == 0)
            {
                return false;
            }
            if (team == 1 && zone == 1)
            {
                return false;
            }
            return !SoccerStateHelper
                .doesTeamHaveBallInZone(team,
                zone,
                teamInPossession,
                ballInZone);
        }

        // Guarenteed to have at least one team player in zone
        public static bool
        canZoneTeamDribble(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            // Terminal zone. Team 0 dribbling to the right cannot dribble past the last index of zone. similar for team 1
            if (team == 0 && zone == 1)
            {
                return false;
            }
            if (team == 1 && zone == 0)
            {
                return false;
            }

            // Starting zone. The starting zone requires a goalkeeper
            if (team == 0 && zone == 0 && numOfTeam0PlayersInZone[zone] == 1)
            {
                return false;
            }
            if (team == 1 && zone == 1 && numOfTeam1PlayersInZone[zone] == 1)
            {
                return false;
            }

            return true;
        }

        // Guarenteed to have at least one team player in zone
        public static int
        shootActionRate(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone
        )
        {
            if (
                !SoccerStateHelper
                    .doesTeamHaveBallInZone(team,
                    zone,
                    teamInPossession,
                    ballInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        // Guarenteed to have at least one team player in zone
        public static int
        dribbleActionRate(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !SoccerStateHelper
                    .doesTeamHaveBallInZone(team,
                    zone,
                    teamInPossession,
                    ballInZone)
            )
            {
                return 0;
            }
            if (
                !canZoneTeamDribble(team,
                zone,
                teamInPossession,
                ballInZone,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        // Guarenteed to have at least one team player in zone
        public static int
        passActionRate(int team, int zone, int teamInPossession, int ballInZone)
        {
            if (
                !SoccerStateHelper
                    .doesTeamHaveBallInZone(team,
                    zone,
                    teamInPossession,
                    ballInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        // Guarenteed to have at least one team player in zone
        public static int
        runActionRate(
            int team,
            int zone,
            int teamInPossession,
            int ballInZone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !canZoneTeamRun(team,
                zone,
                teamInPossession,
                ballInZone,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        public static int shootSuccessRate()
        {
            return 1;
        }

        public static int shootFailRate()
        {
            return 1;
        }

        public static int dribbleSuccessRate()
        {
            return 1;
        }

        public static int
        dribbleFailRate(
            int team,
            int zone,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (team == 0)
            {
                // No opponent means no chance of failure
                if (numOfTeam1PlayersInZone[zone] == 0)
                {
                    return 0;
                }
                return 1; // Calculations here
            }
            else
            {
                // No opponent means no chance of failure
                if (numOfTeam0PlayersInZone[zone] == 0)
                {
                    return 0;
                }
                return 1; // Calculations here
            }
        }
    }
}
