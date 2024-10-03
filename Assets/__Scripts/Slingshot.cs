using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    // Fields set in the Unity Inspector pane
    [Header("Inscribed")]
    public GameObject projectilePrefab;
    public float velocityMult = 10f;
    public GameObject projLinePrefab;

    // Fields set dynamically
    [Header("Dynamic")]
    public GameObject launchPoint;
    public Vector3 launchPos;
    public GameObject projectile;
    public bool aimingMode;

    //LineRenderer
    private LineRenderer lineRenderer;


    void Awake() {
        Transform launchPointTrans = transform.Find("LaunchPoint");
        launchPoint = launchPointTrans.gameObject;
        launchPoint.SetActive( false );
        launchPos = launchPointTrans.position;

        //Initialize line renderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false; //Disables it initially
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;


    }

    void OnMouseEnter() {
        //print( "Slingshot:OnMouseEnter()" );
        launchPoint.SetActive( true );
    }

    void OnMouseExit() {
        //print( "Slingshot:OnMouseExit()" );
        launchPoint.SetActive( false );
    }

    void OnMouseDown() {
        //The player has pressed the mouse button while over slingshot
        aimingMode = true;
        //Instantiate a projectile
        projectile = Instantiate( projectilePrefab ) as GameObject;
        //Start it at the launch point
        projectile.transform.position = launchPos;
        //Set it to isKinematic for now
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        //Enables line renderer when aiming starts
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, launchPos);
    }

    void Update() {
        //If slingshot is not in aimingMode, don't run this code
        if(!aimingMode) return;

        //Get the current mouse position in 2D screen cordinates
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint( mousePos2D );

        //Find the delta from the lauchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D -launchPos;
        
        //Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if( mouseDelta.magnitude > maxMagnitude ) {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        //Move the projectile to this new position
        Vector3 projPos = launchPos + mouseDelta;
        projectile.transform.position = projPos;

        //Update line renderer end point to follow mouse position
        lineRenderer.SetPosition(1, projPos);

        if( Input.GetMouseButtonUp(0) ) { //This 0 is a zero, not an o
            //The mouse has been released
            aimingMode = false;
            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * velocityMult;

            //Switch to slingshot view immediately before setting POI
            FollowCam.SWITCH_VIEW( FollowCam.eView.slingshot );

            FollowCam.POI = projectile; //Set the _MainCamera POI
            //Add a projectile line to the projectile
            Instantiate<GameObject>(projLinePrefab, projectile.transform);
            projectile = null;
            MissionDemolition.SHOT_FIRED();

            //Disable line renderer
            lineRenderer.enabled = false;
        }
    }

}
