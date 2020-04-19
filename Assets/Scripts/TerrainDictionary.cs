using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainDictionary
{
    public static IReadOnlyDictionary<uint, byte> Configurations => _configurations;

    /*
     * 
     *  | 0 | 1 | 2 |
     *  |---|---|---|
     *  | 7 |   | 3 |
     *  |---|---|---|
     *  | 6 | 5 | 4 |
     */
    private static readonly Dictionary<uint, byte> _configurations = new Dictionary<uint, byte>
    {
        { 0b00_00_00, 0 },
        
        /* 1_1 */
        { 0b00_00_01, 0 },
        { 0b00_01_00, 7 },
        { 0b01_00_00, 1 },
        
        /* 2_1s */
        { 0b00_01_01, 7 },
        { 0b01_00_01, 1 },
        { 0b01_01_00, 12 },
        
        /* 3_1s */
        { 0b01_01_01, 16 },
        
        
        /* 1_2 [N/A] */
        /*{ 0b00_00_10, 0 },
        { 0b00_10_00, 0 },
        { 0b10_00_00, 0 },*/
        
        /* 2_2s [N/A] */
        /*{ 0b00_10_10, 0 },
        { 0b10_00_10, 0 },
        { 0b10_10_00, 0 },*/
        
        /* 3_2s */
        { 0b10_10_10, 16 },
        
        /* 1_1, 1_2 [N/A] */
        /*{ 0b00_10_01, 0 },
        { 0b10_00_01, 0 },
        { 0b10_01_00, 0 },
        { 0b00_01_10, 0 },
        { 0b01_00_10, 0 },
        { 0b01_10_00, 0 },*/
        
        /* 2_1s, 1_2 */
        { 0b10_01_01, 16 },
        { 0b01_10_01, 16 },
        { 0b01_01_10, 16 },
        
        /* 1_1, 2_2s */
        { 0b10_10_01, 16 },
        { 0b10_01_10, 16 },
        { 0b01_10_10, 16 },
        
    };

}
