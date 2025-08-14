using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class DungeonGeneratorV2 : Node
{
    // Unique Rooms
    [Export]
    public PackedScene Stairset2x3x2 { get; set; }
    [Export]
    public int Inspector_Number_Of_Stairset2x3x2;
    [Export]
    public PackedScene TurnAround2x3 { get; set; }
    [Export]
    public int Inspector_Number_Of_TurnAround2x3;
    [Export]
    public PackedScene FishSpawnRoom1x2 { get; set; }
    [Export]
    public int Inspector_Number_Of_FishSpawnRoom1x2;

    // End caps 1x1
    [Export]
    public PackedScene Toilet1x1 { get; set; }
    [Export]
    public int Inspector_Number_Of_Toilet1x1;
    [Export]
    public PackedScene Endcap1x1 { get; set; }

    // Junction Rooms
    [Export]
    public PackedScene Junction_1x1 { get; set; }
    [Export]
    public PackedScene TJunction_1x1 { get; set; }

    // Corridor Rooms
    [Export]
    public PackedScene TurnLeft_1x1 { get; set; }
    [Export]
    public PackedScene TurnRight_1x1 { get; set; }
    [Export]
    public PackedScene End_1x1 { get; set; }
    [Export]
    public PackedScene Hall_1x1 { get; set; }

    // Large Corridor Rooms
    [Export]
    public PackedScene TurnLeft_3x3 { get; set; }
    [Export]
    public PackedScene TurnRight_3x3 { get; set; }
    [Export]
    public PackedScene Hall_3x3 { get; set; }

    // storing important room counters
    [Export]
    public int DungeonAttempts;
    public int NumberOfStairset2x3x2;
    public int NumberOfTurnaround2x3;
    public int NumberOfFishSpawnRoom1x2;
    public int NumberOfToilet1x1;

    // Storing Instuctions
    private struct SpawnInstruction
    {
        public string RoomName;
        public int Depth;
        public int Column;
        public string Direction;
        public SpawnInstruction(string prefab, int row, int col, string direction)
        {
            RoomName = prefab;
            Depth = row;
            Column = col;
            Direction = direction;
        }
    }
    private List<SpawnInstruction> SpawnQueue = new List<SpawnInstruction>();
    // spawnQueue.Add(new SpawnInstruction("TurnLeft", 4, 2, 2));

    // Dungeon creator
    private string[,] GroundFloorMap;
    [Export]
    public int DungeonWidth;
    [Export]
    public int DungeonDepth;

    public int OpenDoors = 0;
    // generate 2 random corridors
    // generate 1 random junction
    // generate 2 random corridors
    // generate 1 unique room
    // loop until done
    // connect all loose ends with small corridors
    // if fail, make a new map
    // if succeed, give spawning queue to all people connected
    // execute spawn queue

    public bool DEBUG = false;
    public bool DEBUG1 = true;
    public int record = 10;



    public override void _Ready()
    {
        set_up_for_dungeon_making();
        if (create_good_dungeon())
        {
            // remember to send off the SpawnQueue to everyone
            generate_dungeon_in_scene(SpawnQueue);
        }
        else
        {
            if (DEBUG == true) GD.Print("dungeon failed completely");
        }
    }

    private void set_up_for_dungeon_making()
    {
        GroundFloorMap = new string[DungeonDepth, DungeonWidth];
        // set all nulls to be a space character
        for (int i = 0; i < DungeonDepth; i++)
        {
            for (int j = 0; j < DungeonWidth; j++)
            {
                GroundFloorMap[i, j] = " ";
            }
        }
        int middle = DungeonWidth / 2;
        GroundFloorMap[0, middle] = "v";
        if (DEBUG == true) GD.Print("middle in column: " + middle + " and depth: 0");
        SpawnQueue.Clear();

        // reset all numbers of rooms here
        OpenDoors = 0;
        NumberOfStairset2x3x2 = Inspector_Number_Of_Stairset2x3x2;
        NumberOfTurnaround2x3 = Inspector_Number_Of_TurnAround2x3;
        NumberOfFishSpawnRoom1x2 = Inspector_Number_Of_FishSpawnRoom1x2;
        NumberOfToilet1x1 = Inspector_Number_Of_Toilet1x1;

        //if (DEBUG == true) print_ground_floor_map();
    }
    private bool create_good_dungeon()
    {
        if (DEBUG == true) GD.Print("Good dungeon has been created");
        for (int i = 0; i < DungeonAttempts; i++)
        {
            if (create_dungeon())
            {
                if (DEBUG == true) GD.Print("Good dungeon has been created");
                if (DEBUG1 == true) print_ground_floor_map();
                if (DEBUG1 == true) GD.Print("FOUND THE CORRECT ONE AT: " + i);
                if (DEBUG1 == true) GD.Print("We need to fill: "+OpenDoors);
                return true;
            }
            else
            {
                if (DEBUG == true) print_ground_floor_map();
                if (DEBUG == true) GD.Print("dungeon failed at pass: " + NumberOfStairset2x3x2 + ", trying again");
                if (DEBUG1 == true)
                {
                    if (record > NumberOfStairset2x3x2)
                    {
                        GD.Print("Failed at: " + i + " At Stage: " + NumberOfStairset2x3x2);
                        record = NumberOfStairset2x3x2;
                    }
                }
                set_up_for_dungeon_making();
                if (DEBUG == true) GD.Print(" === NEW ATTEMPT ===");
                //if (DEBUG == true) print_ground_floor_map();
            }
        }
        return false;
    }

    private void generate_dungeon_in_scene(List<SpawnInstruction> spawnQueue)
    {
        foreach (var instruction in spawnQueue)
        {
            if (instruction.RoomName == "stairset")
            {
                Node3D stairset2x3x2 = Stairset2x3x2.Instantiate<Node3D>();
                spawnRoom(stairset2x3x2, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "junction1x1")
            {
                Node3D junction1x1 = Junction_1x1.Instantiate<Node3D>();
                spawnRoom(junction1x1, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "hall1x1")
            {
                Node3D hall1x1 = Hall_1x1.Instantiate<Node3D>();
                spawnRoom(hall1x1, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "right1x1")
            {
                Node3D right1x1 = TurnRight_1x1.Instantiate<Node3D>();
                spawnRoom(right1x1, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "left1x1")
            {
                Node3D left1x1 = TurnLeft_1x1.Instantiate<Node3D>();
                spawnRoom(left1x1, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "t_junction_1x1")
            {
                Node3D tjunction1x1 = TJunction_1x1.Instantiate<Node3D>();
                spawnRoom(tjunction1x1, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "cross_junction_1x1")
            {
                Node3D crossjunction1x1 = Junction_1x1.Instantiate<Node3D>();
                spawnRoom(crossjunction1x1, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "stairset")
            {
                Node3D stairset = Stairset2x3x2.Instantiate<Node3D>();
                spawnRoom(stairset, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "turnaround")
            {
                Node3D turnaround = TurnAround2x3.Instantiate<Node3D>();
                spawnRoom(turnaround, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "fishspawmroom")
            {
                Node3D fishspawnroom = FishSpawnRoom1x2.Instantiate<Node3D>();
                spawnRoom(fishspawnroom, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "endcap")
            {
                Node3D endcap = Endcap1x1.Instantiate<Node3D>();
                spawnRoom(endcap, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else if (instruction.RoomName == "toilet")
            {
                Node3D toilet = Toilet1x1.Instantiate<Node3D>();
                spawnRoom(toilet, instruction.Depth, instruction.Column, instruction.Direction);
            }
            else
            {
                if (DEBUG == true) GD.Print("Unknown room: " + instruction.RoomName);
            }
        }
    }

    private void spawnRoom(Node3D room, int depth, int col, string direction)
    {
        Vector3 positionToSpawn = new Vector3();
        Vector3 wayToRotate = new Vector3(0, 0, 0);
        int middle = DungeonWidth / 2;
        if (direction == "down")
        {
            positionToSpawn = new Vector3((((col - middle) * 10)), 0, ((depth * 10)));
        }
        if (direction == "left")
        {
            positionToSpawn = new Vector3((((col - middle) * 10) + 5), 0, ((depth * 10) + 5));
            wayToRotate = new Vector3(0, -90, 0);
        }
        if (direction == "right")
        {
            positionToSpawn = new Vector3((((col - middle) * 10) - 5), 0, ((depth * 10) + 5));
            wayToRotate = new Vector3(0, 90, 0);
        }
        if (direction == "up")
        {
            positionToSpawn = new Vector3((((col - middle) * 10)), 0, ((depth * 10) + 10));
            wayToRotate = new Vector3(0, 180, 0);
        }
        room.Position = positionToSpawn;
        room.RotationDegrees = wayToRotate;
        GetTree().Root.CallDeferred("add_child", room);
    }

    // CORE
    private bool create_dungeon()
    {
        // making basic layout
        // 2 corridoors, 1 junction, 2 corridoors, 1 unique room
        // keep going till all unique rooms are added
        while ((NumberOfStairset2x3x2 + NumberOfTurnaround2x3 + NumberOfFishSpawnRoom1x2) > 0)
        {
            if (!try_to_add_a_corridoor()) return false;
            if (!try_to_add_a_corridoor()) return false;
            if (!try_to_add_a_corridoor()) return false;
            if (!try_to_add_a_junction()) return false;
            if (!try_to_add_a_corridoor()) return false;
            if (!try_to_add_a_corridoor()) return false;
            if (!try_to_add_a_unique_room()) return false;
            if (DEBUG == true) GD.Print("[HERE] One Loop Successful! only " + NumberOfStairset2x3x2 + " Left!");
        }
        // ok dungeon has been made, lets add all the end caps
        while (!(OpenDoors == 0))
        {
            if (!try_to_add_end_caps()) return false;
        }
        if (DEBUG == true) GD.Print("End Caps Placed Successfully");




        //SpawnQueue.Add(new SpawnInstruction("stairset", 0, 3, "up"));

        // create dungeon
        //   generate 2 corridors, 1 junction, 2 corridors, 1 unique room
        //   check how many unique rooms left, if more than 0, loop
        // if 0
        // connect loose ends
        // if fail to connect loose ends, fail, restart
        // if suceed, return true
        return true;
    }

    private bool try_to_add_a_corridoor()
    {
        (bool successfulFinding, string direction, int foundRow, int foundCol) = find_open_door_in_every_direction();
        if (successfulFinding)
        {
            if (DEBUG == true) GD.Print("open door found at " + foundRow + " " + foundCol + " in direction: " + direction);
            // so we get the open direction and coords
            // shuffle all available corridoords
            // if one succeeds, then stop
            // if one fails, go to next one in shuffled order
            // if all fail, then fail
            Func<bool>[] conditions = new Func<bool>[]
            {
                // Add the hall place scripts here
                () => tryToPlace_Hall1x1x1(foundRow,foundCol,direction),
                () => tryToPlace_Right1x1x1(foundRow,foundCol,direction),
                () => tryToPlace_Left1x1x1(foundRow,foundCol,direction)
            };

            Random rng = new Random();
            for (int i = conditions.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = conditions[i];
                conditions[i] = conditions[j];
                conditions[j] = temp;
            }
            foreach (var condition in conditions)
            {
                if (condition())
                    return true;
            }

            return false;
        }
        return false;
    }

    private bool try_to_add_a_junction()
    {
        (bool successfulFinding, string direction, int foundRow, int foundCol) = find_open_door_in_every_direction();
        if (successfulFinding)
        {
            if (DEBUG == true) GD.Print("open door found at " + foundRow + " " + foundCol + " in direction: " + direction);
            // so we get the open direction and coords
            // shuffle all available corridoords
            // if one succeeds, then stop
            // if one fails, go to next one in shuffled order
            // if all fail, then fail
            Func<bool>[] conditions = new Func<bool>[]
            {
                // Add the junction scripts
                () => tryToPlace_T_Junction1x1x1(foundRow,foundCol,direction),
                () => tryToPlace_Junction1x1x1(foundRow,foundCol,direction)
            };

            Random rng = new Random();
            for (int i = conditions.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = conditions[i];
                conditions[i] = conditions[j];
                conditions[j] = temp;
            }
            foreach (var condition in conditions)
            {
                if (condition())
                    return true;
            }

            // last ditch effort
            //if (try_to_add_a_corridoor()) return true;

            return false;
        }
        return false;
    }

    private bool try_to_add_a_unique_room()
    {
        (bool successfulFinding, string direction, int foundRow, int foundCol) = find_open_door_in_every_direction();
        if (successfulFinding)
        {
            if (DEBUG == true) GD.Print("open door found at " + foundRow + " " + foundCol + " in direction: " + direction);
            Func<bool>[] conditions = new Func<bool>[]
            {
                // Add the unique rooms here
                () => tryToPlace_Stairs2x3x2(foundRow,foundCol,direction),
                () => tryToPlace_TurnAround2x3(foundRow,foundCol,direction),
                () => tryToPlace_FishSpawnRoom1x2(foundRow,foundCol,direction)
            };

            Random rng = new Random();
            for (int i = conditions.Length - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = conditions[i];
                conditions[i] = conditions[j];
                conditions[j] = temp;
            }
            foreach (var condition in conditions)
            {
                if (condition())
                    return true;
            }

            // last ditch effort
            //if (try_to_add_a_corridoor()) return true;

            return false;
        }
        return false;
    }
    private bool try_to_add_end_caps()
    {
        (bool successfulFinding, string direction, int foundRow, int foundCol) = find_open_door_in_every_direction();
        if (successfulFinding)
        {
            if (DEBUG == true) GD.Print("open door found at " + foundRow + " " + foundCol + " in direction: " + direction);
            Func<bool>[] conditions = new Func<bool>[]
            {
                // Add the unique rooms here
                () => tryToPlace_Toilet1x1(foundRow,foundCol,direction),
                () => tryToPlace_End1x1(foundRow,foundCol,direction),
            };
            foreach (var condition in conditions)
            {
                if (condition())
                    return true;
            }

            // last ditch effort
            //if (try_to_add_a_corridoor()) return true;

            return false;
        }
        return false;
    }

    private (bool, string, int, int) find_open_door_in_every_direction()
    {
        bool foundLocation;
        int foundRow;
        int foundCol;
        (foundLocation, foundRow, foundCol) = find_open_door_in_direction("v");
        if (foundLocation == true) return (true, "down", foundRow, foundCol);
        (foundLocation, foundRow, foundCol) = find_open_door_in_direction("<");
        if (foundLocation == true) return (true, "left", foundRow, foundCol);
        (foundLocation, foundRow, foundCol) = find_open_door_in_direction(">");
        if (foundLocation == true) return (true, "right", foundRow, foundCol);
        (foundLocation, foundRow, foundCol) = find_open_door_in_direction("A");
        if (foundLocation == true) return (true, "up", foundRow, foundCol);
        else
        {
            if (DEBUG == true) GD.Print("Failed to find any open doors");
            return (false, "no direction", 0, 0);
        }
    }

    private (bool, int, int) find_open_door_in_direction(string direction)
    {
        int foundRow = -1, foundCol = -1;

        for (int i = 0; i < DungeonDepth; i++)
        {
            for (int j = 0; j < DungeonWidth; j++)
            {
                if (GroundFloorMap[i, j] == direction)
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
            if (DEBUG == true) GD.Print("No cell with value " + direction + " found.");
            return (false, 0, 0);
        }
        if (DEBUG == true) GD.Print("Found a " + direction + " potential");
        return (true, foundRow, foundCol);
    }

    private bool tryToPlace_Hall1x1x1(int Row, int Col, string Direction)
    {
        if (Direction == "down")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row + 1, Col] = "v";
        }
        if (Direction == "left")
        {
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            GroundFloorMap[Row, Col - 1] = "<";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "right")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col + 1] = ">";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "up")
        {
            if (Row - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row + 1, Col] = "A";
            GroundFloorMap[Row, Col] = "X";
        }
        SpawnQueue.Add(new SpawnInstruction("hall1x1", Row, Col, Direction));
        return true;
    }
    private bool tryToPlace_Right1x1x1(int Row, int Col, string Direction)
    {
        if (Direction == "down")
        {
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col - 1] = "<";
        }
        if (Direction == "left")
        {
            if (Row - 1 < 0) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            GroundFloorMap[Row - 1, Col] = "A";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "right")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "up")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col + 1] = ">";
            GroundFloorMap[Row, Col] = "X";
        }
        SpawnQueue.Add(new SpawnInstruction("right1x1", Row, Col, Direction));
        return true;
    }

    private bool tryToPlace_Left1x1x1(int Row, int Col, string Direction)
    {
        if (Direction == "down")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col + 1] = ">";
        }
        if (Direction == "left")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row + 1, Col] = "v";
        }
        if (Direction == "right")
        {
            if (Row - 1 < 0) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row - 1, Col] = "A";
        }
        if (Direction == "up")
        {
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col - 1] = "<";
        }
        SpawnQueue.Add(new SpawnInstruction("left1x1", Row, Col, Direction));
        return true;
    }
    private bool tryToPlace_T_Junction1x1x1(int Row, int Col, string Direction)
    {
        if (Row <= 2) return false; // depth check
        if (Direction == "down")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col - 1] = "<";
            GroundFloorMap[Row, Col + 1] = ">";
        }
        if (Direction == "left")
        {
            if (Row - 1 < 0) return false;
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row - 1, Col] = "A";
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "right")
        {
            if (Row - 1 < 0) return false;
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row - 1, Col] = "A";
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "up")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col - 1] = "<";
            GroundFloorMap[Row, Col + 1] = ">";
        }
        SpawnQueue.Add(new SpawnInstruction("t_junction_1x1", Row, Col, Direction));
        OpenDoors++;
        return true;
    }

    private bool tryToPlace_Junction1x1x1(int Row, int Col, string Direction)
    {
        if (Row <= 2) return false; // depth check
        if (Direction == "down")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Col - 1 < 0) return false;
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col - 1] = "<";
            GroundFloorMap[Row, Col + 1] = ">";
        }
        if (Direction == "left")
        {
            if (Row - 1 < 0) return false;
            if (Col - 1 < 0) return false;
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col - 1] = "<";
            GroundFloorMap[Row - 1, Col] = "A";
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "right")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Row - 1 < 0) return false;
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col + 1] = ">";
            GroundFloorMap[Row - 1, Col] = "A";
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "up")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (Col - 1 < 0) return false;
            if (Row - 1 < 0) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row + 1, Col] = "v";
            GroundFloorMap[Row, Col - 1] = "<";
            GroundFloorMap[Row, Col + 1] = ">";
        }
        SpawnQueue.Add(new SpawnInstruction("cross_junction_1x1", Row, Col, Direction));
        OpenDoors++;
        OpenDoors++;
        return true;
    }
    // UNIQUE ROOMS
    private bool tryToPlace_Stairs2x3x2(int Row, int Col, string Direction)
    {
        if (NumberOfStairset2x3x2 == 0) return false;
        if (Direction == "down")
        {
            if (Row + 3 >= DungeonDepth) return false;
            if (Col + 1 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 2, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 3, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 2, Col] == " ")) return false;

            NumberOfStairset2x3x2--;

            GroundFloorMap[Row, Col + 1] = "X";
            GroundFloorMap[Row + 1, Col + 1] = "X";
            GroundFloorMap[Row + 2, Col + 1] = "X";
            GroundFloorMap[Row + 3, Col + 1] = "v";
            GroundFloorMap[Row + 1, Col] = "X";
            GroundFloorMap[Row + 2, Col] = "X";

            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "left")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (Col - 3 < 0) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col - 2] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col - 3] == " ")) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col - 2] == " ")) return false;

            NumberOfStairset2x3x2--;

            GroundFloorMap[Row + 1, Col] = "X";
            GroundFloorMap[Row + 1, Col - 1] = "X";
            GroundFloorMap[Row + 1, Col - 2] = "X";
            GroundFloorMap[Row + 1, Col - 3] = "<";
            GroundFloorMap[Row, Col - 1] = "X";
            GroundFloorMap[Row, Col - 2] = "X";

            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "right")
        {
            if (Row - 1 < 0) return false;
            if (Col + 3 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col + 2] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col + 3] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 2] == " ")) return false;

            NumberOfStairset2x3x2--;

            GroundFloorMap[Row - 1, Col] = "X";
            GroundFloorMap[Row - 1, Col + 1] = "X";
            GroundFloorMap[Row - 1, Col + 2] = "X";
            GroundFloorMap[Row - 1, Col + 3] = ">";
            GroundFloorMap[Row, Col + 1] = "X";
            GroundFloorMap[Row, Col + 2] = "X";

            GroundFloorMap[Row, Col] = "X";
        }
        if (Direction == "up")
        {
            if (Row - 3 < 0) return false;
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 2, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 3, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row - 2, Col] == " ")) return false;

            NumberOfStairset2x3x2--;

            GroundFloorMap[Row, Col - 1] = "X";
            GroundFloorMap[Row - 1, Col - 1] = "X";
            GroundFloorMap[Row - 2, Col - 1] = "X";
            GroundFloorMap[Row - 3, Col - 1] = "A";
            GroundFloorMap[Row - 1, Col] = "X";
            GroundFloorMap[Row - 2, Col] = "X";

            GroundFloorMap[Row, Col] = "X";
        }
        SpawnQueue.Add(new SpawnInstruction("stairset", Row, Col, Direction));
        return true;
    }
    private bool tryToPlace_TurnAround2x3(int Row, int Col, string Direction)
    {
        if (NumberOfTurnaround2x3 == 0) return false;
        if (Direction == "down")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (Row - 1 < 0) return false;
            if (Col + 2 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col + 2] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col + 2] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col + 2] == " ")) return false;
            NumberOfTurnaround2x3--;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col + 1] = "X";
            GroundFloorMap[Row, Col + 2] = "X";
            GroundFloorMap[Row + 1, Col] = "X";
            GroundFloorMap[Row + 1, Col + 1] = "X";
            GroundFloorMap[Row + 1, Col + 2] = "X";
            GroundFloorMap[Row - 1, Col + 2] = "A";
        }
        if (Direction == "left")
        {
            if (Col - 1 < 0) return false;
            if (Col + 1 >= DungeonWidth) return false;
            if (Row + 2 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 2, Col] == " ")) return false;
            if (!(GroundFloorMap[Row + 2, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row + 2, Col + 1] == " ")) return false;
            NumberOfTurnaround2x3--;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col - 1] = "X";
            GroundFloorMap[Row + 1, Col] = "X";
            GroundFloorMap[Row + 1, Col - 1] = "X";
            GroundFloorMap[Row + 2, Col] = "X";
            GroundFloorMap[Row + 2, Col - 1] = "X";
            GroundFloorMap[Row + 2, Col + 1] = ">";

        }
        if (Direction == "right")
        {
            if (Col - 1 < 0) return false;
            if (Col + 1 >= DungeonWidth) return false;
            if (Row - 2 < 0) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 2, Col] == " ")) return false;
            if (!(GroundFloorMap[Row - 2, Col + 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 2, Col - 1] == " ")) return false;
            NumberOfTurnaround2x3--;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col + 1] = "X";
            GroundFloorMap[Row - 1, Col] = "X";
            GroundFloorMap[Row - 1, Col + 1] = "X";
            GroundFloorMap[Row - 2, Col] = "X";
            GroundFloorMap[Row - 2, Col + 1] = "X";
            GroundFloorMap[Row - 2, Col - 1] = "<";
        }
        if (Direction == "up")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (Row - 1 < 0) return false;
            if (Col - 2 < 0) return false;
            if (!(GroundFloorMap[Row, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row, Col - 2] == " ")) return false;
            if (!(GroundFloorMap[Row + 1, Col - 2] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col - 1] == " ")) return false;
            if (!(GroundFloorMap[Row - 1, Col - 2] == " ")) return false;
            NumberOfTurnaround2x3--;
            GroundFloorMap[Row, Col] = "X";
            GroundFloorMap[Row, Col - 1] = "X";
            GroundFloorMap[Row, Col - 2] = "X";
            GroundFloorMap[Row + 1, Col - 2] = "v";
            GroundFloorMap[Row - 1, Col] = "X";
            GroundFloorMap[Row - 1, Col - 1] = "X";
            GroundFloorMap[Row - 1, Col - 2] = "X";
        }
        SpawnQueue.Add(new SpawnInstruction("turnaround", Row, Col, Direction));
        return true;
    }
    private bool tryToPlace_FishSpawnRoom1x2(int Row, int Col, string Direction)
    {
        if (DEBUG == true) GD.Print("Fish Spawn Room");
        if (NumberOfFishSpawnRoom1x2 == 0) return false;
        if (Row <= 10) return false; // depth check
        if (Direction == "down")
        {
            if (Col + 1 >= DungeonWidth) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "S";
            GroundFloorMap[Row, Col + 1] = "S";
            NumberOfFishSpawnRoom1x2--;
        }
        if (Direction == "left")
        {
            if (Row + 1 >= DungeonDepth) return false;
            if (!(GroundFloorMap[Row + 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col] = "S";
            GroundFloorMap[Row + 1, Col] = "S";
            NumberOfFishSpawnRoom1x2--;
        }
        if (Direction == "right")
        {
            if (Row - 1 < 0) return false;
            if (!(GroundFloorMap[Row - 1, Col] == " ")) return false;
            GroundFloorMap[Row, Col] = "S";
            GroundFloorMap[Row - 1, Col] = "S";
            NumberOfFishSpawnRoom1x2--;
        }
        if (Direction == "up")
        {
            if (Col - 1 < 0) return false;
            if (!(GroundFloorMap[Row, Col + 1] == " ")) return false;
            GroundFloorMap[Row, Col] = "S";
            GroundFloorMap[Row, Col - 1] = "S";
            NumberOfFishSpawnRoom1x2--;
        }
        SpawnQueue.Add(new SpawnInstruction("fishspawmroom", Row, Col, Direction));
        return true;
    }

    // end caps
    private bool tryToPlace_End1x1(int Row, int Col, string Direction)
    {
        if (Direction == "down")
        {
            GroundFloorMap[Row, Col] = "E";
        }
        if (Direction == "left")
        {
            GroundFloorMap[Row, Col] = "E";
        }
        if (Direction == "right")
        {
            GroundFloorMap[Row, Col] = "E";
        }
        if (Direction == "up")
        {
            GroundFloorMap[Row, Col] = "E";
        }
        SpawnQueue.Add(new SpawnInstruction("endcap", Row, Col, Direction));
        OpenDoors--;
        return true;
    }
    private bool tryToPlace_Toilet1x1(int Row, int Col, string Direction)
    {
        if (NumberOfToilet1x1 == 0) return false;
        if (Row <= 7) return false;
        if (Direction == "down")
        {
            GroundFloorMap[Row, Col] = "T";
        }
        if (Direction == "left")
        {
            GroundFloorMap[Row, Col] = "T";
        }
        if (Direction == "right")
        {
            GroundFloorMap[Row, Col] = "T";
        }
        if (Direction == "up")
        {
            GroundFloorMap[Row, Col] = "T";
        }
        NumberOfToilet1x1--;
        OpenDoors--;
        SpawnQueue.Add(new SpawnInstruction("toilet", Row, Col, Direction));
        return true;
    }



















    // Debug help
    private void print_ground_floor_map()
    {
        for (int i = 0; i < DungeonDepth; i++)
        {
            string rowStr = "";
            for (int j = 0; j < DungeonWidth; j++)
            {
                rowStr += GroundFloorMap[i, j] + " ";
            }
            GD.Print(rowStr);  // Print the entire row at once
        }
    }

}
