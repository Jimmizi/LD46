using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour {
    
    public Tilemap TerrainTilemap;
    public Tilemap PropsTilemap;
    
    public TileBase[] Tiles;
    public TileBase[] GroundLevelTiles;
    public TileBase[] DiagonalTiles;
    public TileBase[] PropsTiles;
    public TileBase[] TwoTilePropsTiles;
    public bool PrintDebug;
    
    public float DiagonalTileProbability = 0.2f;
    public float PropTileProbability = 0.6f;
    public float TwoTilePropTileProbability = 0.1f;

    public int TerrainWidth = 8;
    public int TerrainLength = 12;
    
    /// <summary>
    /// Road with in number of tiles.
    /// </summary>
    public int RoadWidth = 4;
    
    public int[,] TerrainMap => terrainMap;

    private int[,] terrainMap;
    
    private TileBase getTile(int level, int tileIndex) {
        if (level == 0 || (level == 1 && tileIndex < 16)) {
            return GroundLevelTiles[tileIndex];
        }
        else {
            if (tileIndex % 2 == 0 && tileIndex < 8) {
                if (Random.Range(0.0f, 1.0f) < DiagonalTileProbability) {
                    return DiagonalTiles[tileIndex / 2];
                }
            }
        }

        return Tiles[tileIndex];
    }
    
    private TileBase getPropsTile(int level) {
        int px = Random.Range(0, PropsTiles.Length);
        return PropsTiles[px];
    }
    
    private void setTwoTilePropsTiles(int level, Vector3Int tilePosition) {
        int px = Random.Range(0, TwoTilePropsTiles.Length / 2);
        TileBase tile = TwoTilePropsTiles[2 * px];
        PropsTilemap.SetTile(tilePosition, tile);
        tile = TwoTilePropsTiles[2 * px + 1];
        PropsTilemap.SetTile(new Vector3Int(tilePosition.x + 1, tilePosition.y, 0), tile);
    }

    void Start() {

        int width = TerrainWidth;
        int height = TerrainLength;
        int road_width_clamped = (RoadWidth <= 0) ? 1 : RoadWidth;
        int side_offset = (width - road_width_clamped) / 2;
        if (side_offset <= 0) {
            side_offset = 1;
        }

        terrainMap = new int[height, width];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (x > side_offset && x < (width - side_offset)) {
                    terrainMap[y, x] = 0;
                }
                else {
                    terrainMap[y, x] = Random.Range(0, 3);
                }
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (terrainMap[y, x] == 2) {
                    if (y > 0 && terrainMap[y - 1, x] != 2) {
                        terrainMap[y - 1, x] = 1;
                    }

                    if (y < (height - 1) && terrainMap[y + 1, x] != 2) {
                        terrainMap[y + 1, x] = 1;
                    }

                    if (x > 0 && terrainMap[y, x - 1] != 2) {
                        terrainMap[y, x - 1] = 1;
                    }

                    if (x < (width - 1) && terrainMap[y, x + 1] != 2) {
                        terrainMap[y, x + 1] = 1;
                    }

                    if (x > 0 && y > 0 && terrainMap[y - 1, x - 1] != 2) {
                        terrainMap[y - 1, x - 1] = 1;
                    }

                    if (x > 0 && y < (height - 1) && terrainMap[y + 1, x - 1] != 2) {
                        terrainMap[y + 1, x - 1] = 1;
                    }

                    if (x < (width - 1) && y > 0 && terrainMap[y - 1, x + 1] != 2) {
                        terrainMap[y - 1, x + 1] = 1;
                    }

                    if (x < (width - 1) && y < (height - 1) && terrainMap[y + 1, x + 1] != 2) {
                        terrainMap[y + 1, x + 1] = 1;
                    }
                }
            }
        }

        if (PrintDebug) {
            for (int y = 0; y < height; y++) {
                string line = "";
                for (int x = 0; x < width; x++) {
                    line += terrainMap[y, x].ToString() + ", ";
                }


                Debug.LogFormat("{0}\n", line);
            }
        }
    
        /* Decoration placement */
        for (int y = 0; y < height - 1; y++) {
            for (int x = 0; x < width - 1; x++) {
                if (x > side_offset && x < (width - side_offset)) {
                    continue;
                }

                int value = terrainMap[y, x];
                if (terrainMap[y + 1, x] >= value &&
                    terrainMap[y, x + 1] >= value &&
                    terrainMap[y + 1, x + 1] >= value) {

                    if (Random.Range(0.0f, 1.0f) >= PropTileProbability) {
                        continue;
                    }

                    if (terrainMap[y, x + 1] == value &&
                        Random.Range(0.0f, 1.0f) < TwoTilePropTileProbability) {
                        setTwoTilePropsTiles(value, new Vector3Int(2 * x + 1, 2 * height + (-2 * y - 1), 0));
                    } else {
                        TileBase tile = getPropsTile(value);
                        PropsTilemap.SetTile(new Vector3Int(2 * x + 1, 2 * height + (-2 * y - 1), 0), tile);
                    }
                }
            }
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int value = terrainMap[y, x];

                uint v0 = (y > 0 && x > 0)
                    ? ((terrainMap[y - 1, x - 1] > value) ? 2u :
                        ((terrainMap[y - 1, x - 1] < value) ? 0u : 1u)) : 1u;
                uint v1 = (y > 0)
                    ? ((terrainMap[y - 1, x] > value) ? 2u :
                        ((terrainMap[y - 1, x] < value) ? 0u : 1u)) : 1u;
                uint v2 = (y > 0 && x < (width - 1))
                    ? ((terrainMap[y - 1, x + 1] > value) ? 2u :
                        ((terrainMap[y - 1, x + 1] < value) ? 0u : 1u)) : 1u;
                uint v3 = (x > 0)
                    ? ((terrainMap[y, x - 1] > value) ? 2u :
                        ((terrainMap[y, x - 1] < value) ? 0u : 1u)) : 1u;
                uint v4 = (x < (width - 1))
                    ? ((terrainMap[y, x + 1] > value) ? 2u :
                        ((terrainMap[y, x + 1] < value) ? 0u : 1u)) : 1u;
                uint v5 = (y < (height - 1) && x > 0)
                    ? ((terrainMap[y + 1, x - 1] > value) ? 2u :
                        ((terrainMap[y + 1, x - 1] < value) ? 0u : 1u)) : 1u;
                uint v6 = (y < (height - 1))
                    ? ((terrainMap[y + 1, x] > value) ? 2u :
                        ((terrainMap[y + 1, x] < value) ? 0u : 1u)) : 1u;
                uint v7 = (y < (height - 1) && x < (width - 1))
                    ? ((terrainMap[y + 1, x + 1] > value) ? 2u :
                        ((terrainMap[y + 1, x + 1] < value) ? 0u : 1u)) : 1u;

                uint encoding =
                    ((v3 & 0x3u) << 4) |
                    ((v1 & 0x3u) << 2) |
                    (v0 & 0x3u);
                
                if (TerrainDictionary.Configurations.ContainsKey(encoding)) {
                    int tilex = TerrainDictionary.Configurations[encoding];
                    TileBase tile = getTile(value, tilex);
                    TerrainTilemap.SetTile(new Vector3Int(2 * x, 2 * height + (-2 * y), 0), tile);
                    if (PrintDebug) {
                        Debug.LogFormat("[{5}, {6}] 0: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
                    }
                }
                
                encoding =
                    ((v1 & 0x3u) << 4) |
                    ((v4 & 0x3u) << 2) |
                    (v2 & 0x3u);
                
                if (TerrainDictionary.Configurations.ContainsKey(encoding)) {
                    int tilex = TerrainDictionary.Configurations[encoding];

                    if (tilex < 8) {
                        tilex = (tilex + 2) % 8;
                    } else if (tilex < 16) {
                        tilex = (tilex - 8 + 2) % 8 + 8;
                    }

                    TileBase tile = getTile(value, tilex);
                    TerrainTilemap.SetTile(new Vector3Int(2 * x + 1, 2 * height + (-2 * y), 0), tile);
                    if (PrintDebug) {
                        Debug.LogFormat("[{5}, {6}] 1: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
                    }
                }
                
                encoding =
                    ((v6 & 0x3u) << 4) |
                    ((v3 & 0x3u) << 2) |
                    (v5 & 0x3u);
                
                if (TerrainDictionary.Configurations.ContainsKey(encoding)) {
                    int tilex = TerrainDictionary.Configurations[encoding];

                    if (tilex < 8) {
                        tilex = (tilex + 6) % 8;
                    } else if (tilex < 16) {
                        tilex = (tilex - 8 + 6) % 8 + 8;
                    }

                    TileBase tile = getTile(value, tilex);
                    TerrainTilemap.SetTile(new Vector3Int(2 * x, 2 * height + (-2 * y - 1), 0), tile);
                    if (PrintDebug) {
                        Debug.LogFormat("[{5}, {6}] 2: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
                    }
                }
                
                encoding =
                    ((v4 & 0x3u) << 4) |
                    ((v6 & 0x3u) << 2) |
                    (v7 & 0x3u);
                
                if (TerrainDictionary.Configurations.ContainsKey(encoding)) {
                    int tilex = TerrainDictionary.Configurations[encoding];

                    if (tilex < 8) {
                        tilex = (tilex + 4) % 8;
                    } else if (tilex < 16) {
                        tilex = (tilex - 8 + 4) % 8 + 8;
                    }

                    TileBase tile = getTile(value, tilex);
                    TerrainTilemap.SetTile(new Vector3Int(2 * x + 1, 2 * height + (-2 * y - 1), 0), tile);
                    if (PrintDebug) {
                        Debug.LogFormat("[{5}, {6}] 3: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
                    }
                }

                if (PrintDebug) {
                    Debug.Log("======================");
                }
            }
        }
        
    }

    void Update() {
        
    }
}
