using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour {
    
    public Tilemap TerrainTilemap;
    public TileBase[] Tiles;

    void Start() {

        int width = 4;
        int height = 4;
        
        int[,] generated = new int[4, 4];

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                generated[y, x] = Random.Range(0, 3);
            }
        }
        
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                if (generated[y, x] == 2) {
                    if (y > 0 && generated[y - 1, x] != 2) {
                        generated[y - 1, x] = 1;
                    }

                    if (y < (height - 1) && generated[y + 1, x] != 2) {
                        generated[y + 1, x] = 1;
                    }

                    if (x > 0 && generated[y, x - 1] != 2) {
                        generated[y, x - 1] = 1;
                    }

                    if (x < (width - 1) && generated[y, x + 1] != 2) {
                        generated[y, x + 1] = 1;
                    }

                    if (x > 0 && y > 0 && generated[y - 1, x - 1] != 2) {
                        generated[y - 1, x - 1] = 1;
                    }

                    if (x > 0 && y < (height - 1) && generated[y + 1, x - 1] != 2) {
                        generated[y + 1, x - 1] = 1;
                    }
                    
                    if (x < (width - 1) && y > 0 && generated[y - 1, x + 1] != 2) {
                        generated[y - 1, x + 1] = 1;
                    }

                    if (x < (width - 1) && y < (height - 1) && generated[y + 1, x + 1] != 2) {
                        generated[y + 1, x + 1] = 1;
                    }
                }
            }
        }

        for (int y = 0; y < height; y++) {
            string line = "";
            for (int x = 0; x < width; x++) {
                line += generated[y, x].ToString() + ", ";
            }
            Debug.LogFormat("{0}\n", line);
        }

        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int value = generated[y, x];

                uint v0 = (y > 0 && x > 0)
                    ? ((generated[y - 1, x - 1] > value) ? 2u :
                        ((generated[y - 1, x - 1] < value) ? 0u : 1u)) : 1u;
                uint v1 = (y > 0)
                    ? ((generated[y - 1, x] > value) ? 2u :
                        ((generated[y - 1, x] < value) ? 0u : 1u)) : 1u;
                uint v2 = (y > 0 && x < (height - 1))
                    ? ((generated[y - 1, x + 1] > value) ? 2u :
                        ((generated[y - 1, x + 1] < value) ? 0u : 1u)) : 1u;
                uint v3 = (x > 0)
                    ? ((generated[y, x - 1] > value) ? 2u :
                        ((generated[y, x - 1] < value) ? 0u : 1u)) : 1u;
                uint v4 = (x < (width - 1))
                    ? ((generated[y, x + 1] > value) ? 2u :
                        ((generated[y, x + 1] < value) ? 0u : 1u)) : 1u;
                uint v5 = (y < (height - 1) && x > 0)
                    ? ((generated[y + 1, x - 1] > value) ? 2u :
                        ((generated[y + 1, x - 1] < value) ? 0u : 1u)) : 1u;
                uint v6 = (y < (height - 1))
                    ? ((generated[y + 1, x] > value) ? 2u :
                        ((generated[y + 1, x] < value) ? 0u : 1u)) : 1u;
                uint v7 = (y < (height - 1) && x < (height - 1))
                    ? ((generated[y + 1, x + 1] > value) ? 2u :
                        ((generated[y + 1, x + 1] < value) ? 0u : 1u)) : 1u;

                uint encoding =
                    ((v3 & 0x3u) << 4) |
                    ((v1 & 0x3u) << 2) |
                    (v0 & 0x3u);
                
                if (TerrainDictionary.Configurations.ContainsKey(encoding)) {
                    int tilex = TerrainDictionary.Configurations[encoding];
                    TerrainTilemap.SetTile(new Vector3Int(2 * x, -2 * y, 0), Tiles[tilex]);
                    Debug.LogFormat("[{5}, {6}] 0: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
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

                    TerrainTilemap.SetTile(new Vector3Int(2 * x + 1, -2 * y, 0), Tiles[tilex]);
                    Debug.LogFormat("[{5}, {6}] 1: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
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

                    TerrainTilemap.SetTile(new Vector3Int(2 * x, -2 * y - 1, 0), Tiles[tilex]);
                    Debug.LogFormat("[{5}, {6}] 2: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
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

                    TerrainTilemap.SetTile(new Vector3Int(2 * x + 1, -2 * y - 1, 0), Tiles[tilex]);
                    Debug.LogFormat("[{5}, {6}] 3: {0}, {1}, {2} => {3} [{4}]\n", v0, v1, v3, encoding, tilex, y, x);
                }
                
                Debug.Log("======================");
            }
        }
        
    }

    void Update() {
        
    }
}
