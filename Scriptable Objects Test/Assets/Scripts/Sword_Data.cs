using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Data : ScriptableObject {

  [SerializeField]
  private string swordName;
  [SerializeField]
  private string description;
  [SerializeField]
  private Sprite icon;
  [SerializeField]
  private int goldCost;
  [SerializeField]
  private int attackDamage;
}
