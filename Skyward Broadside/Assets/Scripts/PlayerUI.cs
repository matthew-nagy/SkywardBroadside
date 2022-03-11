using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


//Code taken from https://doc.photonengine.com/en-us/pun/current/demos-and-tutorials/pun-basics-tutorial/player-ui-prefab
public class PlayerUI : MonoBehaviourPun
{
    #region Private Fields
    [Tooltip("UI Text to display Player's Name")]
    [SerializeField]
    private Text playerNameText;


    [Tooltip("UI Slider to display Player's Health")]
    [SerializeField]
    private Slider playerHealthSlider;

    [Tooltip("Pixel offset from the player target")]
    [SerializeField]
    private Vector3 screenOffset = new Vector3(0f, 30f, 0f);

    Vector3 playerPos;
    float heightAbovePlayer = 0.5f;
    Transform targetTransform;
    Renderer targetRenderer;
    CanvasGroup _canvasGroup;
    Vector3 targetPosition;

    Rigidbody playerRb;

    //Player the health bar and name is attached to
    private PlayerPhotonHub target;
    #endregion

    #region Monobehaviour Callbacks
    private void Awake()
    {
        //Any UI element must be placed within a Canvas GameObject
        //When scenes are going to be loaded and unloaded, so is our Prefab, and the Canvas will be different every time
        //Not actually recommended to do this bc it's slow apparently 
        //Supposedly there's a better way but they don't say what it is...
        this.transform.SetParent(GameObject.Find("Canvas").GetComponent<Transform>(), false);

        _canvasGroup = this.GetComponent<CanvasGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Destroy itself if the target is null, It's a fail safe when Photon is destroying Instances of a Player over the network
        if (target == null)
        {
            Destroy(this.gameObject);
            return;
        }
        // Reflect the Player Health
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = (int)target.currHealth;
        }
    }

    private void FixedUpdate()
    {
        // Do not show the UI if we are not visible to the camera, thus avoid potential bugs with seeing the UI, but not the player itself.
        if (targetRenderer != null)
        {
            this._canvasGroup.alpha = targetRenderer.isVisible ? 1f : 0f;
        }


        targetPosition = playerRb.position;
        print(targetPosition);
        targetPosition.y += heightAbovePlayer;
        transform.position = Camera.main.WorldToScreenPoint(targetPosition) + screenOffset;

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
        playerRb = _target.GetComponentInChildren<Rigidbody>();
        if (playerNameText != null)
        {
            var photonView = playerRb.GetComponent<PhotonView>();
            playerNameText.text = photonView.Owner.NickName;
        }

        targetTransform = this.target.GetComponent<Transform>();
        targetRenderer = this.target.GetComponent<Renderer>();

        
    }
    #endregion
}
