using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIForagerMovement : MonoBehaviour
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
    [Tooltip("Add the game object for the player")]
    public Transform player;
    [Tooltip("Add the game object for the storage house that corresponds to the Forager selected")]
    public Transform storage;
    //Item variables
    [Header("Items")]
    public int resources = 0;
    public int stockpile = 0;
    //Variable array to contain waypoints for AI destinations
    [Header("Waypoints")]
    [Tooltip("Add the RockPoint or StonePoint objects to use as your waypoints depending on which forager is selected")]
    public Transform[] foragePoints;
    public int forageIndex = 0;
    //State Variables (No Header needed as nothing to show in unity)    
    private _foragerState currentState;
    private enum _foragerState
    {
        Foraging,
        Fleeing,
    }
    //Sprite variables
    [Header("Sprites")]
    [Tooltip("Add the sprite from your assets folder that corresponds to the forage state")]
    [SerializeField] private Sprite _forageSprite;
    [Tooltip("Add the sprite from your assets folder that corresponds to the flee state")]
    [SerializeField] private Sprite _fleeSprite;
    private SpriteRenderer _spriteRenderer;
    //Variables to change UI Text
    [Header("User Interface")]
    [Tooltip("Add the ForagerText object from the EnemyStates canvas that corresponds to the selected forager")]
    [SerializeField] private Text _forageText;
    [Tooltip("Add the Resources text object from the EnemyStates canvas that corresponds to the selected player")]
    [SerializeField] private Text _forageResourcesText;
    [Tooltip("Add the AIResources text object from the EnemyStates canvas")]
    [SerializeField] private Text _stockpileText;
    //Boolean variable to enable closest waypoint script to only run once
    private bool _runCount = false;
    #endregion
    void Start()
    {
        #region StartupVariables
        //Retrieve renderer component to enable sprite swapping
        _spriteRenderer = GetComponent<SpriteRenderer>();
        //Give initial values to display text
        _forageResourcesText.text = resources.ToString() +" Units";
        _stockpileText.text = stockpile.ToString();
        //Put current position into movement direction variable for later
        Vector2 aiMoveDir = transform.position;
        //Run the state selection method to determine what action AI will take (Default is foraging)
        SelectState();
        #endregion
    }
    #region Movement
    //Method to change destination to next waypoint on list
    private void WaypointUpdate()
    {
        //If forager is within the set distance to be considered at the destination
        if (Vector2.Distance(transform.position, foragePoints[forageIndex].position) < minDistanceToWaypoint)
        {
            //Add 10 resources to forager's inventory and change UI to reflect that
            resources += 10;
            _forageResourcesText.text = resources.ToString() + " Units";
            //Increment the forage point index to select the next waypoint on list
            forageIndex++;
            //If we have reached end of list, return to the first waypoint
            if (forageIndex >= foragePoints.Length)
            {
                forageIndex = 0;
            }
        }
    }
    //Method to determine closest waypoint after having run from player
    private void ClosestWaypoint()
    {
        //Define a variable to store a distance to be updated as we check through list, make value large so it will not rule out actual waypoiints
        float lowestDistance = float.PositiveInfinity;
        //Lowest index is set to the first waypoint on list by default
        int lowestIndex = 0;
        //A distance variable to store the distance to the current waypoint being checked
        float distance;
        //For each waypoint on list, determine the distance and if distance is less than value of the lowest distance, store that waypoint as the current closest
        for (int i = 0; i < foragePoints.Length; i++)
        {
            distance = Vector2.Distance(player.position, foragePoints[i].position);
            if (distance < lowestDistance)
            {
                lowestDistance = distance;
                lowestIndex = i;
            }
        }
        //Change destination to waypoint we have determined as closest
        forageIndex = lowestIndex;
    }
    //Movement method to control AI movement while it is foraging
    private void AIMoveTowards()
    {
        //If we are far enough away from the destination, move towards it
        if (Vector2.Distance(transform.position, foragePoints[forageIndex].position) > minDistanceToWaypoint)
        {            
            Vector2 directionToGoal = (foragePoints[forageIndex].position - transform.position);
            directionToGoal.Normalize();
            transform.position += (Vector3)directionToGoal * forageSpeed * Time.deltaTime;
            //As we get to resource storage point on the way past drop of resources currently being held
            if (Vector2.Distance(transform.position, storage.position) < 1f && resources > 1)
            {
                //Retrieve current stockpile value from UI to ensure we'll have the correct value for multiple foragers
                int realStockpile = int.Parse(_stockpileText.text);
                //Change stockpile value to value of current stockpile plus resources collected by the forager in question and change UI value to reflect new total
                stockpile = resources + realStockpile;
                _stockpileText.text = stockpile.ToString();
                //Reset value of resources being held by the forager to 0 and change UI to reflect new value
                resources = 0;
                _forageResourcesText.text = resources.ToString() + " Units";                
            }
        }
        //If we are too close to move towards waypoint set position to same as destination position
        else
        {
            transform.position = foragePoints[forageIndex].position;            
        }
    }
    //Movement method to control AI movement while it is fleeing
    private void AIFlee()
    {
        //If we are too close to player move away from player
        if (Vector2.Distance(transform.position, player.position) < distanceToPlayer)
        {
            Vector2 directionToGoal = ((player.position - transform.position) * -1f);
            directionToGoal.Normalize();
            transform.position += (Vector3)directionToGoal * fleeSpeed * Time.deltaTime;
        }        
    }
    #endregion
    #region StateMachine
    private void SelectState()
    {
        //Define coroutines to run for each state listed in the _foragerState enum and run the coroutine for the current state (Default to foaraging)
        switch (currentState)
        {
            case _foragerState.Foraging:
                StartCoroutine(ForageState());
                break;
            case _foragerState.Fleeing:
                StartCoroutine(FleeState());
                break;
            default:
                currentState = _foragerState.Foraging;
                break;
        }
    }
    //Forager state code
    private IEnumerator ForageState()
    {
        //Change sprite to foraging sprite
        _spriteRenderer.sprite = _forageSprite;
        //If we haven't already determined the closest waypoint, run the closest waypoint method and then record that we have now run the calculation
        if (_runCount == false)
        {
            ClosestWaypoint();
            _runCount = true;
        }
        //While they are still foraging and have not been chased
        while (currentState == _foragerState.Foraging)
        {
            //Update UI text to display state as foraging
            _forageText.text = "Foraging";
            //If player is not too close determine next waypoint and move to it
            if (Vector2.Distance(transform.position, player.position) > distanceToPlayer)
            {
                WaypointUpdate();
                AIMoveTowards();                
            }
            //If player gets too close change state to fleeing
            else
            {
                currentState = _foragerState.Fleeing;                
            }
            yield return null;
        }
        //Return to State Selection method
        SelectState();
    }
    //Fleeing state code
    private IEnumerator FleeState()
    {
        //Change sprite to fleeing sprite
        _spriteRenderer.sprite = _fleeSprite;
        //While they are still running away from player
        while (currentState == _foragerState.Fleeing)
        {
            //Update UI text to display current state as fleeing
            _forageText.text = "Fleeing";
            //If player is still too close run the method to run away
            if (Vector2.Distance(transform.position, player.position) < distanceToPlayer)
            {
                AIFlee();
            }
            //If forager has successfully evaded player, reset variable so we will determine closest waypoint again and set current state to foraaging
            else
            {
                _runCount = false;
                currentState = _foragerState.Foraging;
            }
            yield return null;
        }
        //Return to state selection method
        SelectState();
    }
    #endregion
    #region RunWhenDisabled
    private void OnDisable()
    {
        //If UI objects are still enabled
        if (_forageText != null)
        {
            //Reset UI text to reflect this character is now dead
            _forageText.text = "Deceased";
            _forageResourcesText.text = " ";
        }       
    }
    #endregion
}
