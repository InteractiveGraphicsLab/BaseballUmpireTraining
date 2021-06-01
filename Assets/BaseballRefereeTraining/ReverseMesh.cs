using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ReverseMesh : MonoBehaviour
{
    [SerializeField] GameObject avater;

    // Start is called before the first frame update
    void Start()
    {
        var renderers = avater.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var renderer in renderers)
        {
            var mesh = renderer.sharedMesh;
            mesh.vertices = mesh.vertices.Select(v => new Vector3(-v.x, v.y, -v.z)).ToArray();
            mesh.triangles = mesh.triangles.Reverse().ToArray();
            mesh.normals = mesh.normals.Select(n => new Vector3(-n.x, n.y, -n.z)).ToArray();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
