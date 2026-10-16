using System.Collections.Generic;
using UnityEngine;

public class SnapPointsGrid : MonoBehaviour
{
    [Space]
    [Header("Initialize Snap Points Properties")]
    [SerializeField] private Transform _floorSize;
    [SerializeField] private Vector3 _gridSize;
    [SerializeField, Range(1f, 10f)] private int _YSnapPointsCount;

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}