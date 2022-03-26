using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    //Movement Variables
    [Header("Movement")]
    private float _speed = 2.5f;
    private float _horizontal = 0f;
    private float _vertical = 0f;
    //Item Variables
    [Header("Items")]
    [Tooltip("Add the Sword game object from the Environment grouping")]
    [SerializeField]private GameObject _sword;
    [Tooltip("Add the PlayerSword game object that is a child of the player game object")]
    [SerializeField]private GameObject _playerSword;
    //Sprite Variables
    [Header("Sprite")]
    [Tooltip("Add the sprite from your assets that corresponds to the player having a weapon")]
    [SerializeField] private Sprite _playerSwordSprite;
    private SpriteRenderer _playerSwordRenderer;
    //UI Menu Variable
    [Tooltip("Add the RestartMenu canvas object")]
    [SerializeField] private GameObject _restartMenu;
    [Tooltip("Add the PauseMenu canvas object")]
    [SerializeField] private GameObject _pauseMenu;
    #endregion
    private void Start()
    {        
        //Retrieve renderer component to enable sprite swapping
        _playerSwordRenderer = GetComponent<SpriteRenderer>();
    }
    void Update()
    {        
        #region PlayerMovement
        //Set Movement direction variable to current position
        Vector2 _moveDir = transform.position;
        //Get input values and assign them to the appropriate variables
        _horizontal = Input.GetAxisRaw("Horizontal");
        _vertical = Input.GetAxisRaw("Vertical");
        
        //If the left or right buttons are pressed
        if (Input.GetButton("Horizontal"))
        {
            //MoveDir  equals the direction of the input multiplied by the speed set by the speed variable
            _moveDir.x += _horizontal * _speed * Time.deltaTime;
        }
        //If the up or down buttons are pressed
        if (Input.GetButton("Vertical"))
        {
            //MoveDir  equals the direction of the input multiplied by the speed set by the speed variable
            _moveDir.y += _vertical * _speed * Time.deltaTime;
        }
        //If escape button pushed pause the game and open pause menu
        if (Input.GetButton("Cancel"))
        {
            Time.timeScale = 0f;
            _pauseMenu.SetActive(true);
        }
        //Transform the players position based off of moveDir
        transform.position = (Vector3) _moveDir;
        #endregion
    }

    #region Collisions
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Determine if the object collided with is the sword
        if (collision.gameObject.tag == "Item")
        {
            //Disable main sword sprite, swap player sprite and enable players child sword object
            _sword.SetActive(false);
            _playerSwordRenderer.sprite = _playerSwordSprite;
            _playerSword.SetActive(true);
        }
        //If player has already picked up sword and is colliding with something else
        else if (_playerSword.activeInHierarchy && collision.gameObject.tag != "Environment")
        {
            //Kill the other object
            collision.gameObject.SetActive(false);
        }
        //If colliding with Patrol AI without the sword
        else if (collision.gameObject.tag == "AI")
        {
            //AI has killed player, open restart menu
            _restartMenu.SetActive(true);
        }
    }
    #endregion
}
