using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIPatrolMovement : MonoBehaviour
{
    #region Variables
    //Movement Variables
    [Header("Movement")]
    public float forageSpeed = 1.5f;
    public float fleeSpeed = 2f;
    public float runOrChase = -1f;
    //Distance Variables
    [Header("Distances")]
    public float minDistanceToWaypoint = 0.1f;
    public float distanceToPlayer = 3f;
    [Tooltip("Add the game object that represents the player")]
    public Transform player;    
    //Variable array to contain waypoints for AI destinations
    [Header("Waypoints")]
    [Tooltip("Add the scene's game objects to use as your waypoints")]
    public Transform[] foragePoints;
    public int forageIndex = 0;
    //State Variables (No header as nothing to show in Unity)    
    private _patrolState currentPatState;
    private enum _patrolState
    {
        Patrolling,
        Attacking,
        Fleeing,
    }
    //Sprite variables
    [Header("Sprites")]
    [Tooltip("Add the sprite from your assets that corresponds to the Patrolling state")]
    [SerializeField] private Sprite _aiPatrolSprite;
    [Tooltip("Add the sprite from your assets that corresponds to the Fleeing state")]
    [SerializeField] private Sprite _aiFleeSprite;
    [Tooltip("Add the sprite from your assets that corresponds to the Chasing state")]
    [SerializeField] private Sprite _aiChaseSprite;
    private SpriteRenderer _aiSpriteRenderer;
    //UI Variables
    [Header("User Interface")]
    [Tooltip("Add the PatrolText objext from Label01 on the StatesDisplay canvas")]
    [SerializeField] private Text _patrolText;
    [Tooltip("Add the MenuText object from the RestartMenu canvas")]
    [SerializeField] private Text _menuText;
    [Tooltip("Add the RestartMenu canvas object")]
    [SerializeField] private GameObject _resetMenu;
    //Boolean variable to enable closest waypoint script to only run once
    private bool _runCounter = false;
    //Variable to store sword game object to enable us to change way AI reacts to player based on player having weapon
    [Tooltip("Add the PlayerSword object that is a child of the player object")]
    [SerializeField]private GameObject _SwordCheck;
    #endregion

    void Start()
    {
        #region StartupVariables
        //Get renderer component to enable sprite swapping
        _aiSpriteRenderer = GetComponent<SpriteRenderer>();
        //Set movement direction variable to store current position to be manipulated later
        Vector2 patrolMoveDir = transform.position;
        //Run Patrol State selection method to get AI to react according to it's currently selected state (Default Patrolling)
        PatrolState();
        #endregion
    }
    #region Movement
    //Method to determine closest waypoint
    private void ClosestPatWaypoint()
    {
        //Declare variable to set closest distance and default it to a large value so it won't overrule any waypoints
        float lowestPatDistance = float.PositiveInfinity;
        //Default lowest distance is distance of first waypoint on list
        int lowestPatIndex = 0;
        //Declare a variable to store the distance to the waypoint currently being checked
        float patDistance;
        //For every waypoint on list determine its distance and check it against current lowest distance, if it is smaller new lowest distance is set
        for (int i = 0; i < foragePoints.Length; i++)
        {
            patDistance = Vector2.Distance(player.position, foragePoints[i].position);
            if (patDistance < lowestPatDistance)
            {
                lowestPatDistance = patDistance;
                lowestPatIndex = i;
            }
        }
        //Set new destination to waypoint we have determined as closest
        forageIndex = lowestPatIndex;
    }
    //Method to set destination to next waypoint after reaching current destination
    private void WaypointPatUpdate()
    {
        //When AI has reached location of current destination increment the index by 1 to select next waypoint on list as destination
        if (Vector2.Distance(transform.position, foragePoints[forageIndex].position) < minDistanceToWaypoint)
        {
            forageIndex++;
            //If we have reached end of list, set first waypoint as destination
            if (forageIndex >= foragePoints.Length)
            {
                forageIndex = 0;
            }
        }
    }
    //Method to move towards destination when patrolling
    private void PatrolMoveTowards()
    {
        //If we are far enough away from destination, move towards it
        if (Vector2.Distance(transform.position, foragePoints[forageIndex].position) > minDistanceToWaypoint)
        {
            Vector2 directionToGoal = (foragePoints[forageIndex].position - transform.position);
            directionToGoal.Normalize();
            transform.position += (Vector3)directionToGoal * forageSpeed * Time.deltaTime;            
        }
        //If we are closer than determined distance set our position to the same as the waypoints location to enable us to select next waypoint
        else
        {
            transform.position = foragePoints[forageIndex].position;
        }
    }
    //Method to move AI away from or towards player depending on value of runorchase variable
    private void PatrolFlee()
    {
        //If player is still too close move away from player
        if (Vector2.Distance(transform.position, player.position) < distanceToPlayer)
        {
            Vector2 directionToGoal = ((player.position - transform.position) * runOrChase);
            directionToGoal.Normalize();
            transform.position += (Vector3)directionToGoal * forageSpeed * Time.deltaTime;
        }
    }
    #endregion
    #region StateMachine
    //State selection method to declare coroutines to run for each state and run them when that state is selected (Default is patrolling)
    private void PatrolState()
    {
        switch (currentPatState)
        {
            case _patrolState.Patrolling:
                StartCoroutine(PatrollingState());
                break;
            case _patrolState.Attacking:
                StartCoroutine(AttackState());
                break;
            case _patrolState.Fleeing:
                StartCoroutine(FleeingState());
                break;
            default:
                currentPatState = _patrolState.Patrolling;
                break;
        }
    }
    //Coroutine for AI when it is patrolling
    private IEnumerator PatrollingState()
    {
        //Change sprite to patrolling sprite
        _aiSpriteRenderer.sprite = _aiPatrolSprite;
        //If we haven't already determined closest waypoint, run the closest waypoint method then declare that we have run the calculation
        if (_runCounter == false)
        {
            ClosestPatWaypoint();
            _runCounter = true;
        }
        //While we are still patrolling
        while (currentPatState == _patrolState.Patrolling)
        {
            //Change UI text to reflect that we are patrolling
            _patrolText.text = "Patrolling";
            //If we are not close enough to player, determine next waypoint and keep patrolling
            if (Vector2.Distance(transform.position, player.position) > distanceToPlayer)
            {
                WaypointPatUpdate();
                PatrolMoveTowards();
            }
            //If player gets too close change our state to attacking
            else
            {
                currentPatState = _patrolState.Attacking;                                                
            }
            yield return null;
        }
        //Return to state selection method to transfer to new coroutine for new state
        PatrolState();        
    }
    //Coroutine for AI when it is attacking
    private IEnumerator AttackState()
    {
        //Change sprite to the attacking sprite
        _aiSpriteRenderer.sprite = _aiChaseSprite;
        //While AI is still attacking
        while (currentPatState == _patrolState.Attacking)
        {
            //If player has sword change state to fleeing
            if (_SwordCheck.activeInHierarchy)
            {
                currentPatState = _patrolState.Fleeing;
            }
            //If player is unarmed
            else
            {
                //Change UI text to reflect you are being attacked
                _patrolText.text = "Attacking";
                //If player is still within distance keep chasing
                if (Vector2.Distance(transform.position, player.position) < distanceToPlayer)
                {
                    //Value of 1f means PatrolFlee method will cause AI to move towards player
                    runOrChase = 1f;
                    PatrolFlee();
                }
                //If player is no longer in distance reset variable to allow closest waypoint calculation to run and set state to patrolling
                else
                {
                    _runCounter = false;
                    currentPatState = _patrolState.Patrolling;
                }
            }
            yield return null;
        }
        //Return to state selection method to move to a new coroutine for the new state
        PatrolState();
    }
    //Coroutine for fleeing state
    private IEnumerator FleeingState()
    {
        //Change sprite to fleeing sprite
        _aiSpriteRenderer.sprite = _aiFleeSprite;
        //While we are still fleeing
        while (currentPatState == _patrolState.Fleeing)
        {
            //Set UI text to reflect that AI is fleeing
            _patrolText.text = "Fleeing";
            //If player is still too close move away
            if (Vector2.Distance(transform.position, player.position) < distanceToPlayer)
            {
                //Value of -1f will cause PatrolFlee method to move AI away from player
                runOrChase = -1f;
                PatrolFlee();
            }
            //After escaping player, reset variable to allow closest waypoint calculation to be run and set state to patrolling
            else
            {
                _runCounter = false;
                currentPatState = _patrolState.Patrolling;
            }
            yield return null;
        }
        //Return to state selection method to move to new coroutine for new state
        PatrolState();
    }
    #endregion
    #region RunWhenDisabled
    private void OnDisable()
    {
        //If UI elements are still enabled
        if (_resetMenu != null)
        {
            //Pause game time
            Time.timeScale = 0f;
            //Activate victory menu
            _resetMenu.SetActive(true);
            _menuText.text = "Victory!";
        }
    }
    #endregion
}
