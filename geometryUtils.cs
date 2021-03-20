using System;
using System.Collections.Generic;

public class geometryUtils {
    
    // 7 vs. 7 Player Field: 55m * 36.5m 
	
	// expected goals per shot
	// expectedgoals(distance, a) = exp(−distance/a)
	
    
	// ========== UTILITY CLASS DONT USE TIS THING DIRECTLY WITH PAT ==========
	
    public const int NUM_ZONES_X = 5;
    public const int NUM_ZONES_Y = 3;
	public const double FOOTSHOT_ALPHA = 7.1;
    public const double X_LEN = 55;
    public const double Y_LEN = 36.5;
    public const double UNIT_CELL_X_LEN = 55 / 5;
    public const double UNIT_CELL_Y_LEN = 36.5 / 3;
    public static double[] TEAM_0_GOALPOS = initTeamGoalPos(0);
    public static double[] TEAM_1_GOALPOS = initTeamGoalPos(1);
	public static double[,,] goalDistanceCache = initGoalDistanceCache(NUM_ZONES_X, NUM_ZONES_Y);
	public static Dictionary<double, double> expectedGoalCache = new Dictionary<double, double>();
    
	public static double[,,] initGoalDistanceCache(int NUM_ZONES_X, int NUM_ZONES_Y) {
		Console.WriteLine("initGoalDistanceCache called");
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
		double x = -1;
		if (expectedGoalCache.TryGetValue(distance, out x)) {
			return x;
		} else {
			x = Math.Exp((-1.0 * distance) / alpha);
			expectedGoalCache.Add(distance, x);
			return x;
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
		Console.WriteLine("initTeamGoalPos called");
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
    
    public static void Main() {
        Console.WriteLine("This is a Utility Class");
    }
    
}