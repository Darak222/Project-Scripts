using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRace", menuName = "Units/NewRace", order = 1)]
public class UnitsInRace : ScriptableObject
{
    public SpawnableUnit meleeUnit;
    public SpawnableUnit rangedUnit;
    public SpawnableUnit mageUnit;
    public SpawnableUnit riderUnit;
    public SpawnableUnit siegeUnit;
}

