using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Controller : MonoBehaviour {
    public enum State { empty, X, O }

    [SerializeField] UIController uiController;
    [SerializeField] List<Block> blocks;

    bool playersTurn = true;
    bool gameOver;
    bool smarterComputer;
    int fieldSize = 3;
    int moves = 0;


    public bool CanPlayerMove {
        get { return playersTurn && !gameOver; }
    }
    public bool SmarterComputer {
        get { return smarterComputer; }
        set { smarterComputer = value; }
    }

    void Start() {
        uiController.SetupPanel();

        //initial blocks setup
        for (int i = 0; i < blocks.Count; i++) {
            Vector2 coord = new(i / 3, i % 3);
            blocks[i].Setup(this, coord);
        }
    }
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    public void SetupGame() {
        for (int i = 0; i < blocks.Count; i++) {
            blocks[i].Clear();
        }

        moves = 0;
        gameOver = false;

        
        playersTurn = true; //player always goes first
        uiController.SetTurnText("Your turn");
    }

    /// <summary>
    /// For a smarter version:
    /// If available - go center, then check for possible win, then check if can block player. If none, just place at random
    /// For a stupid version, just go random
    /// </summary>
    void ComputerMove() {
        if (smarterComputer) {
            Block center = GetBlockAtPosition(new(1, 1));
            if (center.MyState == State.empty) MakeMove(center);
            else {
                Block winningMove = CheckIfPossibleFinishMove(false);
                Block blockingMove = CheckIfPossibleFinishMove(true);
                if (winningMove) {
                    MakeMove(winningMove);
                } else if (blockingMove) {
                    MakeMove(blockingMove);
                } else {
                    RandomComputerMove();
                }
            }
        } else {
            RandomComputerMove();
        }
    }

    void RandomComputerMove() {
        System.Random r = new System.Random();
        var rnd = blocks.OrderBy(x => r.Next()).ToList();
        foreach (var item in rnd) {
            if (item.MyState == State.empty) {
                MakeMove(item);
                break;
            }
        }
    }

    /// <summary>
    /// If not winning move, change turn
    /// Slightly delayed computer move, so it looks nicer
    /// </summary>
    public void MakeMove(Block move) {
        move.Mark(playersTurn);
        moves++;
        if (CheckForWin(move)) {
            GameOver();
        } else {
            if (CheckForEarlyTie()) TieGame();
            else {
                if (playersTurn) {
                    if (moves < 9) uiController.SetTurnText("Computers turn");
                    if (!gameOver) StartCoroutine(Delay(ComputerMove, Random.Range(0.3f, .8f)));
                } else {
                    uiController.SetTurnText("Your turn");
                }
            }
            playersTurn = !playersTurn;
        }
    }

    /// <summary>
    /// "The game should also recognize when no crosses or circles can be placed in order to win, and the game should then end."
    /// tie possible from 2 moves left
    /// </summary>
    bool CheckForEarlyTie() {
        if(moves == 7) {
            if (CheckIfPossibleFinishMove(false) == null) {
                if (CheckIfPossibleFinishMove(true) == null) return true;
            }
        }else if(moves == 8) {
            if (CheckIfPossibleFinishMove(true) == null) return true;
        }
        return false;
    }

    /// <summary>
    /// Check for win based on last move
    /// </summary>
    bool CheckForWin(Block block) {
        Block b;

        for (int i = 0; i < fieldSize; i++) {
            b = GetBlockAtPosition(new(block.Coordinates.x, i));
            if (b.MyState != block.MyState) break;
            if (i == fieldSize - 1) {
                //ROW WIN
                return true;
            }
        }

        for (int i = 0; i < fieldSize; i++) {
            b = GetBlockAtPosition(new(i, block.Coordinates.y));
            if (b.MyState != block.MyState) break;
            if (i == fieldSize - 1) {
                //COLUMN WIN
                return true;
            }
        }

        if (block.Coordinates.x == block.Coordinates.y) {
            for (int i = 0; i < fieldSize; i++) {
                b = GetBlockAtPosition(new(i, i));
                if (b.MyState != block.MyState) break;
                if (i == fieldSize - 1) {
                    //DIAGONAL WIN
                    return true;
                }
            }
        }

        if (block.Coordinates.x + block.Coordinates.y == fieldSize - 1) {
            for (int i = 0; i < fieldSize; i++) {
                b = GetBlockAtPosition(new(i, (fieldSize - 1) - i));
                if (b.MyState != block.MyState) break;
                if (i == fieldSize - 1) {
                    //ANTI DIAGONAL WIN
                    return true;
                }
            }
        }

        //unnecessary because of the "early tie" check
        if (moves == 9) {
            TieGame();
            return true;
        }

        return false;
    }


    /// <summary>
    /// Get all 3's and check them for a possible winning move
    /// Used by computer to win/block player and to determine early tie
    /// </summary>
    Block CheckIfPossibleFinishMove(bool player) {
        Block b = null;
        List<Block> blocks = new List<Block>();

        //rows
        for (int i = 0; i < fieldSize; i++) {
            if (b == null) {
                blocks.Clear();
                blocks.Add(GetBlockAtPosition(new(i, 0)));
                blocks.Add(GetBlockAtPosition(new(i, 1)));
                blocks.Add(GetBlockAtPosition(new(i, 2)));
                b = CheckListForFinishMove(blocks, player);
            } else return b;
        }

        //columns
        for (int i = 0; i < fieldSize; i++) {
            if (b == null) {
                blocks.Clear();
                blocks.Add(GetBlockAtPosition(new(0, i)));
                blocks.Add(GetBlockAtPosition(new(1, i)));
                blocks.Add(GetBlockAtPosition(new(2, i)));
                b = CheckListForFinishMove(blocks, player);
            } else return b;
        }
        //diag
        if (b == null) {
            blocks.Clear();
            blocks.Add(GetBlockAtPosition(new(0, 0)));
            blocks.Add(GetBlockAtPosition(new(1, 1)));
            blocks.Add(GetBlockAtPosition(new(2, 2)));
            b = CheckListForFinishMove(blocks, player);
        }

        //antidiag
        if (b == null) {
            blocks.Clear();
            blocks.Add(GetBlockAtPosition(new(0, 2)));
            blocks.Add(GetBlockAtPosition(new(1, 1)));
            blocks.Add(GetBlockAtPosition(new(2, 0)));
            b = CheckListForFinishMove(blocks, player);
        }
        return b;
    }

    /// <summary>
    /// check given array of blocks for a winning move
    /// </summary>
    Block CheckListForFinishMove(List<Block> threeBlocks, bool player) {
        int empty, x, o;
        empty = x = o = 0;
        Block available = null;
       
        foreach (var item in threeBlocks) {
            if (item.MyState == State.empty) {
                empty++;
                available = item;
            } else if (item.MyState == State.X) x++;
            else o++;
        }

        if (empty == 1 && x != o) {
            if (player) {
                if (x == 2) {
                    //winning move for player found
                    return available;
                } else return null;
            } else {
                if (o == 2) {
                    //winning move for computer found
                    return available;
                } else return null;
            }
        } else {
            return null;
        }
    }


    void TieGame() {
        gameOver = true;
        uiController.SetupPanel();
        uiController.SetHeaderText("ITS A TIE!");
    }

    void GameOver() {
        gameOver = true;
        uiController.SetupPanel();
        if (playersTurn) uiController.SetHeaderText("YOU WON!");
        else uiController.SetHeaderText("YOU LOST!");
    }

    Block GetBlockAtPosition(Vector2 pos) {
        Block b = null;
        foreach (var item in blocks) {
            if (item.Coordinates == pos) b = item;
        }
        return b;
    }

    IEnumerator Delay(System.Action callback, float time) {
        yield return new WaitForSeconds(time);
        callback();
    }
}
