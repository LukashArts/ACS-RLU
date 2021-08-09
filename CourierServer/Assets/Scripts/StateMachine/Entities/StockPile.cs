using System;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class StockPile : MonoBehaviour
{
    //[SerializeField] private int _maxHeld = 5;

    public string team;
    public Vector3 stockPilePosition = new Vector3(10, 1.1f, 6);
    public int _gathered;

    private void OnEnable()
    {
        _gathered = 0;
        Add();
    }

    public void Add()
    {
        _gathered++;
    }
}