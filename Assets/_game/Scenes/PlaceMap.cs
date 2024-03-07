using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class PlaceMap : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField] private int _numberOfMaps;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _holePrefab;
    private List<Transform> _maps = new List<Transform>(0);

    [Button("Place")]
    private void Place()
    {
        InstantiateMaps();

        for(int i = 0; i < _maps.Count; i++)
        {
            float xPos = _target.position.x + _radius * Mathf.Cos(i * (2 * Mathf.PI / _numberOfMaps));
            float yPos = _target.position.z + _radius * Mathf.Sin(i * (2 * Mathf.PI / _numberOfMaps));
            _maps[i].position = new Vector3(xPos, 0, yPos);
            _maps[i].LookAt(_target, Vector3.up);
        }
    }

    private void Update()
    {
        foreach (Transform t in _maps)
        {
            t.LookAt(_target, Vector3.up);
        }
    }

    private void InstantiateMaps()
    {
        foreach (Transform child in _maps)
        {
            DestroyImmediate(child.gameObject);
        }

        _maps.Clear();

        for(int i = 0; i < _numberOfMaps; i++)
        {
            Transform newCube = Instantiate(_holePrefab,transform);
            _maps.Add(newCube);
        }
    }
}
