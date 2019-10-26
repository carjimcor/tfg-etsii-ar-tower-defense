using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    #region Variables
    public static GameBoard instance;
    GameTileContentFactory contentFactory;

    bool initializing = false;

    GameTile[] tiles;
    List<GameTile> searchFrontier = new List<GameTile>();

    List<GameTileContent> updatingContent = new List<GameTileContent>();
    List<GameTile> spawnPoints = new List<GameTile>();
    public int SpawnPointCount => spawnPoints.Count;

    bool showPaths;
    Vector2Int size;
    Vector2 offset;

    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    // Inspector Variables
    [SerializeField]
    Transform ground = default;
    [SerializeField]
    GameTile tilePrefab = default;
    #endregion

    #region Static Methods
    void OnEnable()
    {
        instance = this;
    }

    public static void RemoveUpdatingContent(GameTileContent content)
    {
        instance.updatingContent.Remove(content);
        GameTile tile = content.Tile;
        tile.Content = instance.contentFactory.Get(GameTileContentType.Empty);
        content.Recycle();
    }

    public static int SpawnCount()
    {
        return instance.SpawnPointCount;
    }

    public static bool FindPathsStatic()
    {
        return instance.FindPaths();
    }
    #endregion

    #region Control Methods
    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory, string template = null)
    {
        initializing = true;
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3((size.x + 0.5f) * HexMetrics.innerRadius * 2, (size.y - 1) * HexMetrics.outerRadius * 1.5f + 2, 1f);
        ground.localScale *= 2f;

        offset = new Vector2(
            ((size.x - 0.5f) * 0.5f) * HexMetrics.doubleInnerRadius,
            ((size.y - 1) * 0.5f) * HexMetrics.heightSeparation
        );

        tiles = new GameTile[size.x * size.y];

        for (int i = 0, y = 0; y < size.y; y++)
        {
            // las líneas impares las desplazamos
            float extraOffset = y % 2 == 0 ? 0f : 0.5f;

            for (int x = 0; x < size.x; x++, i++)
            {

                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.name = "Tile " + i.ToString();
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(
                    (x + extraOffset) * HexMetrics.doubleInnerRadius - offset.x, 0f, (y) * HexMetrics.heightSeparation - offset.y
                );
                tile.coordinates = HexCoordinates.FromOffsetCoordinates(x, y);

                // Horizontal Neighbours
                if (x > 0)
                {
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                }

            }
        }

        // Diagonal Neighbours - only even rows
        for (int row = 1; row < size.y; row += 2)
        {
            bool lastRow = row == size.y - 1;
            // Only odd rows:
            for (int x = 0; x < size.x; x++)
            {
                GameTile currentTile = tiles[row * size.x + x];

                // Diagonals to the left only
                if (!lastRow)
                    GameTile.MakeNorthWestSouthEastNeighbors(tiles[(row + 1) * size.x + x], currentTile);
                GameTile.MakeNorthEastSouthWestNeighbors(currentTile, tiles[(row - 1) * size.x + x]);

                if (x < size.x - 1)
                {
                    // Diagonals to the right only
                    if (!lastRow)
                        GameTile.MakeNorthEastSouthWestNeighbors(tiles[(row + 1) * size.x + x + 1], currentTile);
                    GameTile.MakeNorthWestSouthEastNeighbors(currentTile, tiles[(row + -1) * size.x + x + 1]);
                }
            }
        }

        // Content:
        foreach (GameTile tile in tiles)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
        }

        if (template != null)
        {
            Debug.Assert(template.Length == size.x * size.y, "Invalid Template!");
            for (int i = 0; i < template.Length; i++)
            {
                // For non tower content we use letters, for tower types we use numbers
                // Empty - e, Destination - d, Wall - w, SpawnPoint - s, Tower - <Numbers>

                //GameTileContent content = null;

                switch (template[i])
                {
                    case 'e':
                        break;
                    case 'b':
                        ToggleBlocking(tiles[i]);
                        break;
                    case 'd':
                        ToggleDestination(tiles[i]);
                        break;
                    case 'w':
                        tiles[i].Content = contentFactory.Get(GameTileContentType.Wall);
                        break;
                    case 's':
                        ToggleSpawnPoint(tiles[i]);
                        break;
                    default:
                        Debug.Assert(false, "Unnsuported tile type: " + template[i]);
                        break;
                }
                //tiles[i].Content = content;
            }
            Debug.Assert(FindPaths(), "Invalid Template!");
        }
        else
        {
            ToggleSpawnPoint(tiles[0]);
            ToggleDestination(tiles[tiles.Length - 1]);
        }

        foreach (GameTile tile in tiles)
        {
            tile.nextOnPathStart = tile.nextOnPath;
            tile.distanceStart = tile.distance;
        }

        initializing = false;
    }

    public string getTemplate()
    {
        string res = "";

        for (int i = 0; i < tiles.Length; i++)
        {
            GameTile tile = tiles[i];

            switch (tile.Content.Type)
            {
                case GameTileContentType.Empty:
                    if (tile.BlocksInteraction)
                    {
                        res += 'b';
                    }
                    else
                    {
                        res += 'e';
                    }
                    break;
                case GameTileContentType.Tower:
                    res += 'e';
                    break;
                case GameTileContentType.Destination:
                    res += 'd';
                    break;
                case GameTileContentType.Wall:
                    res += 'w';
                    break;
                case GameTileContentType.SpawnPoint:
                    res += 's';
                    break;

                default:
                    Debug.Assert(false, "Unsupported content type: " + tile.Content.Type);
                    break;
            }
        }

        return res;
    }

    public void GameUpdate()
    {
        for (int i = 0; i < updatingContent.Count; i++)
        {
            updatingContent[i].GameUpdate();
        }
    }

    bool FindPaths()
    {
        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Add(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        if (searchFrontier.Count == 0)
        {
            return false;
        }

        List<GameTile> aux = new List<GameTile>();

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier[0];
            searchFrontier.RemoveAt(0);
            aux.Clear();

            aux.Add(tile.GrowPathEast());
            aux.Add(tile.GrowPathWest());
            aux.Add(tile.GrowPathSouthWest());
            aux.Add(tile.GrowPathNorthEast());
            aux.Add(tile.GrowPathNorthWest());
            aux.Add(tile.GrowPathSouthEast());

            foreach (GameTile tile_aux in aux)
            {
                if (tile_aux != null)
                {
                    searchFrontier.Add(tile_aux);
                }
            }
            searchFrontier.Sort((x, y) => x.distance.CompareTo(y.distance));
        }
        
        foreach (GameTile tile in spawnPoints)
        {
            if (tile.Content.Type == GameTileContentType.SpawnPoint)
            {
                if (!pathFromSpawnPointTileToDestination(tile))
                {
                    return false;
                }
            }
        }

        if (!initializing)
        {
            foreach (GameTile tile in tiles)
            {
                if (!tile.HasPath)
                {
                    // If a tile has no path, we set it the path at the beggining of the game
                    tile.nextOnPath = tile.nextOnPathStart;
                    tile.distance = tile.distanceStart;
                }
            }
        }

        if (showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }

        return true;
    }

    GameTile pathFromSpawnPointTileToDestination(GameTile tile)
    {
        GameTile end = tile;

        while (end.distance != 0)
        {
            if (end.nextOnPath == null)
            {
                Debug.Log("Reached dead end");
                return null;
            }

            end = end.nextOnPath;
        }

        return end;
    }

    public GameTile TouchCell(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1))
        {

            Vector3 position = hit.point;

            position = transform.InverseTransformPoint(position);
            HexCoordinates coordinates = HexCoordinates.FromPosition(position, size, offset);
            //Debug.Log("touched at " + coordinates.ToString());
            Vector2 coordinatesAux = HexCoordinates.Reverse(coordinates);

            if (coordinatesAux.x >= 0 && coordinatesAux.x < size.x && coordinatesAux.y >= 0 && coordinatesAux.y < size.y)
            {
                int index = coordinates.X + coordinates.Z * size.x + coordinates.Z / 2;
                return tiles[index];
            }
        }
        return null;
    }

    public GameTile GetSpawnPoint(int index)
    {
        if (index >= 0 && index < spawnPoints.Count)
            return spawnPoints[index];
        else
            return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
    #endregion

    #region Toggle Methods
    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths())
            {
                tile.Content =
                    contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    public void ToggleSpawnPoint(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if (spawnPoints.Count > 1)
            {
                spawnPoints.Remove(tile);
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
            spawnPoints.Add(tile);
        }
    }

    public void ToggleTower(GameTile tile, TowerType towerType)
    {
        if (tile.Content.Type == GameTileContentType.Empty)
        {
            int cost = contentFactory.TowerCost(towerType);
            if (Game.EnoughCredits(cost))
            {
                tile.Content = contentFactory.Get(towerType);
                if (FindPaths())
                {
                    updatingContent.Add(tile.Content);
                    Game.LoseCredits(cost);
                    Debug.Log("Buying tower for " + cost + "#");

                }
                else
                {
                    tile.Content = contentFactory.Get(GameTileContentType.Empty);
                    FindPaths();
                }
            }
        }
        //else if (tile.Content.Type == GameTileContentType.Wall)
        //{
        //    tile.Content = contentFactory.Get(towerType);
        //    updatingContent.Add(tile.Content);
        //}
    }

    public void ToggleBlocking(GameTile tile)
    {
        if (tile.Type == GameTileType.Normal)
        {
            tile.ChangeToBlocking();
        }
        else
        {
            tile.ChangeToNormal();
        }
    }
    #endregion
}
