using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public enum GizmoType { Never, SelectedOnly, Always }

    public Fish prefab;
    public float spawnRadius = 1;
    public int spawnCount = 1;
    public Color color;
    public GizmoType showSpawnRegion;

    void Awake () {
        for (int i = 0; i < spawnCount; i++) {
            Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
            Fish fish = Instantiate (prefab);
            fish.transform.position = pos;
            fish.transform.forward = Random.insideUnitSphere;
        }
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {
        Gizmos.color = new Color (color.r, color.g, color.b, 0.3f);
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }
}
