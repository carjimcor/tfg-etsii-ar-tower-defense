using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Upgrade : MonoBehaviour
{
    public AnimationCurve damage = default, range = default, shotsPerSecond = default;

    public float sellPercentage = 0.5f, upgradePercentage = 1.25f;

    public Transform[] turretUpgrades = default;
}
