// Pad.cs
// Owner: Joseph
// Layer: Feature, Arena (local)
//
// What this is
// A small tag MapBuilder puts on every pad disc, it remembers which cell the pad is in and its spawn rank
// Click a pad in the Scene view and the Inspector shows its coord and rank
// In the Scene view, spawn pads show "Spawn 1", "Spawn 2"... above them, so the ranks can be checked at a glance
// Own file because Unity needs one MonoBehaviour per file, with the same name
//
// Editor setup
//   none, MapBuilder adds it

using Game.Core;
using UnityEngine;

namespace Game
{
    public class Pad : MonoBehaviour
    {
        [SerializeField] private GridCoord _coord;
        [SerializeField] private int _spawnRank;

        public GridCoord Coord { get { return _coord; } }
        public int SpawnRank { get { return _spawnRank; } }

        // Called by MapBuilder right after making the pad
        // Store coord and spawnRank, name the object "Pad (x,Llevel,z)" using coord.ToString()
        // TODO(Joseph) [X] Completed
        public void Init(GridCoord coord, int spawnRank)
        {
            _coord = coord;
            _spawnRank = spawnRank;
            gameObject.name = $"Pad {coord}";
        }

#if UNITY_EDITOR
        // Editor only: draws "Spawn N" above spawn pads in the Scene view
        // If spawnRank is above 0: UnityEditor.Handles.Label(position + 0.5 m up, "Spawn " + spawnRank)
        // TODO(Joseph) [X] Completed
        private void OnDrawGizmos()
        {
            if (_spawnRank > 0)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "Spawn " + _spawnRank);
            }
        }
#endif
    }
}
