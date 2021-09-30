using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is responsible to sync the unity environment to the Model class
public class View
{
    private GameObject playerObject;
    private Dictionary<int,GameObject> playerObjectDict;

    private GameObject bombObject;
    private Dictionary<int,GameObject> bombObjectDict;
    
    private GameObject wallObject;
    private Dictionary<int, GameObject> wallObjectDict;
    
    private GameObject breakableObject;
    private Dictionary<int, GameObject> breakableObjectDict;

    private GameObject fireObject;
    private Dictionary<int, GameObject> fireObjectDict;
    
    // TEMPORARY VARIABLES
    private Dictionary<int, Bomb> tempDict;
    private List<int> idList;

    

    //private GameObject floorModel;

    //private GameObject destructibleEnvModel;
    
    // TO BE CONTINUED: ADD VISUAL MAP GENERATION
    public View(Dictionary<string, object> gameState, GameObject player, GameObject bomb, GameObject wall, GameObject breakable, GameObject fire)
    {
        playerObject = player;
        playerObjectDict = new Dictionary<int, GameObject>();

        bombObject = bomb;
        bombObjectDict = new Dictionary<int, GameObject>();
        
        wallObject = wall;
        wallObjectDict = new Dictionary<int, GameObject>();

        breakableObject = breakable;
        breakableObjectDict = new Dictionary<int, GameObject>();

        fireObject = fire;
        fireObjectDict = new Dictionary<int, GameObject>();
        
        // For each player from Model we instantiate a new Player model
        foreach (Player playerInfo in (IEnumerable) gameState["PlayersInfo"])
        {
            GameObject newPlayer = GameObject.Instantiate(playerObject);
            newPlayer.transform.position = new Vector3(playerInfo.position.x,0,playerInfo.position.y);
            newPlayer.name = playerInfo.playerID.ToString();
            playerObjectDict.Add(playerInfo.playerID,newPlayer);
            
        }
        
        // Visual Map generation
        Map temp = (Map) gameState["MapInfo"];
        for (int i=0;  i < 15; i++)
        {
            int padz = i;
            for (int j = 0;  j<15; j++)
            {
                int padx = j;
                
                if (temp.myMapLayout[i,j] == MapEnvironment.Wall)
                {
                    BlockFactory.Factory(wall, j,i);
                }
                else if (temp.myMapLayout[i,j] == MapEnvironment.Breakable)
                {
                    BlockFactory.Factory(breakable, j,i);
                }
            }
        }
    }
    
    // Update every model with the positions from Model
    public void UpdateView(Dictionary<string, object> gameState)
    {
        
        // Update Players Position
        foreach (Player playerInfo in (IEnumerable) gameState["PlayersInfo"])
        {
            playerObjectDict[playerInfo.playerID].transform.position =
                new Vector3(playerInfo.position.x, 0, playerInfo.position.y);
        }
        ///////////////////////////// TO BE MERGED BELOW //////////////////////////
        // Check for bombs to destroy and create explosions
        tempDict = gameState["BombsInfo"] as Dictionary<int, Bomb>;
        idList = new List<int>(bombObjectDict.Keys);
        foreach (int bombKey in idList)
        {
            
            if (!tempDict.ContainsKey(bombKey))
            {
                // VISUAL EXPLOSION HANDLED HERE
                GameObject.Destroy(bombObjectDict[bombKey]);
                bombObjectDict.Remove(bombKey);
            }
        }
        ////////////////////////////////////////////////////////////////////////
        
        // Update new bombs
        foreach (KeyValuePair<int,Bomb> bombItem in (IEnumerable) gameState["BombsInfo"])
        {
            // if new bomb add
            if (!bombObjectDict.ContainsKey(bombItem.Key))
            {
                GameObject newBomb = GameObject.Instantiate(bombObject);
                newBomb.transform.position = new Vector3(bombItem.Value.position.x, 0, bombItem.Value.position.y);
                bombObjectDict.Add(bombItem.Key,newBomb);
            }
            // if bombItem.exploding==true -> create fire at bombItem.explosionSquares[] and remove bomb gameobject
            // extra fire script to delete itself

            if (bombItem.Value.exploding == true)
            {
                BlockFactory.Factory(fireObject ,bombItem.Value.position.x, bombItem.Value.position.y);
            }
        }
        
        // Update dynamic environment here with gameState["MapInfo"]

    }
}
