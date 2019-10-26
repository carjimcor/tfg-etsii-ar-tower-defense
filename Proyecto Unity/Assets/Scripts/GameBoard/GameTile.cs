using UnityEngine;

public class GameTile : MonoBehaviour
{
    #region Variables
    [SerializeField]
    GameTileType type = default;

    public GameTileType Type => type;

    public bool BlocksInteraction =>
        Type == GameTileType.Blocking;

    [SerializeField]
    Material normalFill = default, normalRim = default, blockingFill = default, blockingRim = default;
    new Renderer renderer;

    [SerializeField]
    Transform arrow = default;

    GameTile northEast, southEast, southWest, northWest, east, west;
    [HideInInspector]
    public GameTile nextOnPath = null;
    [HideInInspector]
    public GameTile nextOnPathStart = null;

    public Direction PathDirection { get; private set; }

    public HexCoordinates coordinates;
    public int distance;
    public int distanceStart;
    public bool HasPath => distance != int.MaxValue;

    GameTileContent content;
                   
    static Quaternion
    northEastRotation = Quaternion.Euler(90f, 30f, 0f),
    eastRotation = Quaternion.Euler(90f, 90f, 0f),
    southEastRotation = Quaternion.Euler(90f, 150f, 0f),
    southWestRotation = Quaternion.Euler(90f, 210f, 0f),
    westRotation = Quaternion.Euler(90f, 270f, 0f),
    northWestRotation = Quaternion.Euler(90f, 330f, 0f);

    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!");
            if (content != null)
            {
                content.Recycle();
            }
            content = value;
            content.Tile = this;
            content.transform.localPosition = transform.localPosition;
        }
    }
    #endregion

    private void Awake()
    {
        // Initialization
        renderer = gameObject.GetComponent<Renderer>();
    }

    public void ChangeToBlocking()
    {
        // Turns this tile into blocking
        Debug.Assert(type != GameTileType.Blocking, "This tile is already blocking");
        //Renderer renderer = GetComponent<Renderer>();

        Material[] materials = renderer.materials;
        materials[0] = blockingFill;
        materials[1] = blockingRim;
        renderer.materials = materials;

        type = GameTileType.Blocking;
    }

    public void ChangeToNormal()
    {
        // Turns this tile into normal
        Debug.Assert(type != GameTileType.Normal, "This tile is already normal");
        //Renderer renderer = GetComponent<Renderer>();
        
        Material[] materials = renderer.materials;
        materials[0] = normalFill;
        materials[1] = normalRim;
        renderer.materials = materials;

        type = GameTileType.Normal;
    }

    #region PATH

    public void ClearPath()
    {
        // Resets the tile path
        distance = int.MaxValue;
        nextOnPath = null;
    }

    public void ShowPath()
    {
        // Updates the arrow path and enables it
        if (distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);

        arrow.localRotation =
            nextOnPath == northEast ? northEastRotation :
            nextOnPath == east ? eastRotation :
            nextOnPath == southEast ? southEastRotation :
            nextOnPath == southWest ? southWestRotation :
            nextOnPath == west ? westRotation :
            northWestRotation;
    }

    public void HidePath()
    {
        // Hides arrow
        arrow.gameObject.SetActive(false);
    }

    public void BecomeDestination()
    {
        // Makes this tile destination
        distance = 0;
        nextOnPath = null;
    }

    GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        // Expand the path to a neighbour tile and updates their distances and direction
        // If the neighbour already has a better path set, the method is ignored
        Debug.Assert(HasPath, "No path!");
        if (neighbor == null || neighbor.distance <= distance + 1)
        {
            return null;
        }

        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;
        neighbor.PathDirection = direction;
        return neighbor.Content.BlocksPath ? null : neighbor;
    }

    public GameTile GrowPathNorthEast() => GrowPathTo(northEast, Direction.SouthWest);

    public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);

    public GameTile GrowPathSouthEast() => GrowPathTo(southEast, Direction.NorthWest);

    public GameTile GrowPathSouthWest() => GrowPathTo(southWest, Direction.NorthEast);

    public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);

    public GameTile GrowPathNorthWest() => GrowPathTo(northWest, Direction.SouthEast);

    #endregion

    #region NEIGHBOURS
    // Neighbour association creation
    public static void MakeEastWestNeighbors(GameTile east, GameTile west)
    {
        Debug.Assert(
            west.east == null && east.west == null, "Redefined neighbors!"
        );

        west.east = east;
        east.west = west;
    }

    public static void MakeNorthEastSouthWestNeighbors(GameTile northEast, GameTile southWest)
    {
        Debug.Assert(
            northEast.southWest == null && southWest.northEast == null, "Redefined neighbors!"
        );
        northEast.southWest = southWest;
        southWest.northEast = northEast;
    }

    public static void MakeNorthWestSouthEastNeighbors(GameTile northWest, GameTile southEast)
    {
        Debug.Assert(
            northWest.southEast == null && southEast.northWest == null, "Redefined neighbors!"
        );
        northWest.southEast = southEast;
        southEast.northWest = northWest;
    }

    #endregion
}
