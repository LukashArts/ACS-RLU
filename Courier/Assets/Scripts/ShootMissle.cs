using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootMissle : MonoBehaviour
{
    private float nextActionTime = 0.0f;

    public GameObject Projectile;
    public float create_period = 1f;
    public Vector3 plus_missile_position;

    void Update()
    {
        if (Time.time > nextActionTime)
        {
            nextActionTime += create_period;
            ShootProjectileEveryFewSeconds();
        }
    }

    void ShootProjectileEveryFewSeconds()
    {
        var projectile_position = transform.position + plus_missile_position;
        GameObject bullet = Instantiate(Projectile, projectile_position, Quaternion.identity) as GameObject;
        var direction = transform.right + UnityEngine.Random.insideUnitSphere * 0.5f;
        bullet.GetComponent<Rigidbody>().AddForce(direction * 2500);
        Destroy(bullet, 3f);
    }
}
