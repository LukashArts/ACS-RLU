using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Gatherer : MonoBehaviour
{
    public event Action<int> OnGatheredChanged;

    public static int next_id = 1;
    [SerializeField] private int _maxCarried = 20;

    public int id;
    public string team;
    public float health;
    public float maxHealth = 100f;
    public MeshRenderer model;

    private StateMachine _stateMachine;
    private int _gathered;

    public GatherableResource Target { get; set; }
    public StockPile StockPile { get; set; }

    public float pickUpDistance = 2f;
    public Transform Destination;

    private Transform carriedObject = null;
    private int pickupLayer;

    public bool isPicked = false;
    public NavMeshAgent navMeshAgent;
    public TextMeshPro AgentState;
    public TextMeshPro AgentStatePerc;

    private void Awake()
    {
        id = next_id + 100;
        next_id++;
        health = 100;

        pickupLayer = 1 << LayerMask.NameToLayer("Pickup");
        navMeshAgent = GetComponent<NavMeshAgent>();
        //var animator = GetComponent<Animator>();
        //var enemyDetector = gameObject.AddComponent<EnemyDetector>();
        //var fleeParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();

        _stateMachine = new StateMachine();

        var search = new SearchForResource(this);
        var moveToSelected = new MoveToSelectedResource(this, navMeshAgent);
        var collect = new HarvestResource(this);
        var goToDestination = new ReturnToStockpile(this, navMeshAgent);
        var placeResources = new PlaceResourcesInStockpile(this);
        //var flee = new Flee(this, navMeshAgent, enemyDetector, fleeParticleSystem);
        var dead = new Die(this, navMeshAgent);

        At(search, moveToSelected, HasTarget());
        At(moveToSelected, search, StuckForOverAHalfSecond());
        At(moveToSelected, collect, ReachedResource());
        At(collect, search, CheckForItem());
        At(collect, goToDestination, PickedItem());
        At(goToDestination, placeResources, ReachedDestination());
        At(placeResources, search, () => _gathered == 0);
        _stateMachine.AddAnyTransition(dead, () => this.health <= 0);
        At(dead, search, HasHealth());
        //_stateMachine.AddAnyTransition(flee, () => enemyDetector.EnemyInRange);
        //At(flee, search, () => enemyDetector.EnemyInRange == false);

        _stateMachine.SetState(search);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> HasTarget() => () => Target != null;
        Func<bool> StuckForOverAHalfSecond() => () => moveToSelected.TimeStuck > .5f;
        Func<bool> ReachedResource() => () => Target != null &&
                                              Vector3.Distance(transform.position, Target.transform.position) < 2f;

        Func<bool> CheckForItem() => () => (Target == null || Target.IsDepleted) && !PickedItem().Invoke();
        Func<bool> PickedItem() => () => _gathered >= _maxCarried && isPicked == true;
        Func<bool> ReachedDestination() => () => StockPile != null &&
                                               Vector3.Distance(transform.position, StockPile.stockPilePosition) < 2f;
        Func<bool> HasHealth() => () => this.health > 0;
    }

    private void Update()
    {
        _stateMachine.Tick();
    }

    private void FixedUpdate()
    {
        if (carriedObject != null)
        {
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.rotation = Destination.rotation;
            carriedObject.position = Destination.position;
        }

        ServerSend.BotPosition(this);
        ServerSend.BotRotation(this);
    }

    public void TakeFromTarget()
    {
        if (Target.Take())
        {
            PickUp();
            _gathered++;
            OnGatheredChanged?.Invoke(_gathered);
        }
    }

    public bool Take()
    {
        if (_gathered <= 0)
            return false;

        _gathered--;
        OnGatheredChanged?.Invoke(_gathered);
        return true;
    }

    public void DropAllResources()
    {
        if (_gathered > 0)
        {
            Drop();
            //FindObjectOfType<WoodDropper>().Drop(_gathered, transform.position);
            _gathered = 0;
            OnGatheredChanged?.Invoke(_gathered);
        }
    }

    private void PickUp()
    {
        // Collect every pickups around. Make sure they have a collider and the layer Pickup
        Collider[] pickups = Physics.OverlapSphere(transform.position, pickUpDistance, pickupLayer);

        // Find the closest
        float dist = Mathf.Infinity;
        for (int i = 0; i < pickups.Length; i++)
        {
            float newDist = (transform.position - pickups[i].transform.position).sqrMagnitude;
            if (newDist < dist)
            {
                carriedObject = pickups[i].transform;
                dist = newDist;
                isPicked = true;
            }
        }

        if (carriedObject != null) // Check if we found something
        {
            // Set the box in front of character
            carriedObject.GetComponent<BoxCollider>().enabled = false;
            carriedObject.GetComponent<Rigidbody>().useGravity = false;
            carriedObject.rotation = Destination.rotation;
            carriedObject.position = Destination.position;
        }
    }

    public void Drop()
    {
        //PlayerMovement.DropObjects = false;
        if (carriedObject != null)
        {
            carriedObject.GetComponent<GatherableResource>().SetAvailable(1);
            //carriedObject.GetComponent<Rigidbody>().AddForce(this.gameObject.GetComponent<CharacterController>().velocity * 40);
            carriedObject.GetComponent<Rigidbody>().useGravity = true;
            carriedObject.GetComponent<BoxCollider>().enabled = true;
            carriedObject = null;
            isPicked = false;
        }
    }

    public void TakeDamage(float _damage)
    {
        if (health <= 0f)
            return;

        health -= _damage;
        if (health <= 0f)
        {
            //this.enabled = false;
            //health = 0f;
            //_gathered = 0;
            //RespawnPlayerAtPosition();
            //Drop();
            //ServerSend.BotPosition(this);
            //StartCoroutine(Respawn());
        }
    }

    public void RespawnPlayerAtPosition()
    {
        if (team == "blue")
        {
            var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_blue);
            var keys = temp_dict.Keys;
            foreach (var key in keys)
                if (Server.spawn_points_blue[key] == 100 + id)
                    transform.position = key;
        }
        else if (team == "red")
        {
            var temp_dict = new Dictionary<Vector3, int>(Server.spawn_points_red);
            var keys = temp_dict.Keys;
            foreach (var key in keys)
                if (Server.spawn_points_red[key] == 100 + id)
                    transform.position = key;
        }
    }

    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2f);
        this.enabled = true;
        this.model.enabled = true;
        health = 100;
        ServerSend.BotHealth(this);
    }
}