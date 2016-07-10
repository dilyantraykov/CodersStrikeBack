using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        var myPod = new Pod();
        var checkPoints = new List<Point>();

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            int nextCheckpointX = int.Parse(inputs[2]); // x position of the next check point
            int nextCheckpointY = int.Parse(inputs[3]); // y position of the next check point
            int nextCheckpointDist = int.Parse(inputs[4]); // distance to the next checkpoint
            int nextCheckpointAngle = int.Parse(inputs[5]); // angle between your pod orientation and the direction of the next checkpoint
            inputs = Console.ReadLine().Split(' ');
            int opponentX = int.Parse(inputs[0]);
            int opponentY = int.Parse(inputs[1]);

            var myPodPosition = new Point(x, y);
            var opponentPodPosition = new Point(opponentX, opponentY);
            var checkPoint = new Point(nextCheckpointX, nextCheckpointY);

            if (!checkPoints.Contains(checkPoint))
            {
                checkPoints.Add(checkPoint);
            }

            var gameContext = new GameContext(
                                    myPodPosition,
                                    opponentPodPosition,
                                    checkPoint,
                                    nextCheckpointDist,
                                    nextCheckpointAngle,
                                    checkPoints);

            myPod.ProcessTurn(gameContext);
        }
    }
}

public static class Constants
{
    public const int MaxHeight = 9000;
    public const int MaxWidth = 16000;

    public const int CheckPointRadius = 600;

    public const int PodForceFieldRadius = 400;

    public const int BoostsPerRace = 1;
}

public class Pod
{
    public Pod()
    {
        this.BoostsLeft = Constants.BoostsPerRace;
    }

    public Point Point { get; set; }
    public int BoostsLeft { get; set; }

    public void ProcessTurn(GameContext gameContext)
    {
        this.Point = gameContext.MyPodPosition;

        Point tangentPoint1;
        Point tangentPoint2;

        var canFindTangentPoint = Geometry.FindTangents(gameContext.NextCheckpoint, 100, this.Point, out tangentPoint1, out tangentPoint2);
        var targetPoint = gameContext.NextCheckpoint;

        if (canFindTangentPoint)
        {
            var distance1 = Geometry.CalculateDistance(this.Point, tangentPoint1);
            var distance2 = Geometry.CalculateDistance(this.Point, tangentPoint2);

            targetPoint = tangentPoint1;
        }

        if (ShouldBoost(gameContext))
        {
            this.Boost(gameContext.NextCheckpoint);
            return;
        }

        var thrust = CalculateThrust(gameContext);

        this.Move(gameContext.NextCheckpoint, thrust);
    }

    public int CalculateThrust(GameContext gameContext)
    {
        var angle = Math.Abs(gameContext.NextCheckpointAngle);
        
        if (angle > 120)
        {
            return 20;
        }
        else if (angle > 90)
        {
            return 40;
        }
        else if (angle > 45)
        {
            return 60;
        }
        else if (angle > 20)
        {
            return 80;
        }
        else
        {
            return 100;
        }

    }
    
    private bool ShouldBoost(GameContext gameContext)
    {
        return gameContext.NextCheckpointDistance > Constants.CheckPointRadius * 8 &&
            Math.Abs(gameContext.NextCheckpointAngle) < 10 &&
            this.BoostsLeft > 0;
    }

    public void Move(Point point, int thrust)
    {
        Console.WriteLine($"{point.X} {point.Y} {thrust}");
    }

    public void Boost(Point point)
    {
        this.BoostsLeft -= 1;
        Console.WriteLine($"{point.X} {point.Y} BOOST");
    }
}

public class GameContext
{
    public GameContext(Point myPodPosition, Point opponentPodPosition, Point nextCheckpoint, int nextCheckpointDistance, int nextCheckpointAngle, List<Point> checkpoints)
    {
        this.MyPodPosition = myPodPosition;
        this.OpponentPodPosition = opponentPodPosition;
        this.NextCheckpoint = nextCheckpoint;
        this.NextCheckpointDistance = nextCheckpointDistance;
        this.NextCheckpointAngle = nextCheckpointAngle;
        this.Checkpoints = checkpoints;
    }

    public Point MyPodPosition { get; set; }
    public Point OpponentPodPosition { get; set; }
    public Point NextCheckpoint { get; set; }
    public int NextCheckpointDistance { get; set; }
    public int NextCheckpointAngle { get; set; }
    public List<Point> Checkpoints { get; set; }
}

public class Point
{
    public Point(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }
}

public static class Geometry
{
    public static bool FindTangents(Point center, float radius,
    Point external_point, out Point pt1, out Point pt2)
    {
        // Find the distance squared from the
        // external point to the circle's center.
        double dx = center.X - external_point.X;
        double dy = center.Y - external_point.Y;
        double D_squared = dx * dx + dy * dy;
        if (D_squared < radius * radius)
        {
            pt1 = null;
            pt2 = null;
            return false;
        }

        // Find the distance from the external point
        // to the tangent points.
        double L = Math.Sqrt(D_squared - radius * radius);

        // Find the points of intersection between
        // the original circle and the circle with
        // center external_point and radius dist.
        FindCircleCircleIntersections(
            center.X, center.Y, radius,
            external_point.X, external_point.Y, (float)L,
            out pt1, out pt2);

        return true;
    }

    // Find the points where the two circles intersect.
    private static int FindCircleCircleIntersections(
        float cx0, float cy0, float radius0,
        float cx1, float cy1, float radius1,
        out Point intersection1, out Point intersection2)
    {
        // Find the distance between the centers.
        float dx = cx0 - cx1;
        float dy = cy0 - cy1;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            intersection1 = null;
            intersection2 = null;
            return 0;
        }
        else if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            intersection1 = null;
            intersection2 = null;
            return 0;
        }
        else if ((dist == 0) && (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            intersection1 = null;
            intersection2 = null;
            return 0;
        }
        else
        {
            // Find a and h.
            double a = (radius0 * radius0 -
                radius1 * radius1 + dist * dist) / (2 * dist);
            double h = Math.Sqrt(radius0 * radius0 - a * a);

            // Find P2.
            double cx2 = cx0 + a * (cx1 - cx0) / dist;
            double cy2 = cy0 + a * (cy1 - cy0) / dist;

            // Get the points P3.
            intersection1 = new Point(
                (int)(cx2 + h * (cy1 - cy0) / dist),
                (int)(cy2 - h * (cx1 - cx0) / dist));
            intersection2 = new Point(
                (int)(cx2 - h * (cy1 - cy0) / dist),
                (int)(cy2 + h * (cx1 - cx0) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1) return 1;
            return 2;
        }
    }

    public static int GetLegsOfRightTriangle(int hypothenuse)
    {
        var result = (int)Math.Sqrt((hypothenuse * hypothenuse) / 2);

        return result;
    }

    public static int GetHypothenuseOfRightTriangle(int legLenght)
    {
        var result = (int)Math.Sqrt((legLenght * legLenght) * 2);

        return result;
    }

    public static bool IsSamePoint(Point p1, Point p2)
    {
        return p1.X == p2.X && p1.Y == p2.Y;
    }

    public static double CalculateDistance(Point p1, Point p2)
    {
        return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
    }

    public static Point GetPointAlongLine(Point p2, Point p1, double distance)
    {
        Point vector = new Point(p2.X - p1.X, p2.Y - p1.Y);
        double c = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        double a = distance / c;

        var newX = (int)(p1.X + vector.X * a);
        var newY = (int)(p1.Y + vector.Y * a);

        return new Point(newX, newY);
    }
}