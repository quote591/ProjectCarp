using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public enum Direction
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}

public struct PositionDirection
{
    public Vector3 Position;
    public Direction Dir;

    public PositionDirection(Vector3 position, Direction dir)
    {
        Position = position;
        Dir = dir;
    }
}

public partial class DungeonGenerator : Node
{
    [Export]
    public PackedScene Junction_1x1x1 { get; set; }
    [Export]
    public int NumberOfJunction_1x1x1;
    private bool junction1x1x1recent = false;

    [Export]
    public PackedScene End_1x1x1 { get; set; }
    [Export]
    public int NumberOfEnd_1x1x1;
    private bool end1x1x1recent = false;

    [Export]
    public PackedScene Hall_1x3x1 { get; set; }
    [Export]
    public int NumberOfHall_1x3x1;
    private bool hall1x3x1recent = false;

    [Export]
    public PackedScene Stairset2x3x2 { get; set; }
    [Export]
    public int NumberOfStairset2x3x2;
    private bool stairset2x3x2recent = false;

    [Export]
    public PackedScene Hall_1x1x1 { get; set; }
    [Export]
    public int NumberOfHall_1x1x1;
    private bool hall1x1x1recent = false;


    //Vector3[] emptyAgents;

    //private PositionDirection[] agentsToFinish = Array.Empty<PositionDirection>();

    private List<PositionDirection> agentsToFinish = new();

    ///private List<List<int>> groundFloorMap = new();

    private int[,] groundFloorMap;
    [Export]
    public int DungeonWidth;
    [Export]
    public int DungeonLength;


    public override void _Ready()
    {
        //Node3D junction1x1x1 = Junction_1x1x1.Instantiate<Node3D>();
        //Node3D end1x1x1 = End_1x1x1.Instantiate<Node3D>();
        Node3D hall1x3x1 = Hall_1x3x1.Instantiate<Node3D>();
        //Node3D stairset2x3x2 = Stairset2x3x2.Instantiate<Node3D>();
        // adding base stuff
        GetTree().Root.CallDeferred("add_child", hall1x3x1);
        //junction1x1x1.Position = new Vector3(0, 0, 30);
        //GetTree().Root.CallDeferred("add_child", junction1x1x1);

        //emptyAgents.Append((5, 0, 35));
        //agentsToFinish.Append(new PositionDirection(new Vector3(5, 0, 35), Direction.East)).ToArray();
        // agentsToFinish.Add(new PositionDirection(new Vector3(0, 0, 30), Direction.North));
        // //GD.Print(agentsToFinish.Count());

        // while (agentsToFinish.Count() > 0)
        // {
        //     var agent = agentsToFinish[0];
        //     GD.Print(agent.Position, agent.Dir);
        //     spawnAgent(agent);
        //     agentsToFinish.RemoveAt(0);
        // }

        // start agent cycle 
        //spawnAgent(new PositionDirection(new Vector3(0, 0, 30), Direction.North));


        create_2d_map(DungeonWidth, DungeonLength);

        //potentialfill2();
        //potentialFill4();
        //potentialFill3();
        //potentialfill2();
        //potentialFill4();
        //potentialFill3();
        //potentialfill2();
        //potentialfill2();

        //addRandomRoomInDirection(2);
        //addRandomRoomInDirection(3);
        //addRandomRoomInDirection(3);
        //addRandomRoomInDirection(2);
        //addRandomRoomInDirection(2);
        //addRandomRoomInDirection(2);


        for (int i = 0; i < 1000; i++)
        {
            var rng = new RandomNumberGenerator();
            int number = rng.RandiRange(2, 4);
            //GD.Print("Random Direction: " + number);
            addRandomRoomInDirection(number);
        }
        GD.Print("Dunegon Finished");
        printMap();
    }

    private void create_2d_map(int width, int length)
    {
        GD.Print("Width: " + width + " Length: " + length);
        //groundFloorMap
        groundFloorMap = new int[length, width];

        // make start location
        int middle = width / 2;
        groundFloorMap[1, middle] = 2;
        groundFloorMap[0, middle] = 1;

        printMap();
    }

