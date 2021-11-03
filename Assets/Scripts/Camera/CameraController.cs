// CameraController.cs
// Controller for main camera, follows player to a set offset.
// Author:  Dan Blackford
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public int test;
    public float minSpeed;
    public float maxSpeed;
    public Vector2 minView;
    public Vector2 maxView;

    //Relative to player (make private)
    public Vector3 targetSet;  
    public Vector3 target;  
    public Vector3 offset;

    public float offsetZ = -20.0f;  //Z depth for camera

    public void SetTargetX(float x)
    {
        targetSet.x = x;
    }

    public void SetTargetY(float y)
    {
        targetSet.y = y;
    }

    public void OnStart(PlayerController player)
    {
        targetSet = new Vector3(0.0f, 0, offsetZ);
        target = targetSet;
        offset = new Vector3(target.x, target.y, offsetZ);
        transform.position = player.transform.position;
    }

    void Update()
    {
        PlayerController player = GameManager.instance.player;

        if (player == null) return;

        //Enforce camera limits
        Vector3 p = player.transform.position + offset;

        if (p.x > maxView.x) p.x = maxView.x;
        else if (p.x < minView.x) p.x = minView.x;

        if (p.y > maxView.y) p.y = maxView.y;
        else if (p.y < minView.y) p.y = minView.y;

        gameObject.transform.position = p;

        //Enforce target limits
        p = player.transform.position + targetSet;
 
        if (p.x > maxView.x) p.x = maxView.x;
        else if (p.x < minView.x) p.x = minView.x;

        if (p.y > maxView.y) p.y = maxView.y;
        else if (p.y < minView.y) p.y = minView.y;

        target = (p - player.transform.position);
    }

    void FixedUpdate()
    {
        PlayerController player = GameManager.instance.player;

        if (player == null) return;

        Vector3 p = player.transform.position;
        Vector3 c = gameObject.transform.position - p;
        Vector3 d = target - c; 

        //Move camera from current offset to target offset
        float d1 = target.magnitude;    //Rel. distance from player to target
        float d2 = d.magnitude;         //Rel. distance from camera to target
        float t = Time.deltaTime;

        if (d2 < (minSpeed * t))
        {
            //Camera is close enough, snap to target 
            offset = target;
        }
        else
        {
            //Move current offset smoothly towards target
            float r = (d1 > Mathf.Epsilon) ? Mathf.Sqrt(d2 / d1) : 0;

            d.Normalize();
            float speed = Mathf.Lerp(minSpeed, maxSpeed, r);
            Vector2 velocity = d * speed * t;

            offset += (Vector3)velocity;
        }
    }
}
