using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public float speed = 10.0f;
	
	// Update is called once per frame
	void Update () {
        float xSpeed = Input.GetAxis( "Horizontal" );
        float ySpeed = Input.GetAxis( "Vertical" );

        transform.Translate( Vector3.forward * ySpeed * speed * Time.deltaTime );
        transform.Translate( Vector3.right * xSpeed * speed * Time.deltaTime );

        RaycastHit hit;
        if( Physics.Raycast( transform.position, -Vector3.up, out hit ) ) {
            transform.Translate( new Vector3( 0, 150 - hit.distance, 0 ) );
        }
	}
}