using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{    
    public enum GameState
    {
        Start,
        ApproachStrawHouse,
        StrawHouse,
        WoodHouse,
        BrickHouse,
        End
    }

    public GameState CurrentState = GameState.Start;

    public string StrawHouseId = "strawHouse";
    public string WoodHouseId = "woodHouse";
    public string BrickHouseId = "brickHouse";

    public Transform FirstSpot;

    public int ActiveChapterId = 1;
    public static GameStateManager Instance;
    public List<Chapter> Chapters;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;            
        }
        else
        {
            Destroy(gameObject);
        }        
    }

    private void Start()
    {
        var cached = Resources.Load<GameStoryParamsAsset>("GameStoryParams");
        //get chapters
        Chapters = cached?.data?.chapters ?? new List<Chapter>();
    }
}
