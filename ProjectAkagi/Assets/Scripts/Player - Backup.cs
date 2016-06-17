using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Poker
{
    public class PlayerBackup : Photon.MonoBehaviour {
        public List<Poker.Card> hand = new List<Card>();
        Vector2 scrollPosition;
        Touch touch;
        bool flag = false;
        public int wallet = 0;      
        public int id;
        float initX = 0;
        float initY = 0;
        float initialDistance;
        // Events for turn action handling
        public delegate void IsBetting(int playerId);
        public static event IsBetting OnClicked;
        public GameController gameMaster = null;

        Transform cards;
        Transform localCards;

        Vector3 lastTouch;

        private Vector3 currentPos = Vector3.zero;
        private Vector3 previousPos = Vector3.zero;

        public Vector3 originalCardPos;

        bool isLetGo = false;
        bool isHeld = false;
        bool isStationary = false;
        bool isDrag = false;
        bool isClicked = false;
        bool isReleased = false;
        bool isMoved = false;
        bool isPlayingChipSFX = false;
        bool isDown = true;

        private Vector3 targetLocation;
        private Vector3 playerPos;

        Vector3 flopPos;
        GameObject slider;
        GameObject txtBet;

        public bool isPeeking = false;
        public bool isBetting = false;

        public string mString = "";

        Vector3 foldPoint;

        Transform slot;
        Transform localSlot;
        Transform chips;

        private Vector3 correctPos;

        public int betAmount = 0;

        public GameObject mTxtBetAmt = null;

        int slotID = 0;

        public AudioSource chipsCollideSFX;
        public AudioSource chipsCollideSFX2;
        public AudioSource chipsCollideSFX3;
        public AudioSource cardsFlipSFX;

        GameObject peekingSprite;

        float duration = 1.15f;
        float timer = 1.0f;

        public void SendBetting()
        {
            if (OnClicked != null)
            {
                OnClicked(id);
            }
        }

        [PunRPC]
        void setIsBetting(bool _state)
        {
            this.isBetting = _state;
        }

        [PunRPC]
        void setBettingUI(bool _state)
        {
            this.mTxtBetAmt.SetActive(_state);
        }

        [PunRPC]
        void SetId(int _id)
        {            
            id = (_id % 6); // 6 is max players, we need to loop back around as 6th player equals 0th slot, so slot0 is pos 6.
            slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + id);
            localSlot = GameObject.Find("PlayerSlotPositions").transform.Find("slot1");
            chips = slot.Find("mChips");
            originalCardPos = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("PlayerHandPosition").transform.position;
            cards = localSlot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");            
            localCards = localSlot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
            targetLocation = originalCardPos;
            playerPos = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.position;
            playerPos = new Vector3(playerPos.x, 0, playerPos.z);
            foldPoint = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("foldPoint").transform.position;
            flopPos = GameObject.Find("Flop").transform.position;
            slider = GameObject.Find("Canvas").transform.Find("Panel").transform.Find("scrollBet").gameObject;
            txtBet = GameObject.Find("Canvas").transform.Find("txtBet").gameObject;
            this.enabled = true;
        }

        [PunRPC]
        void SetUI()
        {
            // assume ID is known.
            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (_slotID < 0)
                _slotID += 6; // max number of players.

            slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + _slotID);
            chips = slot.Find("mChips");

            this.mTxtBetAmt = GameObject.Find("Canvas").transform.Find("playerBets").transform.Find("txtSlot"+ _slotID).gameObject;

            Vector3 pos = chips.Find("uiPos").transform.position;
            Vector3 screenPos = Camera.main.WorldToViewportPoint(pos);
            screenPos.x *= GameObject.Find("Canvas").GetComponent<RectTransform>().rect.width;
            screenPos.y *= GameObject.Find("Canvas").GetComponent<RectTransform>().rect.height;
            this.mTxtBetAmt.transform.parent.position = screenPos;

            this.mTxtBetAmt.GetComponent<Text>().text = this.betAmount.ToString();

            Transform playerPosition = slot.Find("player");
            Vector3 playerPos = playerPosition.position;
            screenPos = Camera.main.WorldToViewportPoint(playerPos);
            screenPos.x *= GameObject.Find("Canvas").GetComponent<RectTransform>().rect.width;
            screenPos.y *= GameObject.Find("Canvas").GetComponent<RectTransform>().rect.height;
            GameObject mPlayerNameTag = GameObject.Find("Canvas").transform.Find("playerNametags").Find("txtSlot" + _slotID).Find("text").gameObject;
            mPlayerNameTag.transform.parent.position = screenPos;
            mPlayerNameTag.GetComponent<Text>().text = "player" + this.id;
            mPlayerNameTag.GetComponent<Text>().color = this.GetComponent<Renderer>().material.color;
            mPlayerNameTag.transform.parent.gameObject.SetActive(true);

            

        }

        [PunRPC]
        void TogglePeekingSprite(bool _flag)
        {
            // assume ID is known.
            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (_slotID < 0)
                _slotID += 6; // max number of players.

            GameObject Eyes = GameObject.Find("Canvas").transform.Find("PeekingEyes").transform.Find("IsPeeking" + _slotID).gameObject;
            Eyes.SetActive(_flag);
        }

        [PunRPC]
        void InitColour(int id)
        {
            this.GetComponent<Renderer>().material = gameMaster.mats[id - 1];
        }

        [PunRPC]
        void SetGlowEffectCards(string _tag)
        {
            slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (slotID < 0)
                slotID += 6; // max number of players.

            Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
            Transform _cards = slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");

            _cards.transform.Find("Card0").Find("Occludee0").tag = _tag;
            _cards.transform.Find("Card0").Find("Occludee1").tag = _tag;
            _cards.transform.Find("Card1").Find("Occludee0").tag = _tag;
            _cards.transform.Find("Card1").Find("Occludee1").tag = _tag;
            Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
        }

        void SetLocalGlowEffectCards(string _tag)
        {

            Transform _cards = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
            _cards.transform.Find("Card0").Find("Occludee0").tag = _tag;
            _cards.transform.Find("Card0").Find("Occludee1").tag = _tag;
            _cards.transform.Find("Card1").Find("Occludee0").tag = _tag;
            _cards.transform.Find("Card1").Find("Occludee1").tag = _tag;
            Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
        }

        [PunRPC]
        void SetGlowEffectChips(string _tag, bool _isClicked)
        {
            slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (slotID < 0)
                slotID += 6; // max number of players.

            Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
            Transform _chips = slot.transform.Find("mChips");
            _chips.Find("Chips0").tag = _tag;
            _chips.Find("Chips1").tag = _tag;
            _chips.Find("Chips2").tag = _tag;
            _chips.gameObject.GetComponent<Animator>().SetBool("IsClicked", _isClicked);
            Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
        }

        void SetLocalGlowEffectChips(string _tag, bool _isClicked)
        {
            Debug.Log("Local " + _isClicked);
            Transform _chips = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("mChips");
            _chips.Find("Chips0").tag = _tag;
            _chips.Find("Chips1").tag = _tag;
            _chips.Find("Chips2").tag = _tag;
            _chips.gameObject.GetComponent<Animator>().SetBool("IsClicked", _isClicked);
            Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
        }

        [PunRPC]
        void DealHandTest(int[] players)
        {            
            localCards.gameObject.SetActive(true);
            localSlot.Find("mChips").gameObject.SetActive(true);

            for (int i=0; i < players.Length; i++)
            {
                slotID = (players[i] - (PhotonNetwork.player.ID - 1)) % 6;
                // Raaaaaaging
                if (slotID < 0)
                    slotID += 6; // max number of players.

                
                Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
                slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand").gameObject.SetActive(true);
                slot.transform.Find("mChips").gameObject.SetActive(true);
            }
        }

        [PunRPC]
        void DealHand(Vector3 _card0, Vector3 _card1)
        {
            // We recieve two cards as Vector3's whose x, y, z values represent as integers the
            // colour, rank and suit of the respective cards when converted back to their enums
            // as passing custom types via PUN looks a little complicated.
            if (photonView.isMine)
            {
                hand = new List<Card>();
                Poker.Card card0 = new Card((Suit)_card0.x, (Rank)_card0.y, (Colour)_card0.z);
                Poker.Card card1 = new Card((Suit)_card1.x, (Rank)_card1.y, (Colour)_card1.z);
                // Add the two cards to the player's hand.
                hand.Add(card0);
                hand.Add(card1);
                Debug.Log("Hello player" + photonView.ownerId);
                InitializePlayerHand();
            }

        }

        void InitializePlayerHand()
        {
            if (hand.Count == 2)
            {
                GameObject _Card0 = localCards.Find("Card0").gameObject;
                GameObject _Card1 = localCards.Find("Card1").gameObject;

                _Card0.transform.Find("front").gameObject.GetComponent<SpriteRenderer>().sprite = 
                    gameMaster.cardSprites[((int)hand[0].suit * 13) + (int)hand[0].rank - 1];

                _Card1.transform.Find("front").gameObject.GetComponent<SpriteRenderer>().sprite =
                    gameMaster.cardSprites[((int)hand[1].suit * 13) + (int)hand[1].rank - 1];
            }
            else
            {
                Debug.Log("Hand not equal to 2.");
                Debug.Break();
            }
        }

        IEnumerator PlayChipSFX()
        {
            yield return new WaitForSeconds(0.25f);
            chipsCollideSFX.Play();
            chipsCollideSFX2.PlayDelayed(0.35f);
            chipsCollideSFX3.PlayDelayed(0.15f);
        }

        void Awake()
        {
            if (gameMaster == null)
            {
                gameMaster = GameObject.Find("GameController").GetComponent<GameController>();            
            }
        }

        // Use this for initialization
        void Start () {
            if (gameMaster == null)
            {
                GameObject.Find("GameController").GetComponent<GameController>();                
            }
	    }

        // Update is called once per frame
        void Update()
        {
            if (!this.photonView.isMine)
            {
                slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
                //Debug.Log("Slot ID: " + slotID + ", this.id: " + this.id + ", local PlayerID" + PhotonNetwork.player.ID);
                // 2 - (2 - 1) : 1 % 6 = 1
                // Raaaaaaging
                if (slotID < 0)
                    slotID += 6; // max number of players.

                Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
                Transform _cards = slot.Find("PlayerHandPosition").transform.Find("PlayerHand");
                if (localCards != null && _cards != null)
                {
                    // Network player, recieve data            
                    _cards.localPosition = Vector3.Lerp(_cards.localPosition, this.correctPos, Time.deltaTime * 5);
                }

                if (this.isBetting)
                {   
                    this.mTxtBetAmt.GetComponent<Text>().text = ((int)Mathf.Floor(Mathf.Lerp(
                       int.Parse(this.mTxtBetAmt.GetComponent<Text>().text), this.betAmount, Time.deltaTime * 5))).ToString();
                }
                    

                //txtBet.GetComponent<Text>().text = this.betAmount.ToString();
            }
            else
            {
                if (this.isBetting)
                {
                    this.betAmount = (int)Mathf.Floor(slider.GetComponent<Scrollbar>().value * 500);
                    if (!isPlayingChipSFX)
                    {
                        isPlayingChipSFX = true;
                        chipsCollideSFX.Play();
                        chipsCollideSFX2.PlayDelayed(0.35f);
                        chipsCollideSFX3.PlayDelayed(0.15f);
                    }
                    else
                    {
                        duration -= Time.deltaTime;
                        if (duration < 0)
                        {
                            if (!chipsCollideSFX.isPlaying && !chipsCollideSFX2.isPlaying && !chipsCollideSFX3.isPlaying)
                            {
                                isPlayingChipSFX = false;
                            }
                            else
                            {
                                duration = 1.15f;
                            }
                        }

                    }

                }                    

                txtBet.GetComponent<Text>().text = this.betAmount.ToString();
            }
        }

        // At the end of Update
        void LateUpdate () {
            RaycastHit hit;
            // Player is using a device.
            if (Input.GetMouseButtonDown(0) && PhotonNetwork.player.ID == this.id)
            {
                Ray ray;
                Debug.Log("Mouse down");
                currentPos = previousPos = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(currentPos);

                if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player0" && hit.transform.gameObject.layer != LayerMask.NameToLayer("chips"))
                {
                    initX = currentPos.x;
                    initY = currentPos.y;
                    initialDistance = (cards.position - playerPos).magnitude;
                    isHeld = true;
                    slider.SetActive(false);
                    txtBet.SetActive(false);
                    this.photonView.RPC("SetGlowEffectCards", PhotonTargets.Others, "Occludee");
                    SetLocalGlowEffectCards("Occludee");
                    //cardsFlipSFX.Play();
                }
                else if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player0" && (hit.transform.gameObject.layer == LayerMask.NameToLayer("chips")))
                {
                    slider.SetActive(true);
                    txtBet.SetActive(true);
                    this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Occludee", true);
                    SetGlowEffectChips("Occludee", true);
                    this.photonView.RPC("setIsBetting", PhotonTargets.All, true);
                    this.photonView.RPC("setBettingUI", PhotonTargets.Others, true);
                    isPlayingChipSFX = true;
                    chipsCollideSFX.Play();
                    chipsCollideSFX2.PlayDelayed(0.35f);
                    chipsCollideSFX3.PlayDelayed(0.15f);
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Default") && EventSystem.current.currentSelectedGameObject == null)
                {
                    slider.SetActive(false);
                    txtBet.SetActive(false);
                    slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
                    // Raaaaaaging
                    if (slotID < 0)
                        slotID += 6; // max number of players.
                    Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
                    Transform _cards = slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
                    if (localSlot.Find("mChips").gameObject.GetActive())
                    {
                        this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                        SetLocalGlowEffectChips("Untagged", false);
                        this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                        this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                        StartCoroutine(PlayChipSFX());
                    }
                }
            }

            // if we're holding down the left mouse button
            // we are either holding stationary, or dragging.
            if (Input.GetMouseButton(0) && PhotonNetwork.player.ID == this.id)
            {
                currentPos = Input.mousePosition;
                if (isHeld)
                {

                    // if is not moving
                    if (Mathf.Approximately(Input.mousePosition.x, previousPos.x) &&
                            Mathf.Approximately(Input.mousePosition.y, previousPos.y))
                    {
                        Debug.Log("Mouse is stationary");
                        float _angle = Vector3.Angle(Vector3.up, cards.Find("Card0").transform.up);
                        Debug.Log("Angle of cards: " + _angle);
                        if (_angle > 120.0f)
                        {
                            // an angle of greater than 80 degrees suggests the player is peeking at his cards.
                            this.photonView.RPC("TogglePeekingSprite", PhotonTargets.Others, true);
                        }
                    }
                    else
                    {
                        // is moving
                        Debug.Log("Mouse is moving");
                        isStationary = false;
                        isDrag = true;
                        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
                        targetLocation = new Vector3(newPos.x, cards.position.y, newPos.z);
                        if (!isMoved)
                        {
                            cardsFlipSFX.Play();
                            isMoved = true;
                        }
                        this.photonView.RPC("TogglePeekingSprite", PhotonTargets.Others, false);
                    }
                }

                if (txtBet.GetActive())
                {
                    Vector3 handlePos = slider.transform.Find("Sliding Area").Find("Handle").transform.position;
                    txtBet.transform.position = new Vector3(handlePos.x, handlePos.y + 50, handlePos.z);
                }
            }

            // Player released the cards
            if (Input.GetMouseButtonUp(0) && PhotonNetwork.player.ID == this.id)
            {
                isReleased = false;
                isMoved = false;
                Debug.Log("Mouse released");
                isLetGo = true;
                isHeld = false;
                isDrag = false;
                isStationary = false;
                slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
                // Raaaaaaging
                if (slotID < 0)
                    slotID += 6; // max number of players.
                Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
                Transform _cards = slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
                if (_cards.gameObject.GetActive())
                {
                    this.photonView.RPC("SetGlowEffectCards", PhotonTargets.Others, "Untagged");
                    SetLocalGlowEffectCards("Untagged");
                }
                this.photonView.RPC("TogglePeekingSprite", PhotonTargets.Others, false);

            }

            // Lerp the hand back to it's original start position and orientation
            if (isLetGo && !isStationary && !isDrag && !isHeld && PhotonNetwork.player.ID == this.id)
            {
                if (cards != null)
                {
                    cards.position = Vector3.Lerp(cards.position, originalCardPos, 10 * Time.deltaTime);
                    cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                    cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                    if (Mathf.Approximately(cards.position.x, originalCardPos.x) && Mathf.Approximately(cards.position.y, originalCardPos.y)
                        && Mathf.Approximately(cards.position.z, originalCardPos.z))
                    {
                        isLetGo = false;
                    }

                }
            }
            else if ((isDrag || isStationary) && PhotonNetwork.player.ID == this.id)
            {
                // Updating our coordinates, still needs polish              
                if (cards != null)
                {
                    Debug.Log("Dragging...");
                    cards.position = Vector3.Lerp(cards.position, targetLocation, Time.deltaTime * 2);

                    float xCardPos = map(Input.mousePosition.x, initX, Camera.main.WorldToScreenPoint(playerPos).x, 0, 100);
                    float yCardPos = map(Input.mousePosition.y, initY, Camera.main.WorldToScreenPoint(playerPos).y, 0, 100);
                    Vector3 flatCardPosition = new Vector3(cards.position.x, 0, playerPos.z);
                    float distanceToPlayerPos = (flatCardPosition - playerPos).magnitude - initialDistance;
                    float cardPos = Mathf.Abs(distanceToPlayerPos);
                    float rot = Mathf.LerpAngle(0, 180, (cardPos / initialDistance));

                    if (cards.position.x > playerPos.x)
                    {
                        cards.position = new Vector3(playerPos.x, cards.position.y, cards.position.z);

                        if (cards.position.z > foldPoint.z)
                        {
                            cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                            cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                        }
                        else
                        {
                            cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.AngleAxis(rot, new Vector3(0, 0, -1)), Time.deltaTime * 10);
                            cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.AngleAxis(rot, new Vector3(0, 0, -1)), Time.deltaTime * 10);
                        }
                    }
                    else if (cards.position.x <= originalCardPos.x)
                    {
                        cards.position = new Vector3(originalCardPos.x, cards.position.y, cards.position.z);
                        if (cards.position.z > foldPoint.z)
                        {
                            cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                            cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                        }
                        else
                        {
                            cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                            cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                        }                       
                    }
                    else
                    {
                        if (cards.position.z > foldPoint.z)
                        {
                            cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                            cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.identity, Time.deltaTime * 10);
                        }
                        else
                        {
                            cards.Find("Card0").transform.rotation = Quaternion.Lerp(cards.Find("Card0").transform.rotation, Quaternion.AngleAxis(rot, new Vector3(0, 0, -1)), Time.deltaTime * 10);
                            cards.Find("Card1").transform.rotation = Quaternion.Lerp(cards.Find("Card1").transform.rotation, Quaternion.AngleAxis(rot, new Vector3(0, 0, -1)), Time.deltaTime * 10);
                        }
                    }

                }
            }

            previousPos = currentPos;
        }
        
        // convert screen point to world point
        private Vector2 getWorldPoint(Vector2 screenPoint)
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(screenPoint), out hit);
            return hit.point;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (localCards != null)
            {
                if (stream.isWriting)
                {
                    // We own this player, send other players our data
                    stream.SendNext(localCards.localPosition);
                    stream.SendNext(this.betAmount);
                }
                else
                {
                    correctPos = (Vector3)stream.ReceiveNext();
                    this.betAmount = (int)stream.ReceiveNext();
                }
            }
        }

        void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if (PhotonNetwork.player.ID == info.sender.ID)
            {
                // Created by local player
                // Transform already handled by Network Manager.
                //this.transform.SetParent(GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform);
                //this.transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                // Created over the network by another player
                slotID = (info.sender.ID - (PhotonNetwork.player.ID - 1)) % 6;
                // What the fuck C# !?!?!? -Modulo is negative? Wtf.
                if (slotID < 0)
                    slotID += 6; // max number of players.
                
                this.transform.SetParent(GameObject.Find("PlayerSlotPositions").transform.Find("slot"+ slotID).transform);
                this.transform.localPosition = new Vector3(0, 0, 0);
                this.name = "player";
            }

        }

        float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        void OnGUI()
        {
            GUI.Label(new Rect(0, 40, 200, 200), "Player"+(PhotonNetwork.player.ID).ToString());
            GUI.Label(new Rect(0, 90, 200, 200), "Phase: " + mString);
        }
    }
}

