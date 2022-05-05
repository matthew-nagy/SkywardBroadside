using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


//Code taken from https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-ui-prefab
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

    Vector3 playerPos;
    float heightAbovePlayer = 0.5f;
    Renderer targetRenderer;
    CanvasGroup _canvasGroup;
    Vector3 targetPosition;

    Rigidbody playerRb;
    PlayerInfoPPH playerInfo;
    PhotonView photonView;
    //Vector3 instanceOwnerPos;

    //Player the health bar and name is attached to
    private PlayerPhotonHub target;

    bool gotCanvas;

    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        //Any UI element must be placed within a Canvas GameObject
        //When scenes are going to be loaded and unloaded, so is our Prefab, and the Canvas will be different every time
        //Not actually recommended to do this bc it's slow apparently 
        //Supposedly there's a better way but they don't say what it is...
    }

    void TryGetCanvas()
    {

    }

    bool CheckExistance()
    {
        // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
        if (target == null)
        {
            Destroy(gameObject);
            return false;
        }
        return true;
    }

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
            CheckExistance();

            // Reflect the Player Health
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = (int)playerInfo.currHealth;
            }
        }

    }

    private void FixedUpdate()
    {
        if (CheckExistance() && playerRb != null)
        {
            // Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
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

    #region Public Methods

    public void SetTarget(PlayerPhotonHub _target)
    {
        if (_target == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
            return;
        }

        //Cache references for efficiency
        target = _target;

        target.SetUI(gameObject);

        playerRb = _target.GetComponentInChildren<Rigidbody>();
        playerInfo = _target.GetComponentInChildren<PlayerInfoPPH>();
        photonView = playerRb.GetComponent<PhotonView>();
        if (playerNameText != null)
        {
            playerNameText.text = photonView.Owner.NickName;
        }

        //Getting the renderer of one of the child primitive objects
        //This probs won't work when we add the proper airship model in unless we add a mesh renderer
        targetRenderer = target.GetComponentInChildren<Renderer>();
    }

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

    public void SetCanvasAlpha(float alpha)
    {
        if (_canvasGroup != null)
        {
            this._canvasGroup.alpha = alpha;
        }
    }

    public void SetEnemyHealthbar()
    {
        skullImage.gameObject.SetActive(true);
        playerNameText.color = Color.red;
    }

    public void SetFriendlyHealthbar()
    {
        skullImage.gameObject.SetActive(false);
        playerNameText.color = Color.black;
    }

    public void SetDead()
    {
        this.gameObject.SetActive(false);
        isDead = true;
    }

    public void SetAlive()
    {
        targetPosition = playerRb.position;
        targetPosition.y += heightAbovePlayer;
        transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;
        playerHealthSlider.value = (int)playerInfo.currHealth;
        this.gameObject.SetActive(true);
        isDead = false;

    }
    #endregion
}
