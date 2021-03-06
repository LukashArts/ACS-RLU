using System;
using UnityEngine;
using UnityEngine.AI;

public class Gatherer : MonoBehaviour
{
    public event Action<int> OnGatheredChanged;
    
    [SerializeField] private int _maxCarried = 20;
    
    private StateMachine _stateMachine;
    private int _gathered;
    
    public GatherableResource Target { get; set; }
    public StockPile StockPile { get; set; }

    public float pickUpDistance = 2f;
    public Transform Destination;

    private Transform carriedObject = null;
    private int pickupLayer;

    public bool isPicked = false;

    private void Awake()
    {
        pickupLayer = 1 << LayerMask.NameToLayer("Pickup");
        var navMeshAgent = GetComponent<NavMeshAgent>();
        //var animator = GetComponent<Animator>();
        //var enemyDetector = gameObject.AddComponent<EnemyDetector>();
        var fleeParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        
        _stateMachine = new StateMachine();

        var search = new SearchForResource(this);
        var moveToSelected = new MoveToSelectedResource(this, navMeshAgent);
        var harvest = new HarvestResource(this);
        var returnToStockpile = new ReturnToStockpile(this, navMeshAgent);
        var placeResourcesInStockpile = new PlaceResourcesInStockpile(this);
        //var flee = new Flee(this, navMeshAgent, enemyDetector, fleeParticleSystem);
        
        At(search, moveToSelected, HasTarget());
        At(moveToSelected, search, StuckForOverASecond());
        At(moveToSelected, harvest, ReachedResource());
        At(harvest, search, TargetIsDepletedAndICanCarryMore());
        At(harvest, returnToStockpile, InventoryFull());
        At(returnToStockpile, placeResourcesInStockpile, ReachedStockpile());
        At(placeResourcesInStockpile, search, () => _gathered == 0);
        //_stateMachine.AddAnyTransition(flee, () => enemyDetector.EnemyInRange);
        //At(flee, search, () => enemyDetector.EnemyInRange == false);
        
        _stateMachine.SetState(search);

        void At(IState to, IState from, Func<bool> condition) => _stateMachine.AddTransition(to, from, condition);
        Func<bool> HasTarget() => () => Target != null;
        Func<bool> StuckForOverASecond() => () => moveToSelected.TimeStuck > 0.5f;
        Func<bool> ReachedResource() => () => Target != null && 
                                              Vector3.Distance(transform.position, Target.transform.position) < 2f;
        
        Func<bool> TargetIsDepletedAndICanCarryMore() => () => (Target == null || Target.IsDepleted) && !InventoryFull().Invoke();
        Func<bool> InventoryFull() => () => _gathered >= _maxCarried;
        Func<bool> ReachedStockpile() => () => StockPile != null && 
                                               Vector3.Distance(transform.position, StockPile.stockPilePosition) < 2f;
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
            //carriedObject.GetComponent<Rigidbody>().AddForce(this.gameObject.GetComponent<CharacterController>().velocity * 40);
            carriedObject.GetComponent<Rigidbody>().useGravity = true;
            carriedObject.GetComponent<BoxCollider>().enabled = true;
            carriedObject = null;
            isPicked = false;
        }
    }
}