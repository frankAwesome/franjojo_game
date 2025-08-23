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

    public static GameStateManager Instance;

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
}
