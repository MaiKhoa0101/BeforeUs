using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopulationManagement : MonoBehaviour
{
    public Text Population;

    public int NumberOfMob;
    public int maxPop;
    [ContextMenu("Sinh ra mob")]
    void IncreasePop()
    {
        NumberOfMob += 1;
        Population.text = NumberOfMob.ToString();
    }

    [ContextMenu("Hu? mob")]
    void DecreasePop()
    {
        NumberOfMob -= 1;
        Population.text = NumberOfMob.ToString();
    }
}
