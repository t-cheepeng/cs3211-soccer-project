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
        public const int NUM_ZONES_X = 5;

        public const int NUM_ZONES_Y = 3;

        // Check if a given team is in possession of the ball
        public static bool isTeamInPossessionOfBall(int team, int possession)
        {
            return team == possession;
        }

        /// Checks if a zone can act.
        /// A zone can act if there are players in the zone and time is not over.
        public static bool canZoneAct(int numOfPlayersInZone, int time)
        {
            return numOfPlayersInZone > 0 && time > 0;
        }

        /// Checks if a particular team on the zone can act.
        /// The team on the zone can act if it can perform any action.
        public static bool
        canTeamZoneAct(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            return canZoneTeamShoot(team,
            zoneX,
            zoneY,
            teamInPossession,
            ballPosX,
            ballPosY,
            numOfTeam0PlayersInZone,
            numOfTeam1PlayersInZone) ||
            canZoneTeamDribble(team,
            zoneX,
            zoneY,
            teamInPossession,
            ballPosX,
            ballPosY,
            numOfTeam0PlayersInZone,
            numOfTeam1PlayersInZone) ||
            canZoneTeamPass(team,
            zoneX,
            zoneY,
            teamInPossession,
            ballPosX,
            ballPosY,
            numOfTeam0PlayersInZone,
            numOfTeam1PlayersInZone) ||
            canZoneTeamRun(team,
            zoneX,
            zoneY,
            teamInPossession,
            ballPosX,
            ballPosY,
            numOfTeam0PlayersInZone,
            numOfTeam1PlayersInZone);
        }

        public static bool
        doesTeamHaveBallInZone(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY
        )
        {
            return teamInPossession == team &&
            ballPosX == zoneX &&
            ballPosY == zoneY;
        }

        public static bool
        doesTeamHavePlayersInZone(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (team == 0)
            {
                return numOfTeam0PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] > 0;
            }
            else
            {
                return numOfTeam1PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] > 0;
            }
        }

        public static bool
        canZoneTeamShoot(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !(
                doesTeamHavePlayersInZone(team,
                zoneX,
                zoneY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone) &&
                doesTeamHaveBallInZone(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY)
                )
            )
            {
                return false;
            }

            return true;
        }

        public static bool
        canZoneTeamDribble(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !(
                doesTeamHavePlayersInZone(team,
                zoneX,
                zoneY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone) &&
                doesTeamHaveBallInZone(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY)
                )
            )
            {
                return false;
            }

            // Starting zone. The starting zone requires a goalkeeper (i.e. a single player in the starting zone cannot dribble, must pass/shoot)
            if (
                team == 0 &&
                zoneX == 0 &&
                zoneY == 1 &&
                numOfTeam0PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 1
            )
            {
                return false;
            }
            if (
                team == 1 &&
                zoneX == NUM_ZONES_X - 1 &&
                zoneY == 1 &&
                numOfTeam1PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 1
            )
            {
                return false;
            }

            return true;
        }

        public static bool
        canDribbleToZone(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone,
            int toZoneX,
            int toZoneY
        )
        {
            // Terminal zone. Cannot dribble past the last index of zone.
            if (
                toZoneX < 0 ||
                toZoneX >= NUM_ZONES_X ||
                toZoneY < 0 ||
                toZoneY >= NUM_ZONES_Y
            )
            {
                return false;
            }

            return true;
        }

        public static bool
        canZoneTeamPass(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !(
                doesTeamHavePlayersInZone(team,
                zoneX,
                zoneY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone) &&
                doesTeamHaveBallInZone(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY)
                )
            )
            {
                return false;
            }

            // Check if there is any teammate in any other zone
            for (int i = 0; i < NUM_ZONES_X; i++)
            {
                for (int j = 0; j < NUM_ZONES_Y; j++)
                if (
                    canPassToZone(team,
                    zoneX,
                    zoneY,
                    numOfTeam0PlayersInZone,
                    numOfTeam1PlayersInZone,
                    i,
                    j)
                )
                {
                    return true;
                }
            }

            return false;
        }

        public static bool
        canPassToZone(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone,
            int toZoneX,
            int toZoneY
        )
        {
            if (zoneX == toZoneX && zoneY == toZoneY)
            {
                return false;
            }

            if (team == 0)
            {
                return numOfTeam0PlayersInZone[toZoneX * NUM_ZONES_Y +
                toZoneY] >
                0;
            }
            else
            {
                return numOfTeam1PlayersInZone[toZoneX * NUM_ZONES_Y +
                toZoneY] >
                0;
            }
        }

        public static bool
        canZoneTeamRun(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            // No player teammate in zone, cannot run
            if (
                !doesTeamHavePlayersInZone(team,
                zoneX,
                zoneY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return false;
            }

            bool moreThanOne =
                team == 0
                    ? numOfTeam0PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] > 1
                    : numOfTeam1PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] > 1;

            // More than one teammate in the zone, can run
            if (moreThanOne)
            {
                return true;
            }

            // Single teammate and has ball, cannot run
            if (
                doesTeamHaveBallInZone(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY)
            )
            {
                return false;
            }

            // Starting zone. The starting zone requires a goalkeeper
            // Single teammate at starting zone and has no ball, cannot run
            if (team == 0 && zoneX == 0 && zoneY == 1)
            {
                return false;
            }
            if (team == 1 && zoneX == NUM_ZONES_X - 1 && zoneY == 1)
            {
                return false;
            }

            // Single teammante not at starting zone and has no ball, can run
            return true;
        }

        public static bool
        canRunToZone(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone,
            int toZoneX,
            int toZoneY
        )
        {
            // Terminal zone. Cannot run past the last index of zone.
            if (
                toZoneX < 0 ||
                toZoneX >= NUM_ZONES_X ||
                toZoneY < 0 ||
                toZoneY >= NUM_ZONES_Y
            )
            {
                return false;
            }

            return true;
        }

        public static int
        shootActionRate(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !canZoneTeamShoot(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        public static int
        dribbleActionRate(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !canZoneTeamDribble(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        public static int
        passActionRate(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !canZoneTeamPass(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY,
                numOfTeam0PlayersInZone,
                numOfTeam1PlayersInZone)
            )
            {
                return 0;
            }
            return 1;
        }

        public static int
        runActionRate(
            int team,
            int zoneX,
            int zoneY,
            int teamInPossession,
            int ballPosX,
            int ballPosY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (
                !canZoneTeamRun(team,
                zoneX,
                zoneY,
                teamInPossession,
                ballPosX,
                ballPosY,
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
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (team == 0)
            {
                // No opponent means no chance of failure
                if (numOfTeam1PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 0)
                {
                    return 0;
                }
                return 1; // Calculations here
            }
            else
            {
                // No opponent means no chance of failure
                if (numOfTeam0PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 0)
                {
                    return 0;
                }
                return 1; // Calculations here
            }
        }

        public static int passSuccessRate()
        {
            return 1;
        }

        public static int
        passFailRate(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            if (team == 0)
            {
                // No opponent means no chance of failure
                if (numOfTeam1PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 0)
                {
                    return 0;
                }
                return 1; // Calculations here
            }
            else
            {
                // No opponent means no chance of failure
                if (numOfTeam0PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 0)
                {
                    return 0;
                }
                return 1; // Calculations here
            }
        }
    }
}
