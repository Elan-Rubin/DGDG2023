using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    enum GridSpace { Empty, Floor, Wall };
    GridSpace[,] grid;
    int roomHeight, roomWidth;
    int roomHeightReal, roomWidthReal;
    //float gutterRatio;
    //[SerializeField] private int gutter = 10;
    [SerializeField] private Sprite debugSprite;
    [SerializeField] Vector2Int roomSizeWorldUnits = new Vector2Int(30, 30);
    float worldUnitsInOneGridCell = 1;
    struct Walker
    {
        public Vector2 dir;
        public Vector2 pos;
    }
    [SerializeField] private List<EnemyTier> enemies;
    [SerializeField] private int enemyCount = 15;
    [SerializeField] private int chestCount = 2;
    [SerializeField] private GameObject chest, portal;

    List<Walker> walkers;
    [SerializeField] float chanceWalkerChangeDir = 0.5f, chanceWalkerSpawn = 0.05f;
    [SerializeField] float chanceWalkerDestoy = 0.05f;
    [SerializeField] int maxWalkers = 10;
    [SerializeField] float percentToFill = 0.2f;
    [SerializeField] private RuleTile wallTile, floorTile;
    [SerializeField] private RuleTile lowerWallTile, lowerFloorTile;
    private Grid gridParent;
    private Tilemap floorTilemap, wallTilemap;
    private Tilemap lowerFloorTilemap, lowerWallTilemap;
    [Header("Flooring")]
    [SerializeField] private float portionSpecial;
    [SerializeField] private List<Sprite> floorBase;
    [SerializeField] private List<Sprite> lowerFloorBase;
    [SerializeField] private List<Sprite> floorSpecial;
    [SerializeField] private List<Sprite> lowerFloorspecial;
    [SerializeField] private List<BigTile> bigTiles;
    [SerializeField] private List<BigTile> lowerBigTiles;

    List<Vector2> tails = new();
    List<Vector2> regulars = new();
    List<Enemy> levelEnemies = new();
    private static LevelGenerator instance;
    public static LevelGenerator Instance { get { return instance; } }
    private void Awake()
    {
        if (instance != this && instance != null) Destroy(this);
        else instance = this;
    }
    void Start()
    {


        gridParent = transform.GetChild(0).GetComponent<Grid>();
        var gridParent2 = transform.GetChild(1).GetComponent<Grid>();
        floorTilemap = gridParent.transform.GetChild(0).GetComponent<Tilemap>();
        wallTilemap = gridParent.transform.GetChild(1).GetComponent<Tilemap>();
        lowerFloorTilemap = gridParent2.transform.GetChild(0).GetComponent<Tilemap>();
        lowerWallTilemap = gridParent2.transform.GetChild(1).GetComponent<Tilemap>();

        ConfigureTiles();
        Setup();
        CreateFloors();
        CreateWalls();
        //RemoveSingleWalls();
        SpawnLevel();
        SpawnAdditional();
    }
    void Setup()
    {
        //find grid size
        roomHeight = roomHeightReal = Mathf.RoundToInt(roomSizeWorldUnits.x / worldUnitsInOneGridCell);
        //roomHeight = roomHeightReal + gutter * 2;
        roomWidth = roomWidthReal = Mathf.RoundToInt(roomSizeWorldUnits.y / worldUnitsInOneGridCell);

        //right=good, bottom=one higher
        //left=one right top=down one(or two?)

        var topRight = new Vector2(roomWidth, roomHeight);
        GameManager.Instance.BottomLeft = topRight * -1f + Vector2.right * 12 + Vector2.up * 12;
        GameManager.Instance.TopRight = topRight - Vector2.right * 11 - Vector2.up * 9;
        //Debug.Log(topRight);

        //roomWidth = roomWidthReal + gutter * 2;

        //var gHRatio = (float)roomHeightReal / roomHeight;
        //var gWRatio = (float)roomWidthReal / roomWidth;
        //gutterRatio = (gHRatio + gWRatio) / 2f;

        //create grid
        grid = new GridSpace[roomWidth, roomHeight];
        //set grid's default state
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                //make every cell "empty"
                grid[x, y] = GridSpace.Empty;
            }
        }
        //set first walker
        //init list
        walkers = new List<Walker>();
        //create a walker 
        Walker newWalker = new Walker();
        newWalker.dir = RandomDirection();
        //find center of grid
        Vector2 spawnPos = new Vector2(Mathf.RoundToInt(roomWidth / 2.0f),
                                        Mathf.RoundToInt(roomHeight / 2.0f));
        newWalker.pos = spawnPos;
        //add walker to list
        walkers.Add(newWalker);
    }
    void CreateFloors()
    {
        int iterations = 0;//loop will not run forever
        do
        {
            //create floor at position of every walker
            foreach (Walker myWalker in walkers)
            {
                grid[(int)myWalker.pos.x, (int)myWalker.pos.y] = GridSpace.Floor;
            }
            //chance: destroy walker
            int numberChecks = walkers.Count; //might modify count while in this loop
            for (int i = 0; i < numberChecks; i++)
            {
                //only if its not the only one, and at a low chance
                if (Random.value < chanceWalkerDestoy && walkers.Count > 1)
                {
                    tails.Add(walkers[i].pos);
                    //PlayerMovement.Instance.TeleportPlayer(walkers[i].pos * 2f);
                    walkers.RemoveAt(i);
                    break; //only destroy one per iteration
                }
            }
            //chance: walker pick new direction
            for (int i = 0; i < walkers.Count; i++)
            {
                if (Random.value < chanceWalkerChangeDir)
                {
                    Walker thisWalker = walkers[i];
                    thisWalker.dir = RandomDirection();
                    walkers[i] = thisWalker;
                }
            }
            //chance: spawn new walker
            numberChecks = walkers.Count; //might modify count while in this loop
            for (int i = 0; i < numberChecks; i++)
            {
                //only if # of walkers < max, and at a low chance
                if (Random.value < chanceWalkerSpawn && walkers.Count < maxWalkers)
                {
                    //create a walker 
                    Walker newWalker = new Walker();
                    newWalker.dir = RandomDirection();
                    newWalker.pos = walkers[i].pos;
                    walkers.Add(newWalker);
                }
            }
            //move walkers
            for (int i = 0; i < walkers.Count; i++)
            {
                Walker thisWalker = walkers[i];
                thisWalker.pos += thisWalker.dir;
                walkers[i] = thisWalker;

            }
            //avoid border of grid
            for (int i = 0; i < walkers.Count; i++)
            {
                Walker thisWalker = walkers[i];
                //clamp x,y to leave a 1 space boarder: leave room for walls
                thisWalker.pos.x = Mathf.Clamp(thisWalker.pos.x, 2, roomWidth - 3);
                thisWalker.pos.y = Mathf.Clamp(thisWalker.pos.y, 2, roomHeight - 3);
                walkers[i] = thisWalker;
                regulars.Add(thisWalker.pos);
            }
            //check to exit loop
            if ((float)NumberOfFloors() / (float)grid.Length > percentToFill)
            {
                break;
            }
            iterations++;
        } while (iterations < 100000);
    }

    void CreateWalls()
    {
        //loop though every grid space
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                //if theres a floor, check the spaces around it
                if (grid[x, y] == GridSpace.Floor)
                {
                    //if any surrounding spaces are empty, place a wall
                    if (grid[x, y + 1] == GridSpace.Empty)
                    {
                        grid[x, y + 1] = GridSpace.Wall;
                    }
                    if (grid[x, y - 1] == GridSpace.Empty)
                    {
                        grid[x, y - 1] = GridSpace.Wall;
                    }
                    if (grid[x + 1, y] == GridSpace.Empty)
                    {
                        grid[x + 1, y] = GridSpace.Wall;
                    }
                    if (grid[x - 1, y] == GridSpace.Empty)
                    {
                        grid[x - 1, y] = GridSpace.Wall;
                    }
                }
            }
        }
    }

    Vector2 ConvertPos(Vector2 gridPos)
    {
        //depends on positive or negative!!
        var offsetX = roomSizeWorldUnits.x > 0 ? (2f * Mathf.Sign(roomSizeWorldUnits.x)) : 0;
        var offsetY = roomSizeWorldUnits.x > 0 ? (2f * Mathf.Sign(roomSizeWorldUnits.x)) : 0;

        return new Vector2(gridPos.x * 2 - roomSizeWorldUnits.x + offsetX, gridPos.y * 2 - roomSizeWorldUnits.y + offsetY);
    }

    void SpawnAdditional()
    {
        regulars = regulars.OrderBy(x => Random.value).ToList();
        tails = tails.OrderBy(x => Random.value).ToList();
        var index = 0;

        while (tails.Count < chestCount + 2)
        {
            var t = regulars[++index];
            tails.Add(t);
        }
        var playerPos = tails[0];
        PlayerMovement.Instance.TeleportPlayer(ConvertPos(playerPos));
        var p = Instantiate(portal, ConvertPos(tails[1]), Quaternion.identity);
        PlayerRenderer.Instance.AssignTarget(p.transform.position);
        List<int> usedIndices = new();
        GunManager.Instance.RefillGuns();
        var gunList = GunManager.Instance.GunList;
        for (int i = 0; i < chestCount; i++)
        {
            var c = Instantiate(chest, ConvertPos(tails[i + 2]), Quaternion.identity).GetComponent<GunChest>();
            int newIndex;
            do
            {
                newIndex = Random.Range(0, gunList.Count);
            } while (usedIndices.Contains(newIndex) || GunManager.Instance.SelectedGun.Equals(gunList[newIndex]));
            c.StoredGun = gunList[newIndex];
        }

        for (int i = 0; i < enemyCount; i++)
        {
            var spawnPos = playerPos;
            do
            {
                spawnPos = regulars[++index];
            }
            while (Vector2.Distance(spawnPos, playerPos) < 6f);
            var tier = Random.value > 0.2f ? 1 : 0;
            var e = Instantiate(enemies[tier].Enemies[Random.Range(0, enemies[tier].Enemies.Count())], ConvertPos(spawnPos), Quaternion.identity).GetComponent<Enemy>();
            levelEnemies.Add(e);
        }
    }

    void RemoveSingleWalls()
    {
        //loop though every grid space
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                //if theres a wall, check the spaces around it
                if (grid[x, y] == GridSpace.Wall)
                {
                    //assume all space around wall are floors
                    bool allFloors = true;
                    //check each side to see if they are all floors
                    for (int checkX = -1; checkX <= 1; checkX++)
                    {
                        for (int checkY = -1; checkY <= 1; checkY++)
                        {
                            if (x + checkX < 0 || x + checkX > roomWidth - 1 ||
                                y + checkY < 0 || y + checkY > roomHeight - 1)
                            {
                                //skip checks that are out of range
                                continue;
                            }
                            if ((checkX != 0 && checkY != 0) || (checkX == 0 && checkY == 0))
                            {
                                //skip corners and center
                                continue;
                            }
                            if (grid[x + checkX, y + checkY] != GridSpace.Floor)
                            {
                                allFloors = false;
                            }
                        }
                    }
                    if (allFloors)
                    {
                        grid[x, y] = GridSpace.Floor;
                    }
                }
            }
        }
    }
    void SpawnLevel()
    {
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                switch (grid[x, y])
                {
                    case GridSpace.Empty:
                        Spawn(x, y, GridSpace.Wall);
                        break;
                    case GridSpace.Floor:
                        Spawn(x, y, GridSpace.Floor);
                        break;
                    case GridSpace.Wall:
                        Spawn(x, y, GridSpace.Wall);
                        break;
                }
            }
        }
    }

    void ConfigureTiles()
    {
        foreach (var b in bigTiles)
        {
            foreach (var r in b.RequiredTiles)
            {
                var direction = r.Direction;
                var number = -1;
                if (direction.Equals(Vector2Int.up)) number = 0;
                else if (direction.Equals(new Vector2Int(1, 1))) number = 1;
                else if (direction.Equals(Vector2Int.right)) number = 2;
                else number = 3;
                r.Assign(number);
            }
        }
    }

    Vector2 RandomDirection()
    {
        //pick random int between 0 and 3
        int choice = Mathf.FloorToInt(Random.value * 3.99f);
        //int choice = Random.Range(0, 3);
        //use that int to chose a direction
        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            default:
                return Vector2.right;
        }
    }
    int NumberOfFloors()
    {
        int count = 0;
        foreach (GridSpace space in grid)
        {
            if (space == GridSpace.Floor)
            {
                count++;
            }
        }
        return count;
    }
    void Spawn(float x, float y, GridSpace type)
    {
        //find the position to spawn
        Vector2 offset = roomSizeWorldUnits / 2;
        Vector2 spawnPos = new Vector2(x, y) * worldUnitsInOneGridCell - offset;
        var spawnPosList = new List<Vector3Int>
        {
            new Vector3Int((int)spawnPos.x * 2, (int)spawnPos.y * 2 + 1, 0), //bottom right

            new Vector3Int((int)spawnPos.x * 2 + 1, (int) spawnPos.y * 2 + 1, 0), //top right


            new Vector3Int((int)spawnPos.x * 2, (int)spawnPos.y * 2, 0), //bottom left
            new Vector3Int((int)spawnPos.x * 2 + 1, (int)spawnPos.y * 2, 0), //top left

        };
        //spawn object
        if (type.Equals(GridSpace.Wall))
        {
            foreach (var p in spawnPosList)
            {
                wallTilemap.SetTile(p, wallTile);
                lowerWallTilemap.SetTile(p, lowerWallTile);
            }
        }
        else
        {
            var val = Random.Range(0, 1f);
            if (val < portionSpecial)
            {
                var chosenIndex = Random.Range(0, bigTiles.Count);
                var chosen = bigTiles[chosenIndex];
                var chosenLower = lowerBigTiles[chosenIndex];
                for (int i = 0; i < 4; i++)
                {
                    if (chosen.RequiredTiles.Where(t => t.Number == i).Count() > 0)
                    {
                        SetTile(spawnPosList[i], chosen.RequiredTiles.Where(t => t.Number == i).ToArray()[0].TileSprite);
                        SetLowerTile(spawnPosList[i], chosenLower.RequiredTiles.Where(t => t.Number == i).ToArray()[0].TileSprite);
                    }
                    else
                    {
                        SetFloorTile(spawnPosList[i]);
                    }
                }
            }
            else
            {
                foreach (var p in spawnPosList)
                {
                    SetFloorTile(p);

                    //SetTile(p, floorBase[Random.Range(0, floorBase.Count)]);
                }
            }
            //SetTile(spawnPosV3, floorBase[Random.Range(0, floorBase.Count)]);
            //var val = Random.Range(0, 1f);
            //if (val < portionSpecial)
            //{
            //    //this is inefficient as fuck
            //    bool makeBig = true;
            //    BigTile bt = bigTiles[Random.Range(0, bigTiles.Count)];
            //    foreach(var rt in bt.RequiredTiles)
            //    {
            //        if (!grid[spawnPosV3.x + rt.Direction.x,spawnPosV3.y + rt.Direction.y].Equals(GridSpace.Floor)) makeBig = false;
            //    }
            //    if (val < portionSpecial / 2 && makeBig)
            //    {
            //        foreach (var rt in bt.RequiredTiles)
            //        {
            //            SetTile(spawnPosV3 + (Vector3Int)rt.Direction, rt.TileSprite);
            //        }
            //        return;
            //    }
            //    else SetTile(spawnPosV3, floorSpecial[Random.Range(0, floorSpecial.Count)]);
            //}
            //else
            //{
            //    SetTile(spawnPosV3, floorBase[Random.Range(0, floorBase.Count)]);
            //}
        }
    }
    private void SetTile(Vector3Int pos, Sprite sprite)
    {
        //Debug.Log(pos+","+sprite);
        Tile tempTile = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;
        tempTile.sprite = sprite;
        floorTilemap.SetTile(pos, tempTile);
    }
    private void SetLowerTile(Vector3Int pos, Sprite sprite)
    {
        Tile tempTile = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;
        tempTile.sprite= sprite;
        lowerFloorTilemap.SetTile(pos, tempTile);
    }
    private void SetFloorTile(Vector3Int pos)
    {
        if (Random.Range(0, 1f) > portionSpecial)
        {
            floorTilemap.SetTile(pos, floorTile);
            lowerFloorTilemap.SetTile(pos, lowerFloorTile);
        }
        else
        {
            var r = Random.Range(0, floorSpecial.Count);
            SetTile(pos, floorSpecial[r]);
            SetLowerTile(pos, lowerFloorspecial[r]);
        }
    }

    public bool AllEnemiesDead() => EnemyCount == DeadEnemyCount;
    public int EnemyCount => levelEnemies.Count;
    public int DeadEnemyCount => levelEnemies.Where(e => e.IsDead()).Count();

    [System.Serializable]
    private struct BigTile
    {
        public List<RequiredTile> RequiredTiles;
    }

    [System.Serializable]
    private struct RequiredTile
    {
        public int Number;
        //top left, top right, bottom left, bottom right
        public Sprite TileSprite;
        public Vector2Int Direction;
        RequiredTile(Vector2Int dir, Sprite sprite)
        {
            Direction = dir;
            TileSprite = sprite;
            Number = -1;
        }
        public void Assign(int number)
        {
            Number = number;
        }
    }
    [System.Serializable]
    public struct EnemyTier
    {
        public int Tier;
        public List<GameObject> Enemies;
    }
}