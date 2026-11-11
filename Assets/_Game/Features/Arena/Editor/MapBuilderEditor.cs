// MapBuilderEditor.cs
// Owner: Joseph
// Layer: Editor only (in an Editor folder, never part of the game build)
//
// What this is
// Adds four buttons to MapBuilder's Inspector, so maps can be loaded, built and checked without pressing Play
//   Import map JSON: copies a map file into the map asset
//   Build: builds the arena in the scene so you can look at it
//   Clear: removes the built arena
//   Validate: runs MapValidator and prints every error, warning and info line in the Console
//
// Map files
// A map is written as a JSON text file in Assets/_Game/Data/Maps (e.g. Map_Test.json), it is MapLayout written out as JSON
// Maps are data, not clicked in by hand: the file is written from a sketch or by a generator
// Text files go in git like code, so a map change is reviewed like any other change
//
// Editor setup
//   none, the buttons appear on the MapBuilder Inspector by themselves

using Game.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game
{
    [CustomEditor(typeof(MapBuilder))]
    public class MapBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            MapBuilder builder = (MapBuilder)target;

            // Import: JsonUtility.FromJsonOverwrite(builder.MapJson.text, builder.Map.Layout)
            // then EditorUtility.SetDirty(builder.Map) and AssetDatabase.SaveAssets() so the map asset keeps it
            // We need to make sure Map and Map Json are both assigned, log an error and stop otherwise
            // TODO(Joseph)
            if (GUILayout.Button("Import map JSON"))
            {
            }

            // Build: builder.Build(), then EditorSceneManager.MarkSceneDirty(builder.gameObject.scene)
            // TODO(Joseph)
            if (GUILayout.Button("Build"))
            {
            }

            // Clear: builder.Clear(), then mark the scene dirty the same way
            // TODO(Joseph)
            if (GUILayout.Button("Clear"))
            {
            }

            // Validate:
            //   movesets: MovesetFactory.Create for Cluster, Ring and Ray
            //   geometry: builder.Map.Geometry(builder.Settings.StandardEyeHeight)
            //   report = MapValidator.Validate(builder.Map.Layout, movesets, geometry)
            //   Debug.LogError for each error, Debug.LogWarning for each warning, Debug.Log for each info line
            //   finish with "Map OK" or "Map has N errors"
            // TODO(Joseph)
            if (GUILayout.Button("Validate"))
            {
            }
        }
    }
}
