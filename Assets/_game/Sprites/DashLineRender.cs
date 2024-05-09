using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer)),ExecuteAlways]
public class DashLineRender : MonoBehaviour
{
    private LineRenderer _lineRenderer;
    [SerializeField] private float _rep = 1f;
    [SerializeField] private float _w = 1f;

    private void Awake() { _lineRenderer = GetComponent<LineRenderer>(); }

    private void Update()
    {
        // width is the width of the line
        float width = _lineRenderer.startWidth;
        _lineRenderer.material.mainTextureScale = new Vector2(_rep / width, _w);
        // 1/width is the repetition of the texture per unit (thus you can also do doublelines)
    }
}
