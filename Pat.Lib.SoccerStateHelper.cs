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

        // Below numbers are calculated from statsbomb free soccer data
        // See compile_data.py for more details
        public const double SHOOT_ACTION_PERCENTAGE = 0.009617925746881376 * 10000;
        public const double DRIBBLE_RUN_ACTION_PERCENTAGE = 0.3080043896394667 * 10000;
        public const double PASS_ACTION_PERCENTAGE = 0.3743732949741852 * 10000;

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

		// ========== Variables for DRIBBLE success rate computation/caching ==========
		
		// 7 vs. 7 Player Field: 55m * 36.5m 	
        public const double INTERCEPT_SUCCESS_RATE = 0.2533349003937239;
        public const double DRIBBLE_SUCCESS_RATE = 1 - INTERCEPT_SUCCESS_RATE;
        public const double DRIBBLE_FAILURE_RATE = INTERCEPT_SUCCESS_RATE;

		// ========== Variables for PASS success rate computation/caching ==========
		
		// 7 vs. 7 Player Field: 55m * 36.5m
        // Probability obtained from StatsBomb. Explanation is provided in report.
        public const double SHORT_PASS_SUCCESS_RATE = 0.75;
        public const double LONG_PASS_SUCCESS_RATE = 0.428571429;
        public const double CROSS_PASS_SUCCESS_RATE = 0.342857143;

		// ========== Methods for PASS success rate computation/caching ==========
		public static double getExpectedPass(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone,
            int toZoneX,
            int toZoneY) {
            
            bool isOpponentInTargetZone = (team==0 && numOfTeam1PlayersInZone[toZoneX * NUM_ZONES_Y + toZoneY] > 0) ||
                                          (team==1 && numOfTeam0PlayersInZone[toZoneX * NUM_ZONES_Y + toZoneY] > 0);
            double passTypeRate = getPassTypeRate(team, zoneX, zoneY, toZoneX, toZoneY);
            if (isOpponentInTargetZone) {
                return passTypeRate * (1-INTERCEPT_SUCCESS_RATE);
            } else {
                return passTypeRate;
            }
		}

		public static double getPassTypeRate(int team, int zoneX, int zoneY, int toZoneX, int toZoneY) {
			// Cross Pass from flank to in front of goal (4,1) or (0,1)
            if ((team==0 && zoneX==4 && toZoneX==4 && toZoneY==1) ||
                (team==1 && zoneX==0 && toZoneX==0 && toZoneY==1)) {
                return CROSS_PASS_SUCCESS_RATE;
            // Short Pass to any adjacent cell (side or diagonally in front)
            } else if (Math.Abs(zoneX-toZoneX) <= 1 && 
                       Math.Abs(zoneY - toZoneY) <= 1) {
                return SHORT_PASS_SUCCESS_RATE;
            // Long Pass
            } else {
                return LONG_PASS_SUCCESS_RATE;
            }
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
            
            float mid = NUM_ZONES_X / 2;
            if (teamInPossession == 0)
            {
                return zoneX > mid;
            } else {
                return zoneX < mid;
            }
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

            // Check if there is any teammate in front/at the side
            if (team == 0) 
            {
                for (int i = zoneX; i < NUM_ZONES_X; i++)
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
            } else 
            {
                for (int i = 0; i <= zoneX; i++)
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
            return (int) SHOOT_ACTION_PERCENTAGE;
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
            return (int) DRIBBLE_RUN_ACTION_PERCENTAGE;
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
            return (int) PASS_ACTION_PERCENTAGE;
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
            return (int) DRIBBLE_RUN_ACTION_PERCENTAGE;
        }

        public static int shootSuccessRate(int zoneX, int zoneY, int teamInPosession)
        {
            return (int) (getExpectedGoal(getDistanceToOpponentGoal(zoneX, zoneY, teamInPosession), FOOTSHOT_ALPHA) * 10000);
        }

        public static int shootFailRate(int zoneX, int zoneY, int teamInPosession)
        {
            return (int) ((1 - getExpectedGoal(getDistanceToOpponentGoal(zoneX, zoneY, teamInPosession), FOOTSHOT_ALPHA)) * 10000);
        }


        public static int 
        dribbleSuccessRate(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone
        )
        {
            return 10000-dribbleFailRate(team,zoneX,zoneY,numOfTeam0PlayersInZone,numOfTeam1PlayersInZone);
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
                return (int) (DRIBBLE_FAILURE_RATE * 10000);
            }
            else
            {
                // No opponent means no chance of failure
                if (numOfTeam0PlayersInZone[zoneX * NUM_ZONES_Y + zoneY] == 0)
                {
                    return 0;
                }
                return (int) (DRIBBLE_FAILURE_RATE * 10000);
            }
        }

        public static int 
        passSuccessRate(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone,
            int toZoneX,
            int toZoneY
        )
        {
            return (int) (getExpectedPass(team, zoneX, zoneY, numOfTeam0PlayersInZone, numOfTeam1PlayersInZone, toZoneX, toZoneY) * 10000);
        }

        public static int
        passFailRate(
            int team,
            int zoneX,
            int zoneY,
            int[] numOfTeam0PlayersInZone,
            int[] numOfTeam1PlayersInZone,
            int toZoneX,
            int toZoneY
        )
        {
            return (int) ((1-getExpectedPass(team, zoneX, zoneY, numOfTeam0PlayersInZone, numOfTeam1PlayersInZone, toZoneX, toZoneY)) * 10000);
        }
    }
}
