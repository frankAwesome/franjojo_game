using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public static List<House> Houses = new List<House>();
    public bool IsDestroyed = false;
    public string HouseId;

    public GameObject HouseDoorPosition;

    public GameObject DestroyedGameobjectPrefab;

    public static House FindHouseById(string houseId)
    {
        foreach (var house in Houses)
        {
            if (house.HouseId == houseId)
            {
                return house;
            }
        }
        return null;
    }

    private void OnEnable()
    {        
        if (!Houses.Contains(this))
        {
            Houses.Add(this);
        }
    }

    private void OnDisable()
    {
        if (Houses.Contains(this))
        {
            Houses.Remove(this);
        }        
    }
}
