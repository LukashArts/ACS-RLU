using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigidBody;
    //public int thrownByPlayer;
    public Vector3 initialForce;
    public float explosionRadius = 1.5f;
    public float explosionDamage = 100f;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        ServerSend.SpawnProjectile(this);

        rigidBody.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.ProjectilePosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        //thrownByPlayer = _thrownByPlayer;
    }

    private void Explode()
    {
        //ServerSend.ProjectileExploded(this);

        Collider[] _colliders = Physics.OverlapSphere(transform.position, 3);
        foreach (Collider _collider in _colliders)
        {
            if (_collider.CompareTag("Player"))
                _collider.GetComponent<Player>().TakeDamage(explosionDamage);
            if (_collider.CompareTag("Bot"))
                _collider.GetComponent<Gatherer>().TakeDamage(explosionDamage);
        }

        //projectiles.Remove(id);
        //Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(3f);
        projectiles.Remove(id);
        Destroy(gameObject);
        Explode();
    }
}
