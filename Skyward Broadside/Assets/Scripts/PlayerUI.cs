using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

//Contains code taken from https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-ui-prefab
//A class that manages the behaviour of a player's healthbar and nametag. Each player has their own healthbar and nametag and thus their own PlayerUI instance.
public class PlayerUI : MonoBehaviour
{
    public bool isDead = false;

    #region Private Fields

    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    public Text playerNameText;

    [Tooltip("Image of skull, shown on enemy players")]
    [SerializeField]
    private RawImage skullImage;

    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;

    [Tooltip("Pixel offset from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    float heightAbovePlayer = 0.5f;
    CanvasGroup _canvasGroup;
    Vector3 targetPosition;

    Rigidbody playerRb;
    PlayerInfoPPH playerInfo;
    PhotonView photonView;

    //Player the health bar and name is attached to
    private PlayerPhotonHub target;

    bool gotCanvas;

    #endregion

    #region Monobehaviour Callbacks

    // Update is called once per frame
    void Update()
    {
        if (!gotCanvas)
        {
            if (GameObject.Find("Canvas") != null)
            {
                transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);
                _canvasGroup = GetComponent<CanvasGroup>();
                gotCanvas = true;
            }
        }
        else
        {
            CheckExistence();

            //Change the slider to show the player's current health
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = (int)playerInfo.currHealth;
            }
        }

    }

    //Track the position of the player. This ensures that the healthbar and nametag will always be displayed above the player.
    private void FixedUpdate()
    {
        if (CheckExistence() && playerRb != null)
        {
            targetPosition = playerRb.position;
            targetPosition.y += heightAbovePlayer;
            transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
        }
        else
        {
            Debug.LogWarning("Could not find playerRb");
            Destroy(gameObject);
        }
    }
    #endregion

    //Helper function to check whether the player still exists in the gameworld or not.
    bool CheckExistence()
    {
        // Destroy itself if the target is null. It's a fail safe when Photon is destroying Instances of a Player over the network.
        if (target == null)
        {
            Destroy(gameObject);
            return false;
        }
        return true;
    }

    #region Public Methods

    //Sets the player that the healthbar and nametag will correspond to. Also sets some variables relating to the player and sets the text for the nametag.
    public void SetTarget(PlayerPhotonHub _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayerPhotonHub target for PlayerUI.SetTarget.", this);
            return;
        }

        //Cache references for efficiency
        target = _target;

        //Tell the PlayerPhotonHub that the game object this script is attached to is the healthbar and nametag for the player.
        target.SetUI(gameObject);

        playerRb = _target.GetComponentInChildren<Rigidbody>();
        playerInfo = _target.GetComponentInChildren<PlayerInfoPPH>();
        photonView = playerRb.GetComponent<PhotonView>();
        if (playerNameText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
        }

    }

    //Check whether this particular player is visible to the camera of this game instance. This is done by checking whether this player's rigidbody is
    //within the camera's viewport. 
    public bool PlayerIsVisible()
    {
        if (playerRb != null)
        {
            var photonView = playerRb.GetComponent<PhotonView>();
            if (photonView.IsMine)
            {
                return false;
            }
            Vector3 screenPoint = Camera.main.WorldToViewportPoint(playerRb.position);

            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            {
                return true;
            }
        }
        return false;
    }

    //Sets the alpha value of the canvas group containing the healthbar and nametag.
    public void SetCanvasAlpha(float alpha)
    {
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = alpha;
        }
    }

    //Set the appearance of this player's healthbar and nametag to be that of an enemy player. This involves making the name text red and enabling the skull image
    //above the name.
    public void SetEnemyHealthbar()
    {
        skullImage.gameObject.SetActive(true);
        playerNameText.color = Color.red;
    }

    //Set the appearance of this player's healthbar and nametag to be that of a friendly player. The name text is black and there is no skull image.
    public void SetFriendlyHealthbar()
    {
        skullImage.gameObject.SetActive(false);
        playerNameText.color = Color.black;
    }

    //Set the healthbar and nametag to disappear when the player they attached to is dead.
    public void SetDead()
    {
        gameObject.SetActive(false);
        isDead = true;
    }

    //Set the healthbar and nametag to reappear when the player they are attached to respawns.
    public void SetAlive()
    {
        targetPosition = playerRb.position;
        targetPosition.y += heightAbovePlayer;
        transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
        playerHealthSlider.value = (int)playerInfo.currHealth;
        gameObject.SetActive(true);
        isDead = false;

    }
    #endregion
}
