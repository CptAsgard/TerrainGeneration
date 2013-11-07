using UnityEngine;
using System.Collections;

public class LoadMainLevel : MonoBehaviour {

    IEnumerator Start() {
        Debug.Log( "Loading level" );
        AsyncOperation async = Application.LoadLevelAsync( "level" );
        yield return async;
        Debug.Log( "Loading complete" );
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
