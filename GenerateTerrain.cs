using UnityEngine;
using UnityEditor;
using System.Collections;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

/**
 * This class is the meat of the project
 * It'll generated two layers, a mountain layer and a grass layer.
 * This class is handled by 
 */
public class GenerateTerrain : MonoBehaviour {
    public Material landMaterial;   // Material to coat the generated land with
    public Object Grass;            // Grass object to spawn
    public GameObject GrassParent;  // Parent to child the grass to
    public GameObject Tree;         // .. Tree

    public bool Generated;

    public Vector3 Size = new Vector3( 200, 30, 200 );  // Size in Unity units of the generated land
    public int Width = 100; // Amount of points to fill the mesh
    public int Height = 100;

    public float offsetX;
    public float offsetY;

    [SerializeField]
    private float    yIntensity  = 0.3F;     // Intensity of randomization on the Y axis
    [SerializeField]
    private float    amplitude   = 8.0F;    // Scales the noise height
    [SerializeField]
    private float    frequency   = 0.1F;    // How "zoomed" in you are. This usually is a very small number.
    [SerializeField]
    private int      octaves     = 8;        // Number of iterations
    [SerializeField]
    private float    persistence = 0.5F;     // How much each octave contributes to the total height
    [SerializeField]
    private int      seed        = 10;        // Generating seed

    private Mesh mesh; 
    private ex.FractalNoise noise;  // Object of FractalNoise class, namespaced in ex

    /**
     * This function generates the heightmap and makes the vertices.
     * Normals are calculated by the MakeVertsUnique() function.
     * Calling this function is required, so there's no need to be save about executing the 
     * RecalculateNormals() function
     */
    public void GenerateHeightmap() {

        noise = new ex.FractalNoise();
        noise.SetNoise( offsetX, offsetY, seed );

        mesh = GetComponent<MeshFilter>().mesh;
        if( !mesh ) {
            Debug.Log( "Can't find mesh!" );
            return;
        }

        if( landMaterial )
            renderer.material = landMaterial;
        else
            renderer.material.color = Color.magenta;

	    Vector3[] vertices = new Vector3[Height * Width];
        Vector2[] uv = new Vector2[Height * Width];

        Vector2 uvScale = new Vector2( 1.0f / ( Width - 1 ), 1.0f / ( Height - 1 ) );
        Vector3 sizeScale = new Vector3( Size.x / ( Width - 1 ), Size.y, Size.z / ( Height - 1 ) );
	
        int y = 0;
	    int x = 0;
	    for( y = 0; y < Height; y++ ) {
		    for( x = 0; x < Width; x++ ) {

                Vector3 vertex = new Vector3( x, noise.GenerateFractal( amplitude, frequency, octaves, (float) x, (float) y, persistence ), y );
                vertex.y += noise.GeneratePerlin( (float) x, (float) y ) * yIntensity;

			    vertices[y*Width + x] = Vector3.Scale( sizeScale, vertex ); // Sets the vertex position on the grid
                uv[y * Width + x] = Vector2.Scale( new Vector2( x, y ), uvScale );

                if( y < Height - 1 && y > 0 && x < Width - 1 && x > 0 ) {       // Don't randomize outer bounds of node because of (future) tiling
                    vertices[y * Width + x].x += Random.Range( -1.0f, 1.0f ) * yIntensity;
                    vertices[y * Width + x].z += Random.Range( -1.0f, 1.0f ) * yIntensity;
                }
		    }
	    }

	    // Build triangle indices: 3 indices into vertex array for each triangle
	    int[] triangles = new int[(Height - 1) * (Width - 1) * 6];
	    int index = 0;
	    for( y = 0; y < Height-1; y++ ) {
		    for( x = 0; x < Width-1; x++ ) {
			    // For each grid cell output two triangles
			    triangles[index++] = (y     * Width) + x;
			    triangles[index++] = ((y+1) * Width) + x;
			    triangles[index++] = (y     * Width) + x + 1;

			    triangles[index++] = ((y+1) * Width) + x;
			    triangles[index++] = ((y+1) * Width) + x + 1;
			    triangles[index++] = (y     * Width) + x + 1;
		    }
	    }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uv;
	    mesh.triangles = triangles;

        MakeVertsUnique();
    }

    /**
     * This function goes over each vertex very inefficiently.
     * I don't know of any other way yet
     */
    private void MakeVertsUnique() {

        int[] triangles = mesh.triangles;
        Vector3[] vertices = mesh.vertices;
        int[] trianglesNew = new int[triangles.Length];
        Vector3[] verticesNew = new Vector3[triangles.Length];

        for( int i = 0; i < trianglesNew.Length; i++ ) {
            Vector3 v3Pos = vertices[triangles[i]];
            trianglesNew[i] = i;
            verticesNew[i] = v3Pos;
        }

        mesh.vertices = verticesNew;
        mesh.triangles = trianglesNew;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;

        Generated = true;
    }

    /**
     * Starts the generation of grass. Mountains are done first.
     * Starts a coroutine for generating trees
     * @see GenerateTerrain.SpawnTrees()
     */
    public void SpawnGrass() {

        var _grass = Object.Instantiate( Grass, new Vector3( transform.position.x, transform.position.y + 40, transform.position.z ), Quaternion.identity ) as GameObject;
        _grass.transform.parent = GrassParent.transform;

        var generateTerrain = _grass.GetComponent<GenerateTerrain>();

        generateTerrain.offsetX = offsetX;
        generateTerrain.offsetY = offsetY;

        generateTerrain.GenerateHeightmap();
        StartCoroutine( generateTerrain.SpawnTrees() );
    }

    /**
     * Spawns simple trees
     * @see Tree
     */
    public IEnumerator SpawnTrees() {
        var treeParent = new GameObject();
        treeParent.name = "- Trees";
        treeParent.transform.parent = gameObject.transform;

        for( int y = 0; y < Height; y++ ) {
            for( int x = 0; x < Width; x++ ) {
                if( noise.GenerateFractal( 8, 0.1f, 8, (float) x + 550, (float) y + 550, 0.5f ) > 9.8f ) { // Use perlin to spawn trees in areas above a certain threshold.
                    var _tree = GameObject.Instantiate( Tree, new Vector3( transform.position.x + x * Width, 500, transform.position.z + y * Height ), Quaternion.identity ) as GameObject;
                    _tree.transform.parent = treeParent.transform;
                }
            }
        }
        yield return null;

    }
}