using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeManager : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private float snakeMoveInterval = 0.5f;
    [SerializeField] private int tileCount = 10; // 10 means a 10x10 world
    [SerializeField] private float tileSize = 1f;
    [Header("Refs")]
    [SerializeField] private GameObject worldHolder;
    [SerializeField] private GameObject tilePrefab;


    private static SnakeManager instance;
    public static SnakeManager Instance { get { return instance; } }

    // The list of actual sprite renderers in the level
    private List<List<SpriteRenderer>> snakeRenderers = new List<List<SpriteRenderer>>();
    // The list of where snake sections are in the world
    private List<List<SnakeSection>> snakeSections = new List<List<SnakeSection>>();

    // Outside list is parts, inside list is directions
    // HeadN, HeadE, HeadS, HeadW
    // MiddleN, etc
    [SerializeField] private List<SpriteList> spritesList;

    // A list of the snake parts from head to tail, the Vector2 holds where in the snakeRenderers they are
    private List<Vector2Int> snakePartIndices = new List<Vector2Int>();
    // For now, we start facing/moving to the right (like the google snake game lol)
    private Vector2Int facingDirection = new Vector2Int(1, 0);


    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        else instance = this;
    }
    void Start()
    {
        // Fill up arrays with Blank values
        FillArrays();
        // Fill the world with empty sprite renderers
        CreateObjectRenderers();
        // Add in the snake

        snakePartIndices.Add(new Vector2Int(4, 5));
        snakePartIndices.Add(new Vector2Int(3, 5));
        snakePartIndices.Add(new Vector2Int(2, 5));
        snakePartIndices.Add(new Vector2Int(1, 5));
        snakePartIndices.Add(new Vector2Int(0, 5));
        // Move the snake every moveInterval seconds
        InvokeRepeating("TryMoveSnake", 0, snakeMoveInterval);
    }

    private void FillArrays()
    {
        SnakeSection blank = new SnakeSection();
        blank.direction = Direction.Blank;
        blank.part = SnakePart.Blank;

        for (int x = 0; x < tileCount; x++)
        {
            snakeSections.Add(new List<SnakeSection>());
            for (int y = 0; y < tileCount; y++)
            {
                snakeSections[x].Add(blank);
            }
        }
    }

    private void CreateObjectRenderers()
    {
        for (int x = 0; x < tileCount; x++)
        {
            snakeRenderers.Add(new List<SpriteRenderer>());
            for (int y = 0; y < tileCount; y++)
            {
                snakeRenderers[x].Add(new SpriteRenderer());
            }
        }
        for (int x = 0; x < tileCount; x++)
        {
            for (int y = 0; y < tileCount; y++)
            {
                GameObject newTile = Instantiate(tilePrefab, worldHolder.transform);
                newTile.transform.position = worldHolder.transform.position - new Vector3(tileSize * tileCount, tileSize * tileCount, 0)/2 + new Vector3(x * tileSize, y * tileSize, 0);
                newTile.transform.localScale = new Vector2(tileSize, tileSize);
                snakeRenderers[x][y] = newTile.GetComponent<SpriteRenderer>();
            }
        }
    }

    private void Update()
    {
        GetInput();
    }

    private void TryMoveSnake()
    {
        // If we can move the snake, move it and render it
        if (CanMoveSnake())
        {
            MoveSnake();
            RenderSnake();
        }
        // Otherwise, the player loses
        else
        {
            GameOver();
        }
    }

    private void GetInput()
    {
        int horizontal = Mathf.CeilToInt(Input.GetAxisRaw("Horizontal"));
        int vertical = Mathf.CeilToInt(Input.GetAxisRaw("Vertical"));

        if (horizontal == 0 && vertical == 0)
        {
            horizontal = facingDirection.x;
            vertical = facingDirection.y;
        }
        else if (horizontal == 1 && vertical == 1)
        {
            vertical = 0;
        }

        facingDirection = new Vector2Int(horizontal, vertical);
    }

    private bool CanMoveSnake()
    {
        // TODO: Return whether or not the snake can move, if it can't, player loses
        return true;
    }

    private void MoveSnake()
    {
        // THEORY
        // Head moves forward in the direction we're facing
        // Each body segment moves into where the next body segment is/was

        // How do we also set which part of the snake this should be?
        // Head is always head
        // Other parts will check where the next part is in relation to them, then use that to determine what they should be
        // The last part will be tail


        // Clear out the snakeSections list, we'll be replacing it
        snakeSections.Clear();
        FillArrays();

        Vector2Int partLastPosition = Vector2Int.zero;
        for (int i = 0; i < snakePartIndices.Count; i++)
        {
            // If this is the first section, this is the head
            if (i == 0)
            {
                // Set partLastPosition to this
                // This is so that the next section after the head can move to where the head is
                partLastPosition = snakePartIndices[i];
                // Move the head in the facing direction
                snakePartIndices[i] += facingDirection;

                // Set the value of snakeSections at this snakePartIndex to head and set direction
                SnakeSection sectionToModify = snakeSections[snakePartIndices[i].x][snakePartIndices[i].y];
                sectionToModify.part = SnakePart.Head;
                sectionToModify.direction = (Direction)PartDirectionIndexFromVector2(facingDirection);
                snakeSections[snakePartIndices[i].x][snakePartIndices[i].y] = sectionToModify;
            }
            // Otherwise it's one of the body sections
            else
            {
                // Set where the part currently is
                Vector2Int partCurrentPosition = snakePartIndices[i];
                // Move the part
                snakePartIndices[i] = partLastPosition;
                // Set partLastPosition to where the part just moved from
                partLastPosition = partCurrentPosition;

                // Set the value of snakeSections at this snakePartIndex to what it should be
                SnakeSection sectionToModify = snakeSections[snakePartIndices[i].x][snakePartIndices[i].y];

                Vector2Int pointingToPreviousPart = snakePartIndices[i - 1] - snakePartIndices[i];
                Vector2Int pointingToNextPart = new Vector2Int(0, 0);
                if (i + 1 < snakePartIndices.Count)
                    pointingToNextPart = snakePartIndices[i + 1] - snakePartIndices[i];

                // Figure out which part it should be
                sectionToModify.part = (i == snakePartIndices.Count - 1) ? SnakePart.Tail : (SnakePart)FindPartType(snakePartIndices[i], pointingToPreviousPart, pointingToNextPart);
                // Use the difference between this part and the part before it (closer to the head) to determine the direction
                sectionToModify.direction = (Direction)PartDirectionIndexFromVector2(pointingToPreviousPart);
                snakeSections[snakePartIndices[i].x][snakePartIndices[i].y] = sectionToModify;
            }
        }
    }

    private int PartDirectionIndexFromVector2(Vector2Int dir)
    {
        if (dir.x == 0)
        {
            if (dir.y > 0)
            {
                return 0;
            }
            else if (dir.y < 0)
            {
                return 2;
            }
        }
        else if (dir.x > 0)
        {
            return 1;
        }
        else if (dir.x < 0)
        {
            return 3;
        }

        Debug.LogError("The direction you supplied was not valid: ");
        Debug.LogError(dir);
        return 0;
    }

    private int FindPartType(Vector2Int currentPos, Vector2Int pointingToPreviousPart, Vector2Int pointingToNextPart)
    {
        bool allYsDifferent = currentPos.y != pointingToPreviousPart.y && currentPos.y != pointingToNextPart.y;
        bool allXsDifferent = currentPos.x != pointingToPreviousPart.x && currentPos.x != pointingToNextPart.x;

        // Return Straight if the Xs or Ys are all different (and thus in a line), otherwise Corner
        return allXsDifferent || allYsDifferent ? 1 : 2;
    }

    private void RenderSnake()
    {
        foreach (List<SpriteRenderer> listOfSR in snakeRenderers)
        {
            foreach (SpriteRenderer sr in listOfSR)
            {
                sr.enabled = false;
            }
        }

        int x = 0;
        int y = 0;
        foreach (List<SnakeSection> snakeSectionList in snakeSections)
        {
            foreach (SnakeSection snakeSection in snakeSectionList)
            {
                if (snakeSection.direction != Direction.Blank && snakeSection.part != SnakePart.Blank)
                {
                    RenderSnakeSection(snakeSection, x, y);
                }
                y++;
            }
            y = 0;
            x++;
        }
    }

    private void RenderSnakeSection(SnakeSection section, int x, int y)
    {
        snakeRenderers[x][y].sprite = spritesList[(int)section.part].spritesList[(int)section.direction];
        snakeRenderers[x][y].enabled = true;
    }

    private void GameOver()
    {
        // TODO: Show the player their score
        Debug.Log("GAME OVER");
    }
}

[System.Serializable]
public class SpriteList
{
    [SerializeField]
    public List<Sprite> spritesList;
}

struct SnakeSection
{
    public SnakePart part;
    public Direction direction;
}

enum SnakePart
{
    Head,
    Straight,
    Corner,
    Tail,
    Blank
}

enum Direction
{
    North,
    East,
    South,
    West,
    Blank
}