    // 1 is taken
    // 2 is room potential Downwards
    // 3 is room potental Left
    // 4 is room potential Right
    // 5 is room potential Upwards
    private void addRandomRoomInDirection(int Direction)
    {
        int foundRow = -1, foundCol = -1;

        for (int i = 0; i < DungeonLength; i++)
        {
            for (int j = 0; j < DungeonWidth; j++)
            {
                if (groundFloorMap[i, j] == Direction)
                {
                    foundRow = i;
                    foundCol = j;
                    break;
                }
            }
            if (foundRow != -1) break;
        }
        if (foundRow == -1)
        {
            //GD.Print("No cell with value " + Direction + " found.");
            return; // nothing to do
        }
        //GD.Print("Found a " + Direction + " potential");

        //if (NumberOfJunction_1x1x1 != 0) tryToPlace_Junction1x1x1(foundRow, foundCol, Direction);
        //if (NumberOfStairset2x3x2 != 0) tryToPlace_Stairs2x3x2(foundRow, foundCol, Direction);
        //if (NumberOfJunction_1x1x1 != 0) tryToPlace_Junction1x1x1(foundRow, foundCol, Direction);
        //if (NumberOfHall_1x3x1 != 0) tryToPlace_Hall1x3x1(foundRow, foundCol, Direction);

        var rng2 = new RandomNumberGenerator();
        int number2 = rng2.RandiRange(1, 4);
        switch (number2)
        {
            case 1:
                //GD.Print("1x1x1");
                if ((NumberOfJunction_1x1x1 != 0) && !junction1x1x1recent) tryToPlace_Junction1x1x1(foundRow, foundCol, Direction);
                break;
            case 2:
                if ((NumberOfStairset2x3x2 != 0) && !stairset2x3x2recent) tryToPlace_Stairs2x3x2(foundRow, foundCol, Direction);
                break;
            case 3:
                //GD.Print("1x3x1");
                if ((NumberOfHall_1x3x1 != 0) && !hall1x3x1recent)tryToPlace_Hall1x3x1(foundRow, foundCol, Direction);
                break;
            case 4:
                //GD.Print("1x3x1");
                if ((NumberOfHall_1x1x1 != 0) && !hall1x1x1recent)tryToPlace_Hall1x1x1(foundRow, foundCol, Direction);
                break;
            default:
                break;
        }

    }

