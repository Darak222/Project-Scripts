using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spellcaster : MonoBehaviour
{
    public void UseSpell(Spell spell){
        Spell usedSpell = Instantiate(spell, transform);
        usedSpell.tag = this.tag;
        usedSpell.attacker = GetComponent<Attacking>();
    }
}
