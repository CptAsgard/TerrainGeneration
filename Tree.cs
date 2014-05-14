using UnityEngine;
using System.Collections;

/**
 * It's a tree
 */ 

public class Tree : MonoBehaviour {

    public Material TreeMaterial;

	// Use this for initialization
	void Start () {
	
        // Raycast to see if we aren't colliding with a ground piece. I only want to place
        // trees on grassy areas. If it hits a mountainous area, it won't spawn.
        // TODO: "chunk(Clone")" is a bad name to determine mountainous areas. Hardcoded and should definitely
        // be done differently. Tag compares are pretty slow. Any other way?
        RaycastHit hit;
        if( Physics.Raycast( transform.position, -Vector3.up, out hit, 1 << 8 ) )
        {
            if( hit.transform.name != "chunk(Clone)" )
                transform.Translate( new Vector3( 0, 5 - hit.distance, 0 ) );
            else
                return;
        }

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        meshRenderer.receiveShadows = false;

        Mesh mesh = meshFilter.sharedMesh;
        if( mesh == null ) {
            meshFilter.mesh = new Mesh();
            mesh = meshFilter.sharedMesh;
        }

        if( TreeMaterial )
            renderer.material = TreeMaterial;
        else
            renderer.material.color = Color.magenta;

        mesh.Clear();

        // Pyramid shape for trees
        Vector3 p0 = new Vector3( 0, 0, 0 );
        Vector3 p1 = new Vector3( 1, 0, 0 );
        Vector3 p2 = new Vector3( 1, 0, 1 );
        Vector3 p3 = new Vector3( 0, 0, 1 );
        Vector3 p4 = new Vector3( 0.5f, 1, 0.5f );

        mesh.vertices = new Vector3[] { 
            p1, p0, p4,
            p2, p1, p4,
            p3, p2, p4,
            p0, p3, p4,
            p0, p1, p3,
            p1, p2, p3
        };

        mesh.triangles = new int[] {
            0, 1, 2,
            3, 4, 5,
            6, 7, 8,
            9, 10, 11,
            12, 13, 14,
            15, 16, 17
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter( Collision col ) {
        Debug.Log( col.transform.name );
        if( col.transform.name == "chunk(Clone)" )
            Destroy( gameObject );
    }
}