    private bool tryToPlace_Stairs2x3x2(int Row, int Col, int Direction)
    {
        //GD.Print("Trying to place at " + Row + " " + Col);
        //GD.Print("Trying to add Stairset in Direction "+Direction);
        if (Direction == 2)
        {
            //GD.Print("2x3x2 Direction 2");
            if (Row + 3 >= DungeonLength) return false;
            if (Col + 1 >= DungeonWidth) return false;
            if (!(groundFloorMap[Row, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row + 2, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row + 3, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row + 2, Col] == 0)) return false;

            NumberOfStairset2x3x2--;

            groundFloorMap[Row, Col + 1] = 8;
            groundFloorMap[Row + 1, Col + 1] = 8;
            groundFloorMap[Row + 2, Col + 1] = 8;
            groundFloorMap[Row + 3, Col + 1] = 2;
            groundFloorMap[Row + 1, Col] = 8;
            groundFloorMap[Row + 2, Col] = 8;

            groundFloorMap[Row, Col] = 8;
        }
        if (Direction == 3)
        {
            //GD.Print("2x3x2 Direction 3");
            if (Row + 1 >= DungeonLength) return false;
            if (Col - 3 <= 0) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col - 1] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col - 2] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col - 3] == 0)) return false;
            if (!(groundFloorMap[Row, Col - 1] == 0)) return false;
            if (!(groundFloorMap[Row, Col - 2] == 0)) return false;

            NumberOfStairset2x3x2--;

            groundFloorMap[Row + 1, Col] = 8;
            groundFloorMap[Row + 1, Col - 1] = 8;
            groundFloorMap[Row + 1, Col - 2] = 8;
            groundFloorMap[Row + 1, Col - 3] = 3;
            groundFloorMap[Row, Col - 1] = 8;
            groundFloorMap[Row, Col - 2] = 8;

            groundFloorMap[Row, Col] = 8;
        }
        if (Direction == 4)
        {
            //GD.Print("2x3x2 Direction 4");
            if (Row - 1 <= 0) return false;
            if (Col + 3 >= DungeonWidth) return false;
            if (!(groundFloorMap[Row - 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row - 1, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row - 1, Col + 2] == 0)) return false;
            if (!(groundFloorMap[Row - 1, Col + 3] == 0)) return false;
            if (!(groundFloorMap[Row, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row, Col + 2] == 0)) return false;

            NumberOfStairset2x3x2--;

            groundFloorMap[Row - 1, Col] = 8;
            groundFloorMap[Row - 1, Col + 1] = 8;
            groundFloorMap[Row - 1, Col + 2] = 8;
            groundFloorMap[Row - 1, Col + 3] = 4;
            groundFloorMap[Row, Col + 1] = 8;
            groundFloorMap[Row, Col + 2] = 8;

            groundFloorMap[Row, Col] = 8;
        }

        stairset2x3x2recent = true;
        hall1x3x1recent = false;
        junction1x1x1recent = false;
        Node3D stairset2x3x2 = Stairset2x3x2.Instantiate<Node3D>();
        spawnRoom(stairset2x3x2, Row, Col, Direction);

        return true;
    }
    private bool tryToPlace_Hall1x3x1(int Row, int Col, int Direction)
    {
        //GD.Print("tryToPlace_Hall1x3x1");
        Node3D hall1x3x1;
        if (Direction == 2)
        {
            if (Row + 3 >= DungeonLength) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row + 2, Col] == 0)) return false;
            if (!(groundFloorMap[Row + 3, Col] == 0)) return false;

            NumberOfHall_1x3x1--;

            groundFloorMap[Row, Col] = 1;
            groundFloorMap[Row + 1, Col] = 1;
            groundFloorMap[Row + 2, Col] = 1;
            groundFloorMap[Row + 3, Col] = 2;
        }
        if (Direction == 3)
        {
            if (Col - 3 <= 0) return false;
            if (!(groundFloorMap[Row, Col - 1] == 0)) return false;
            if (!(groundFloorMap[Row, Col - 2] == 0)) return false;
            if (!(groundFloorMap[Row, Col - 3] == 0)) return false;

            NumberOfHall_1x3x1--;

            groundFloorMap[Row, Col] = 1;
            groundFloorMap[Row, Col - 1] = 1;
            groundFloorMap[Row, Col - 2] = 1;
            groundFloorMap[Row, Col - 3] = 3;
        }
        if (Direction == 4)
        {
            if (Col + 3 >= DungeonWidth) return false;
            if (!(groundFloorMap[Row, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row, Col + 2] == 0)) return false;
            if (!(groundFloorMap[Row, Col + 3] == 0)) return false;

            NumberOfHall_1x3x1--;

            groundFloorMap[Row, Col] = 1;
            groundFloorMap[Row, Col + 1] = 1;
            groundFloorMap[Row, Col + 2] = 1;
            groundFloorMap[Row, Col + 3] = 4;
        }

        hall1x3x1recent = true;
        hall1x3x1 = Hall_1x3x1.Instantiate<Node3D>();
        spawnRoom(hall1x3x1, Row, Col, Direction);

        return true;
    }

    private bool tryToPlace_Junction1x1x1(int Row, int Col, int Direction)
    {
        //GD.Print("tryToPlace_Junction1x1x1");
        Node3D junction1x1x1;

        if (Direction == 2)
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Col - 1 <= 0) return false;
            if (Row + 1 >= DungeonLength) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row, Col - 1] == 0)) return false;
            if (!(groundFloorMap[Row, Col + 1] == 0)) return false;
            NumberOfJunction_1x1x1--;
            groundFloorMap[Row, Col] = 1;
            groundFloorMap[Row + 1, Col] = 2;
            groundFloorMap[Row, Col - 1] = 3;
            groundFloorMap[Row, Col + 1] = 4;
        }
        if (Direction == 3)
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Col - 1 <= 0) return false;
            if (Row + 1 >= DungeonLength) return false;
            if (!(groundFloorMap[Row, Col - 1] == 0)) return false;
            if (!(groundFloorMap[Row - 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            NumberOfJunction_1x1x1--;
            groundFloorMap[Row, Col - 1] = 3;
            groundFloorMap[Row - 1, Col] = 5;
            groundFloorMap[Row + 1, Col] = 2;
            groundFloorMap[Row, Col] = 1;
        }
        if (Direction == 4)
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Row - 1 <= 0) return false;
            if (Row + 1 >= DungeonLength) return false;
            if (!(groundFloorMap[Row, Col + 1] == 0)) return false;
            if (!(groundFloorMap[Row - 1, Col] == 0)) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            NumberOfJunction_1x1x1--;
            groundFloorMap[Row, Col + 1] = 4;
            groundFloorMap[Row - 1, Col] = 5;
            groundFloorMap[Row + 1, Col] = 2;
            groundFloorMap[Row, Col] = 1;
        }

        junction1x1x1recent = true;
        stairset2x3x2recent = false;
        junction1x1x1 = Junction_1x1x1.Instantiate<Node3D>();
        spawnRoom(junction1x1x1, Row, Col, Direction);
        return true;
    }

    private bool tryToPlace_Hall1x1x1(int Row, int Col, int Direction)
    {
        //GD.Print("tryToPlace_Junction1x1x1");
        Node3D hall1x1x1;

        if (Direction == 2)
        {
            if (Row + 1 >= DungeonLength) return false;
            if (!(groundFloorMap[Row + 1, Col] == 0)) return false;
            NumberOfHall_1x1x1--;
            groundFloorMap[Row, Col] = 1;
            groundFloorMap[Row + 1, Col] = 2;
        }
        if (Direction == 3)
        {
            if (Col - 1 <= 0) return false;
            if (!(groundFloorMap[Row, Col - 1] == 0)) return false;
            NumberOfHall_1x1x1--;
            groundFloorMap[Row, Col - 1] = 3;
            groundFloorMap[Row, Col] = 1;
        }
        if (Direction == 4)
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (!(groundFloorMap[Row, Col + 1] == 0)) return false;
            NumberOfHall_1x1x1--;
            groundFloorMap[Row, Col + 1] = 4;
            groundFloorMap[Row, Col] = 1;
        }
        hall1x1x1recent = false; // always spawn
        stairset2x3x2recent = false;
        junction1x1x1recent = false;
        hall1x1x1 = Hall_1x1x1.Instantiate<Node3D>();
        spawnRoom(hall1x1x1, Row, Col, Direction);
        return true;
    }

    private void spawnRoom(Node3D room, int row, int col, int direction)
    {
        Vector3 positionToSpawn = new Vector3();
        Vector3 wayToRotate = new Vector3(0, 0, 0);
        int middle = DungeonWidth / 2;
        if (direction == 2)
        {
            positionToSpawn = new Vector3((((col - middle) * 10)), 0, ((row * 10) + 20));
        }
        if (direction == 3)
        {
            positionToSpawn = new Vector3((((col - middle) * 10) + 5), 0, ((row * 10) + 25));
            wayToRotate = new Vector3(0, -90, 0);
        }
        if (direction == 4)
        {
            positionToSpawn = new Vector3((((col - middle) * 10) - 5), 0, ((row * 10) + 25));
            wayToRotate = new Vector3(0, 90, 0);
        }
        room.Position = positionToSpawn;
        room.RotationDegrees = wayToRotate;
        GetTree().Root.CallDeferred("add_child", room);
        //printMap();
    }

    private void printMap()
    {
        for (int i = 0; i < DungeonLength; i++)
        {
            string rowStr = "";
            for (int j = 0; j < DungeonWidth; j++)
            {
                rowStr += groundFloorMap[i, j] + " ";
            }
            GD.Print(rowStr);  // Print the entire row at once
        }
    }
}
