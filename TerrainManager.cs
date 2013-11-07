/*
 * Will be responsible for generating terrain
 * within the user's view.
 * Terrain generator will create chunk and 
 * decide amplitude and frequency.
 * When a chunk gets into view, it should generate the 
 * chunks around the view.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainManager : MonoBehaviour {

    public Object chunk;
    public int perlinOffset = 9;
    public Vector2 fieldSize = new Vector2( 10, 10 );

    public Transform cameraPosition;

    private GameObject chunkParent;
    private GameObject grassParent;

    // Generated tiles are stored in this list.
    // I sample around the camera, and see if the tiles that need to be
    // generated exist already. If they don't exist, I generate the tile.
    private Dictionary<string, GameObject> terrainList;

	// Use this for initialization
	void Start () {
        chunkParent = new GameObject();
        chunkParent.name = "- Chunks";

        grassParent = new GameObject();
        grassParent.name = "- Grass";

        terrainList = new Dictionary<string,GameObject>();

	    StartCoroutine( GenerateChunk( 0, 0 ) );
	}
	
	// Update is called once per frame
	void Update () {
        GenerateChunksAroundCamera();
	}

    IEnumerator GenerateChunk( int x, int y ) {
        var _chunk = Object.Instantiate( chunk ) as GameObject;
        var generateTerrain = _chunk.GetComponent<GenerateTerrain>();

        _chunk.transform.position = new Vector3( x * generateTerrain.Size.x, 0, y * generateTerrain.Size.z );
        _chunk.transform.parent = chunkParent.transform;

        generateTerrain.offsetX = perlinOffset * x;
        generateTerrain.offsetY = perlinOffset * y;

        generateTerrain.GenerateHeightmap();

        generateTerrain.GrassParent = grassParent;

        generateTerrain.SpawnGrass();

        terrainList.Add( x.ToString() + y.ToString(), _chunk );

        yield return null;
    }

    void GenerateChunksAroundCamera() {
        // Albeit it a bit ugly (very ugly), this loop goes from -6 to +6 chunks around the camera. 
        // So, the raycasts are relative to the camera's position.
        for( int x = -6 + Mathf.FloorToInt( cameraPosition.position.x / 400 ); x < 6 + Mathf.FloorToInt( cameraPosition.position.x / 400 ); x++ ) {
            for( int y = -6 + Mathf.FloorToInt( cameraPosition.position.z / 400 ); y < 6 + Mathf.FloorToInt( cameraPosition.position.z / 400 ); y++ ) {
                FindTerrainUnder( new Vector3( cameraPosition.position.x + x * 400, cameraPosition.position.y, cameraPosition.position.z + y * 400 ), x, y ); 
            }
        }
    }

    void FindTerrainUnder( Vector3 location, int x, int y ) {
        if( !Physics.Raycast( location, new Vector3( location.x, -location.y, location.z ), 1 << 8 ) ) {
            if( !terrainList.ContainsKey( x.ToString() + y.ToString() ) ) {
                StartCoroutine( GenerateChunk( x, y ) );
            }
        }
    }
}
