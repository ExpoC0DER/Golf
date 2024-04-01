using System;
using System.Collections.Generic;
using System.Linq;
using _game.Scripts.Controls;
using Cinemachine;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace _game.Scripts
{
    public class Map : MonoBehaviour
    {
        [field: SerializeField] public CinemachineVirtualCamera MapCam { get; set; }
        [SerializeField] private List<Hole> _holes = new();
        [SerializeField] private float _radius;
        [SerializeField] private float _distance;
        [SerializeField] private int _numberOfPlayers;
        [SerializeField] private Transform _target;
        private List<List<Hole>> _maps = new();

        public int RoundsCount { get => _holes.Count; }

        public bool GetRoundStartLocation(int round, int playerId, out Transform startTransform)
        {
            if (round <= _holes.Count)
            {
                startTransform = _maps[round].First(hole => hole.PlayerId == playerId).StartLocation;
                return true;
            }
            startTransform = null;
            return false;
        }

        public void SetGrassColor(int playerId, Color color)
        {
            foreach (List<Hole> roundHoles in _maps)
            {
                foreach (Hole hole in roundHoles)
                {
                    if (hole.PlayerId == playerId)
                    {
                        hole.SetGrassColor(color);
                        break;
                    }
                }
            }
        }

        [Button("Place")]
        private void Place()
        {
            ClearMaps();

            for(int i = 0; i < _holes.Count; i++)
                InstantiateMaps(i);

            for(int i = 0; i < _maps.Count; i++)
            {
                for(int j = 0; j < _maps[i].Count; j++)
                {
                    float xPos = i * _distance + _radius * Mathf.Cos(j * (2 * Mathf.PI / _numberOfPlayers));
                    float yPos = _radius * Mathf.Sin(j * (2 * Mathf.PI / _numberOfPlayers));
                    _maps[i][j].transform.position = new Vector3(xPos, 0, yPos);
                    _maps[i][j].transform.LookAt(new Vector3(i * _distance, 0, 0), Vector3.up);
                }
            }
        }

        public void GenerateMap(Dictionary<int, Player> players)
        {
            int[] keys = players.Keys.ToArray();
            //_numberOfPlayers = players.Count;

            ClearMaps();

            for(int i = 0; i < _holes.Count; i++)
                InstantiateMaps(i);

            for(int i = 0; i < _maps.Count; i++)
            {
                for(int j = 0; j < _maps[i].Count; j++)
                {
                    float xPos = i * _distance + _radius * Mathf.Cos(j * (2 * Mathf.PI / _numberOfPlayers));
                    float yPos = _radius * Mathf.Sin(j * (2 * Mathf.PI / _numberOfPlayers));
                    _maps[i][j].transform.position = new Vector3(xPos, 0, yPos);
                    _maps[i][j].transform.LookAt(new Vector3(i * _distance, 0, 0), Vector3.up);
                    if (j < keys.Length)
                        _maps[i][j].PlayerId = keys[j];
                }
            }
        }

        private void ClearMaps()
        {
            foreach (List<Hole> roundHoles in _maps)
            {
                foreach (Hole hole in roundHoles)
                {
                    DestroyImmediate(hole.gameObject);
                }
            }

            _maps.Clear();
        }

        private void InstantiateMaps(int round)
        {
            List<Hole> newRoundHoles = new List<Hole>();
            for(int i = 0; i < _numberOfPlayers; i++)
            {
                Hole newHole = Instantiate(_holes[round], transform);
                newRoundHoles.Add(newHole);
            }
            _maps.Add(newRoundHoles);
        }

        private void Update() { MapCam.transform.parent.Rotate(Vector3.up, 10 * Time.deltaTime); }

        private void OnRoundStart(int round) { MapCam.transform.parent.position = new Vector3(round * _distance, 0, 0); }

        private void OnGamePhaseChanged(Enums.GamePhase gamePhase) { MapCam.m_Priority = gamePhase == Enums.GamePhase.Intermission ? 100 : 0; }

        private void OnEnable()
        {
            GameManager.OnRoundStart += OnRoundStart;
            GameManager.OnGamePhaseChanged += OnGamePhaseChanged;
        }

        private void OnDisable()
        {
            GameManager.OnRoundStart -= OnRoundStart;
            GameManager.OnGamePhaseChanged -= OnGamePhaseChanged;
        }
    }
}
