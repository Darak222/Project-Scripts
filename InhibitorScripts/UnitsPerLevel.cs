using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level0", menuName = "Units/NewLevel", order = 2)]
public class UnitsPerLevel : ScriptableObject
{
    public int meleeUnits = 3;
    public int rangedUnits = 3;
    public int mageUnits = 1;
    public int riderUnits = 2;
    public int siegeUnits = 1;
}
