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

		// ========== Variables for shoot success rate computation/caching ==========
		
		// 7 vs. 7 Player Field: 55m * 36.5m 
	
		// expected goals per shot
		// expectedgoals(distance, a) = exp(−distance/a)
		
        public const double FOOTSHOT_ALPHA = 7.1;
        public const double X_LEN = 55;
        public const double Y_LEN = 36.5;
		
        public const double UNIT_CELL_X_LEN = 55 / 5;
        public const double UNIT_CELL_Y_LEN = 36.5 / 3;
        public static double[] TEAM_0_GOALPOS = initTeamGoalPos(0);
        public static double[] TEAM_1_GOALPOS = initTeamGoalPos(1);
		
        public static double[,,] goalDistanceCache = initGoalDistanceCache(NUM_ZONES_X, NUM_ZONES_Y);
        public static Dictionary<double, double> expectedGoalCache = new Dictionary<double, double>();

		// ========== Methods for shoot success rate computation/caching ==========

		public static double[,,] initGoalDistanceCache(int NUM_ZONES_X, int NUM_ZONES_Y) {
			//Console.WriteLine("initGoalDistanceCache called");
			double[,,] toReturn = new double[2, NUM_ZONES_X, NUM_ZONES_Y];
			for (int i = 0; i < 2; i++) {
				for (int j = 0; j < NUM_ZONES_X; j++) {
					for (int k = 0; k < NUM_ZONES_Y; k++) {
						toReturn[i,j,k] = -1.0;
					}
				}
			}
			return toReturn;
		}
		
		public static double getExpectedGoal(double distance, double alpha) {
			double value = -1;
			if (expectedGoalCache.TryGetValue(distance, out value)) {
				return value;
			} else {
				value = Math.Exp((-1.0 * distance) / alpha);
				expectedGoalCache.Add(distance, value);
				return value;
			}
		}
		
		public static double[] getCenterCoordinates(int zoneX, int zoneY) {
			double X_lowBound = (double)zoneX * UNIT_CELL_X_LEN;
			double X_hiBound = (double)(zoneX+1) * UNIT_CELL_X_LEN;
			
			double Y_lowBound = (double)zoneY * UNIT_CELL_Y_LEN;
			double Y_hiBound = (double)(zoneY+1) * UNIT_CELL_Y_LEN;
			
			double[] toReturn = new double[2] {(X_hiBound + X_lowBound) / 2, (Y_hiBound + Y_lowBound) / 2};
			return toReturn;
		}
		
		public static double getDistance(double x1, double y1, double x2, double y2) {
			return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
		}
		
		public static double[] initTeamGoalPos(int teamNumber) {
			//Console.WriteLine("initTeamGoalPos called");
			double[] goalPos = new double[2];
			goalPos[1] = Y_LEN / 2;
			goalPos[0] = teamNumber == 0 ? 0 : X_LEN;
			return goalPos;
		}
		
		public static double getDistanceToOpponentGoal(int zoneX, int zoneY, int teamInPosession) {
			double cacheVal = goalDistanceCache[teamInPosession, zoneX, zoneY];
			if (cacheVal != -1.0) {
				return cacheVal;
			}
			double[] targetGoalPos = teamInPosession == 0 ? TEAM_1_GOALPOS : TEAM_0_GOALPOS;		
			double[] shooterCoordinates = getCenterCoordinates(zoneX, zoneY);
			cacheVal = getDistance(shooterCoordinates[0], shooterCoordinates[1], targetGoalPos[0], targetGoalPos[1]);
			goalDistanceCache[teamInPosession, zoneX, zoneY] = cacheVal;
			return cacheVal;
		}

		// ========== Methods for Zone related computation ==========

        // Check if a given team is in possession of the ball
        public static bool isTeamInPossessionOfBall(int team, int possession)
        {
            return team == possession;
        }

        /// Checks if a zone can act.
        /// A zone can act if there are players in the zone and if the zone is currently only 1 cell away from the current ball position
        public static bool canZoneAct(int numOfPlayersInZone, int zoneX, int zoneY, int ballPosX, int ballPosY)
        {
            return numOfPlayersInZone > 0 && 
                Math.Abs(zoneX - ballPosX) < 2 &&
                Math.Abs(zoneY - ballPosY) < 2;
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

        public static bool
        disallowShootingBeforeHalfLine(
            int teamInPossesionOfBall,
            int ballPosX,
            int ballPosY
        )
        {
            int mid = NUM_ZONES_X / 2;
            if (teamInPossesionOfBall == 0) {
                return ballPosX >= mid;
            } else {
                return ballPosX <= mid;
            }
        }

        public static int[]
        findNearestPlayerToTakeBall(
            int teamInPossessionOfBallBeforeFail,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            // Just do a linear search because cannot be bothered with more complex algorithms
            if (teamInPossessionOfBallBeforeFail == 0) 
            {
                for (int i = NUM_ZONES_X - 1; i >= 0; i--) 
                {
                    for (int j = 0; j < NUM_ZONES_Y; j++)
                    {
                        if (numOfTeam1PlayersInZone[i * NUM_ZONES_Y + j] > 0)
                        {
                            int[] result = {i, j};
                            return result;
                        }
                    }
                }
            } else {
                for (int i = 0; i < NUM_ZONES_X; i++) 
                {
                    for (int j = 0; j < NUM_ZONES_Y; j++)
                    {
                        if (numOfTeam0PlayersInZone[i * NUM_ZONES_Y + j] > 0)
                        {
                            int[] result = {i, j};
                            return result;
                        }
                    }
                }
            }
            int[] areYouSerious = {69, 69};
            return areYouSerious;
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

        public static int shootSuccessRate(int zoneX, int zoneY, int teamInPosession)
        {
            return (int) (getExpectedGoal(getDistanceToOpponentGoal(zoneX, zoneY, teamInPosession), FOOTSHOT_ALPHA) * 10000);
        }

        public static int shootFailRate(int zoneX, int zoneY, int teamInPosession)
        {
            return (int) ((1 - getExpectedGoal(getDistanceToOpponentGoal(zoneX, zoneY, teamInPosession), FOOTSHOT_ALPHA)) * 10000);
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
