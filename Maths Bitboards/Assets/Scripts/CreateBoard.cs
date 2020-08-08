using System;
using UnityEngine;
using UnityEngine.UI;

public class CreateBoard : MonoBehaviour
{
    GameObject[] tiles;

    // SETUP
    [SerializeField] GameObject[] tilePrefabs;
    [SerializeField] GameObject housePrefab;
    [SerializeField] GameObject treePrefab;

    [SerializeField] Text score;
    [SerializeField] int boardSize = 8;
    [SerializeField] float treeSpawnTime = 0.5f;

    //Need a bitboard for every cell type/
    long dirtBB = 0;
    long treeBB = 0;
    long desertBB = 0;
    long playerBB = 0;

    //Score Config
    [SerializeField] int dirtScoreValue = 10;
    [SerializeField] int desertScoreValue = 2;

    // Start is called before the first frame update
    void Start()
    {
        tiles = new GameObject[64];
        GenerateBoard();
        InvokeRepeating("PlantTree", treeSpawnTime, treeSpawnTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                int row = (int)hit.collider.gameObject.transform.position.z;
                int col = (int)hit.collider.gameObject.transform.position.x;

                if (GetCellState((dirtBB | desertBB) & ~treeBB, row, col))
                {
                    GameObject house = Instantiate(housePrefab);
                    house.transform.parent = hit.collider.gameObject.transform;
                    house.transform.localPosition = Vector3.zero;
                    playerBB = SetCellState(playerBB, row, col);
                    CalculateScore();
                }
            }
        }
    }

    void PlantTree()
    {
        int randomRow = UnityEngine.Random.Range(0, boardSize);
        int randomCol = UnityEngine.Random.Range(0, boardSize);
        //Only allow trees on dirt and no player houses - all of the dirt cells - occupied player cells, and no player
        if (GetCellState(dirtBB & ~playerBB, randomRow, randomCol))
        {
            GameObject tree = Instantiate(treePrefab);
            tree.transform.parent = tiles[randomRow * boardSize + randomCol].transform;
            tree.transform.localPosition = Vector3.zero;
            treeBB = SetCellState(treeBB, randomRow, randomCol);
        }
    }

    private void GenerateBoard()
    {
        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                //Pick a random tile type, instantiate at the position in the board.
                int randomTile = UnityEngine.Random.Range(0, tilePrefabs.Length);
                Vector3 pos = new Vector3(column, 0, row);
                GameObject tile = Instantiate(tilePrefabs[randomTile], pos, Quaternion.identity);
                //Setup tile
                tile.transform.SetParent(this.transform);
                tile.name = tile.tag + "_" + row + "_" + column;
                tiles[row * 8 + column] = tile;
                //If dirt tile type, add to dirtBB
                if (tile.tag == "Dirt")
                {
                    dirtBB = SetCellState(dirtBB, row, column);
                    //PrintBitBoard("Dirt", dirtBB);
                }
                else if (tile.tag == "Desert")
                {
                    desertBB = SetCellState(desertBB, row, column);
                }
            }
        }
    }

    long SetCellState(long bitboard, int row, int col)
    {
        //1 long value bit shifted by the flattened array
        long newBit = 1L << (row * boardSize + col);
        //Adds the bits together
        return (bitboard |= newBit);
    }

    void PrintBitBoard(string name, long bitboard)
    {
        Debug.Log(name + ": " + Convert.ToString(bitboard, 2).PadLeft(64, '0'));
    }

    bool GetCellState(long bitboard, int row, int col)
    {
        long mask = 1L << (row * boardSize + col);
        return ((bitboard & mask) != 0);
    }

    int CellCount(long bitboard)
    {
        int count = 0;
        long bb = bitboard;
        while (bb != 0)
        {
            bb &= bb - 1;
            count++;
        }
        return count;
    }

    void CalculateScore()
    {
        score.text = "Score: " + (CellCount(dirtBB & playerBB) * dirtScoreValue) + CellCount(desertBB & playerBB) * desertScoreValue; ;
    }

}
