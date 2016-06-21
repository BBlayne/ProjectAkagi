using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Poker
{
    public class Player : Photon.MonoBehaviour {
        public List<Poker.Card> hand = new List<Card>();


        Vector2 scrollPosition;
        Touch touch;
        bool flag = false;
        public int wallet = 500;      
        public int id;
        float initX = 0;
        float initY = 0;
        float initZ = 0;
        float initialDistance;

        public GameController gameMaster = null;

        bool isDealer = false;

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
        public bool hasPlayed = false;
        public bool hasFolded = false;

        public bool hasCalled = false;
        public bool hasBet = false;

        Animator cardAnim0;
        Animator cardAnim1;

        private Vector3 targetLocation;
        private Vector3 playerPos;

        Vector3 flopPos;
        SliderButtons slider;
        GameObject txtBet;

        public bool isPeeking = false;
        public bool isBetting = false;

        public string mString = "";

        Vector3 foldPoint;

        Transform slot;
        Transform localSlot;
        Transform chips;
        Transform peekPoint;

        public bool isMuted = true;

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

        // Private Variables
        RectTransform playerBetAmountRT;
        Slot slot1;

        public void CallHandler()
        {
            slider.mBetButton.SetActive(false);            
            ToggleSliderButtons(false);
            slider.mSlider.SetActive(false);
            this.photonView.RPC("UpdateWallet", PhotonTargets.All, -(int)slider.mSlider.GetComponent<Slider>().value);
            if (gameMaster.playerSlots[slotID].chips.gameObject.GetActive() && isBetting)
            {
                this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                SetLocalGlowEffectChips("Untagged", false);
                this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                StartCoroutine(PlayChipSFX());
            }
            this.photonView.RPC("setHasCalled", PhotonTargets.All, true);
            gameMaster.photonView.RPC("CallHandler", PhotonTargets.All);
        }

        public void CheckHandler()
        {            
            ToggleSliderButtons(false);
            slider.mSlider.SetActive(false);
            if (gameMaster.playerSlots[slotID].chips.gameObject.GetActive() && isBetting)
            {
                this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                SetLocalGlowEffectChips("Untagged", false);
                this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                StartCoroutine(PlayChipSFX());
            }
            this.photonView.RPC("setHasCalled", PhotonTargets.All, true);
            gameMaster.photonView.RPC("CheckHandler", PhotonTargets.All);
        }

        public void BetHandler()
        {            
            slider.mBetButton.SetActive(false);
            slider.mSlider.SetActive(false);
            ToggleSliderButtons(false);
            this.photonView.RPC("UpdateWallet", PhotonTargets.All, -(int)slider.mSlider.GetComponent<Slider>().value);
            if (gameMaster.playerSlots[slotID].chips.gameObject.GetActive() && isBetting)
            {
                this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                SetLocalGlowEffectChips("Untagged", false);
                this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                StartCoroutine(PlayChipSFX());
            }
            this.photonView.RPC("setHasBet", PhotonTargets.All, true);
            this.photonView.RPC("setHasCalled", PhotonTargets.All, true);
            gameMaster.photonView.RPC("BetHandler", PhotonTargets.All);
        }

        public void RaiseHandler()
        {
            slider.mBetButton.SetActive(false);            
            ToggleSliderButtons(false);
            slider.mSlider.SetActive(false);
            this.photonView.RPC("UpdateWallet", PhotonTargets.All, -(int)slider.mSlider.GetComponent<Slider>().value);
            if (gameMaster.playerSlots[slotID].chips.gameObject.GetActive() && isBetting)
            {
                this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                SetLocalGlowEffectChips("Untagged", false);
                this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                StartCoroutine(PlayChipSFX());
            }
            this.photonView.RPC("setHasBet", PhotonTargets.All, true);
            this.photonView.RPC("setHasCalled", PhotonTargets.All, true);
            gameMaster.photonView.RPC("RaiseHandler", PhotonTargets.All, (int)slider.mSlider.GetComponent<Slider>().value);
        }

        public void AllInHandler()
        {
            slider.mBetButton.SetActive(false);
            ToggleSliderButtons(false);
            slider.mSlider.SetActive(false);
            this.photonView.RPC("UpdateWallet", PhotonTargets.All, -(int)slider.mSlider.GetComponent<Slider>().maxValue);
            if (gameMaster.playerSlots[slotID].chips.gameObject.GetActive() && isBetting)
            {
                this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                SetLocalGlowEffectChips("Untagged", false);
                this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                StartCoroutine(PlayChipSFX());
            }
            this.photonView.RPC("setHasBet", PhotonTargets.All, true);
            this.photonView.RPC("setHasCalled", PhotonTargets.All, true);
            gameMaster.photonView.RPC("AllInHandler", PhotonTargets.All, (int)slider.mSlider.GetComponent<Slider>().maxValue);
        }

        [PunRPC]
        void setHasBet(bool _status)
        {
            this.hasBet = _status;
        }

        [PunRPC]
        void setHasCalled(bool _status)
        {
            this.hasCalled = _status;
        }

        [PunRPC]
        void toggleHasPlayed(bool state)
        {
            hasPlayed = state;
        }

        [PunRPC]
        void setIsBetting(bool _state)
        {
            this.isBetting = _state;
        }

        [PunRPC]
        void ClearTurnHighlight()
        {
            if (this.photonView.isMine)
            {
                    // New version
                foreach (TextSlot _slot in gameMaster.playerNameElements)
                {
                    _slot.highlight.tag = "Untagged";
                }
                gameMaster.mHighlighter.RefreshHighlight();
            }

        }

        void ToggleSliderButtons(bool _flag)
        {
            slider.callButton.GetComponent<Button>().interactable = _flag;
            slider.allInButton.GetComponent<Button>().interactable = _flag;
            slider.raiseButton.GetComponent<Button>().interactable = _flag;
            slider.mCheckButton.GetComponent<Button>().interactable = _flag;
        }

        [PunRPC]
        void startTurn(int minimumBetAmount, bool hasBet)
        {
            this.photonView.RPC("setHasCalled", PhotonTargets.All, false);
            this.photonView.RPC("setHasBet", PhotonTargets.All, false);
            // assume ID is known.
            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (_slotID < 0)
                _slotID += 6; // max number of players.

            // Setting the glow effect to indicate the current player
            //GameObject _nameUI = GameObject.Find("Canvas").transform.Find("playerNametags").Find("txtSlot" + _slotID).gameObject;            
            //_nameUI.transform.Find("Plane").gameObject.tag = "Occludee";
            gameMaster.playerNameElements[_slotID].highlight.tag = "Occludee";
            gameMaster.mHighlighter.RefreshHighlight();

            if (wallet > 0)
            {
                slider.mSlider.GetComponent<Slider>().maxValue = wallet;

                if (slider.mSlider.GetComponent<Slider>().maxValue < minimumBetAmount)
                    slider.mSlider.GetComponent<Slider>().minValue = slider.mSlider.GetComponent<Slider>().maxValue;
                else
                    slider.mSlider.GetComponent<Slider>().minValue = minimumBetAmount;
            }
            else
            {
                gameMaster.sliderButtons.raiseButton.GetComponent<Button>().interactable = false;
                slider.mSlider.GetComponent<Slider>().maxValue = 0;
                slider.mSlider.GetComponent<Slider>().minValue = 0;
            }
                

            if (this.photonView.isMine)
            {
                // If the player object belongs to the client.
                ToggleSliderButtons(true);
                slider.mSlider.SetActive(true);
                setIsBetting(true);
                txtBet.SetActive(true);
                this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Occludee", true);
                SetGlowEffectChips("Occludee", true);
                this.photonView.RPC("setIsBetting", PhotonTargets.All, true);
                this.photonView.RPC("setBettingUI", PhotonTargets.Others, true);

                if (hasBet)
                {
                    slider.mCheckButton.SetActive(false);
                    slider.callButton.SetActive(true);
                }
                else
                {
                    slider.mCheckButton.SetActive(true);
                    slider.callButton.SetActive(false);
                }
               
            }
        }

        [PunRPC]
        void setBettingUI(bool _state)
        {
            // assume ID is known.
            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (_slotID < 0)
                _slotID += 6; // max number of players.

            // Our slots go Last Player - First Player - Second Player - ...
            // So use out "slotID" to find the correct slot, which has stored
            // the game objects we wish to interact with.
            Slot _slot = gameMaster.playerSlots[_slotID];

            // Our slots go Last Player - First Player - Second Player - ...
            // So use out "slotID" to find the correct slot, which has stored
            // the game object that has the text element we wish to interact.
            this.mTxtBetAmt = gameMaster.betTextSlots[_slotID];

            // Find the world coordinates of the position
            // of where we wish the bet text to go.
            Vector3 pos = _slot.uiPos.position; 

            // Cache it's rect transform.
            playerBetAmountRT = this.mTxtBetAmt.GetComponent<RectTransform>();


            // Calculating position of the UI element
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(pos);
            Vector2 WorldObject_ScreenPosition = new Vector2(
             ((ViewportPosition.x * gameMaster.mCanvasRT.sizeDelta.x) - (gameMaster.mCanvasRT.sizeDelta.x * 0.5f)),
             ((ViewportPosition.y * gameMaster.mCanvasRT.sizeDelta.y) - (gameMaster.mCanvasRT.sizeDelta.y * 0.5f)));

            // Assign the new achored position.
            playerBetAmountRT.anchoredPosition = WorldObject_ScreenPosition;

            // Make it visible.
            this.mTxtBetAmt.SetActive(_state);
        }

        [PunRPC]
        void SetId(int _id)
        {            
            id = (_id % 6); // 6 is max players, we need to loop back around as 6th player equals 0th slot, so slot0 is pos 6.
            slot = gameMaster.playerSlots[id].gameObject.transform; //Object.Find("PlayerSlotPositions").transform.Find("slot" + id);
            slot1 = gameMaster.playerSlots[1];
            //localSlot = GameObject.Find("PlayerSlotPositions").transform.Find("slot1");
            //chips = slot.Find("mChips");
            chips = slot1.chips;
            //originalCardPos = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("PlayerHandPosition").transform.position;
            originalCardPos = slot1.PlayerHandPosition.position;
            //cards = localSlot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");            
            cards = slot1.PlayerHand;
            //localCards = localSlot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
            targetLocation = originalCardPos;
            //playerPos = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.position;
            playerPos = slot1.gameObject.transform.position;
            playerPos = new Vector3(playerPos.x, 0, playerPos.z);
            //foldPoint = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("foldPoint").transform.position;
            foldPoint = slot1.foldPoint.position;
            //flopPos = GameObject.Find("Flop").transform.position;
            flopPos = gameMaster.FlopPos.position;

            slider = gameMaster.sliderButtons;
            slider.mSlider.GetComponent<Slider>().maxValue = wallet;
            txtBet = slider.mBetTextAmount;
            //peekPoint = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").Find("peekPoint");
            peekPoint = slot1.peekPoint;

            //Transform slot1 = GameObject.Find("PlayerSlotPositions").transform.Find("slot1");

            //Transform _cards = slot1.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");

            cardAnim0 = slot1.mCardRoot0.GetComponent<Animator>();
            cardAnim1 = slot1.mCardRoot1.GetComponent<Animator>();
            this.enabled = true;

            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            if  (_slotID == 1)
            {
                // Slider handling
                slider.callButton.GetComponent<Button>().onClick.AddListener(() => { CallHandler(); });
                slider.allInButton.GetComponent<Button>().onClick.AddListener(() => { AllInHandler(); });
                slider.raiseButton.GetComponent<Button>().onClick.AddListener(() => { RaiseHandler(); });
                slider.mCheckButton.GetComponent<Button>().onClick.AddListener(() => { CheckHandler(); });
                slider.mBetButton.GetComponent<Button>().onClick.AddListener(() => { BetHandler(); });
            }
        }

        [PunRPC]
        void SetUI()
        {
            // assume ID is known.
            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (_slotID < 0)
                _slotID += 6; // max number of players.

            //slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + _slotID);

            //chips = slot.Find("mChips");

            //this.mTxtBetAmt = GameObject.Find("Canvas").transform.Find("playerBets").transform.Find("txtSlot"+ _slotID).gameObject;
            this.mTxtBetAmt = gameMaster.betTextSlots[_slotID];

            Vector3 pos = slot1.uiPos.position;
            Vector3 screenPos = Camera.main.WorldToViewportPoint(pos);
            screenPos.x *= gameMaster.mCanvasRT.rect.width;
            screenPos.y *= gameMaster.mCanvasRT.rect.height;
            this.mTxtBetAmt.GetComponent<Text>().text = this.betAmount.ToString();

            //Transform playerPosition = slot.Find("player");
            Transform playerPosition = this.transform;
            Vector3 playerPos = playerPosition.position;

            //GameObject mPlayerNameTag = GameObject.Find("Canvas").transform.Find("playerNametags").
            //  Find("txtSlot" + _slotID).Find("text").gameObject;

            GameObject mPlayerNameTag = gameMaster.playerNameElements[_slotID].gameObject;

            gameMaster.playerWallets[_slotID].SetActive(true);
            gameMaster.playerWalletTexts[_slotID].text = wallet.ToString();
            //GameObject mPlayerName = GameObject.Find("Canvas").transform.Find("playerNametags").
            //  Find("txtSlot" + _slotID).gameObject;



            RectTransform rtUI;
            //RectTransform canvasRT;

            rtUI = mPlayerNameTag.GetComponent<RectTransform>();
            //canvasRT = GameObject.Find("Canvas").GetComponent<Canvas>().GetComponent<RectTransform>();


            // Calculating position of the UI element
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(playerPosition.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
             ((ViewportPosition.x * gameMaster.mCanvasRT.sizeDelta.x) - (gameMaster.mCanvasRT.sizeDelta.x * 0.5f)),
             ((ViewportPosition.y * gameMaster.mCanvasRT.sizeDelta.y) - (gameMaster.mCanvasRT.sizeDelta.y * 0.5f)));

            rtUI.anchoredPosition = WorldObject_ScreenPosition;

            gameMaster.playerNameElements[_slotID].nameTag.GetComponent<Text>().text = "player" + this.id;
            gameMaster.playerNameElements[_slotID].nameTag.GetComponent<Text>().color = this.GetComponent<Renderer>().material.color;
            mPlayerNameTag.SetActive(true);

            gameMaster.mHighlighter.RefreshHighlight();

        }

        [PunRPC]
        void TogglePeekingSprite(bool _flag)
        {
            // assume ID is known.
            int _slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (_slotID < 0)
                _slotID += 6; // max number of players.

            //GameObject Eyes = GameObject.Find("Canvas").transform.Find("PeekingEyes").transform.Find("IsPeeking" + _slotID).gameObject;
            GameObject Eyes = gameMaster.playerSlots[_slotID].mEyes;

            //Vector3 _pos = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + _slotID).Find("PlayerHandPosition").position;
            Vector3 _pos = gameMaster.playerSlots[_slotID].PlayerHandPosition.position;

            RectTransform rtUI = Eyes.GetComponent<RectTransform>();

            // Calculating position of the UI element
            Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(_pos);
            Vector2 WorldObject_ScreenPosition = new Vector2(
             ((ViewportPosition.x * gameMaster.mCanvasRT.sizeDelta.x) - (gameMaster.mCanvasRT.sizeDelta.x * 0.5f)),
             ((ViewportPosition.y * gameMaster.mCanvasRT.sizeDelta.y) - (gameMaster.mCanvasRT.sizeDelta.y * 0.5f)));

            rtUI.anchoredPosition = WorldObject_ScreenPosition;

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

            //Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
            //Transform slot = gameMaster.playerSlots[slotID]
            //Transform _cards = slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
            //Transform _cards = gameMaster.playerSlots[slotID].PlayerHand;

            //_cards.transform.Find("Card0").Find("PlayingCard").Find("Card").tag = _tag;
            //_cards.transform.Find("Card1").Find("PlayingCard").Find("Card").tag = _tag;

            gameMaster.playerSlots[slotID].mCardChild0.tag = _tag;
            gameMaster.playerSlots[slotID].mCardChild1.tag = _tag;

            gameMaster.mHighlighter.RefreshHighlight();
        }

        void SetLocalGlowEffectCards(string _tag)
        {

            //Transform _cards = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
            //_cards.transform.Find("Card0").Find("PlayingCard").Find("Card").tag = _tag;
            //_cards.transform.Find("Card1").Find("PlayingCard").Find("Card").tag = _tag;
            slot1.mCardChild0.tag = _tag;
            slot1.mCardChild1.tag = _tag;
            gameMaster.mHighlighter.RefreshHighlight();
        }

        [PunRPC]
        void UpdateWallet(int _wallet)
        {
            slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (slotID < 0)
                slotID += 6; // max number of players.

            wallet += _wallet;
            gameMaster.playerWalletTexts[slotID].text = wallet.ToString();
        }

        [PunRPC]
        void SetGlowEffectChips(string _tag, bool _isClicked)
        {
            slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
            // Raaaaaaging
            if (slotID < 0)
                slotID += 6; // max number of players.

            //Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
            //Transform _chips = slot.transform.Find("mChips");

            foreach (GameObject _chip in gameMaster.playerSlots[slotID].mHighlightChips)
            {
                _chip.tag = _tag;
            }
            //_chips.Find("Chips0").tag = _tag;
            //_chips.Find("Chips1").tag = _tag;
            //_chips.Find("Chips2").tag = _tag;
            //_chips.gameObject.GetComponent<Animator>().SetBool("IsClicked", _isClicked);
            gameMaster.playerSlots[slotID].chips.gameObject.GetComponent<Animator>().SetBool("IsClicked", _isClicked);
            gameMaster.mHighlighter.RefreshHighlight();
        }

        void SetLocalGlowEffectChips(string _tag, bool _isClicked)
        {
            //Transform _chips = GameObject.Find("PlayerSlotPositions").transform.Find("slot1").transform.Find("mChips");
            //_chips.Find("Chips0").tag = _tag;
            //_chips.Find("Chips1").tag = _tag;
            //_chips.Find("Chips2").tag = _tag;
            //_chips.gameObject.GetComponent<Animator>().SetBool("IsClicked", _isClicked);

            foreach (GameObject _chip in slot1.mHighlightChips)
            {
                _chip.tag = _tag;
            }
            slot1.chips.gameObject.GetComponent<Animator>().SetBool("IsClicked", _isClicked);
            gameMaster.mHighlighter.RefreshHighlight();
        }

        [PunRPC]
        void DealHandTest(int[] players)
        {
            //localCards.gameObject.SetActive(true);
            //localSlot.Find("mChips").gameObject.SetActive(true);
            slot1.PlayerHand.gameObject.SetActive(true);
            slot1.chips.gameObject.SetActive(true);

            for (int i=0; i < players.Length; i++)
            {
                slotID = (players[i] - (PhotonNetwork.player.ID - 1)) % 6;
                // Raaaaaaging
                if (slotID < 0)
                    slotID += 6; // max number of players.

                
                //Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);                
                //slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand").gameObject.SetActive(true);
                //slot.transform.Find("mChips").gameObject.SetActive(true);

                Slot _slot = gameMaster.playerSlots[slotID];
                _slot.PlayerHand.gameObject.SetActive(true);
                _slot.chips.gameObject.SetActive(true);
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
                InitializePlayerHand();
            }

        }

        void InitializePlayerHand()
        {
            if (hand.Count == 2)
            {
                //GameObject _Card0 = localCards.Find("Card0").Find("PlayingCard").Find("Card").gameObject;
                //GameObject _Card1 = localCards.Find("Card1").Find("PlayingCard").Find("Card").gameObject;

                GameObject _Card0 = slot1.mCardChild0;
                GameObject _Card1 = slot1.mCardChild1;

                Texture2D _tex = gameMaster.cardTextures[((int)hand[0].suit * 13) + (int)hand[0].rank - 1];
                _Card0.GetComponent<Renderer>().materials[0].mainTexture = _tex;
                _Card0.GetComponent<Renderer>().materials[0].SetTexture("_EmissionMap", _tex);

                _tex = gameMaster.cardTextures[((int)hand[1].suit * 13) + (int)hand[1].rank - 1];
                _Card1.GetComponent<Renderer>().materials[0].mainTexture = _tex;
                _Card1.GetComponent<Renderer>().materials[0].SetTexture("_EmissionMap", _tex);

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

            if (isMuted)
            {
                chipsCollideSFX.mute = isMuted;
                chipsCollideSFX2.mute = isMuted;
                chipsCollideSFX3.mute = isMuted;
                cardsFlipSFX.mute = isMuted;
            }
            else
            {
                chipsCollideSFX.mute = isMuted;
                chipsCollideSFX2.mute = isMuted;
                chipsCollideSFX3.mute = isMuted;
                cardsFlipSFX.mute = isMuted;
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
                // Meh
                if (slotID < 0)
                    slotID += 6; // max number of players.

                //Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
                Transform _cards = gameMaster.playerSlots[slotID].PlayerHand;
                //Transform _cards = slot.Find("PlayerHandPosition").transform.Find("PlayerHand");
                if (slot1 != null && _cards != null)
                {
                    // Network player, recieve data            
                    //_cards.localPosition = Vector3.Lerp(_cards.localPosition, this.correctPos, Time.deltaTime * 5);
                    _cards.localPosition = Vector3.Lerp(_cards.localPosition, this.correctPos, Time.deltaTime * 5);
                }

                if (this.isBetting)
                {   
                    this.mTxtBetAmt.GetComponent<Text>().text = ((int)Mathf.Floor(Mathf.Lerp(
                       int.Parse(this.mTxtBetAmt.GetComponent<Text>().text), this.betAmount, Time.deltaTime * 5))).ToString();
                }
            }
            else
            {
                if (this.isBetting)
                {
                    Slider sb = slider.mSlider.GetComponent<Slider>();
                    //this.betAmount = (int)Mathf.Floor(sb.value * 500);
                    this.betAmount = (int)Mathf.Floor(sb.value);
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
                //Debug.Log("Mouse down");
                currentPos = previousPos = Input.mousePosition;
                ray = Camera.main.ScreenPointToRay(currentPos);

                if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player0" && hit.transform.gameObject.layer != LayerMask.NameToLayer("chips"))
                {
                    initX = currentPos.x;
                    initY = currentPos.y;
                    initialDistance = (cards.position - peekPoint.position).magnitude;
                    isHeld = true;
                    slider.mSlider.SetActive(false);
                    txtBet.SetActive(false);
                    this.photonView.RPC("SetGlowEffectCards", PhotonTargets.Others, "Occludee");
                    SetLocalGlowEffectCards("Occludee");
                    cardsFlipSFX.Play();
                    // Disable chips when peeking at cards?
                    /*
                    if (slot1.chips.gameObject.GetActive() && isBetting)
                    {
                        this.photonView.RPC("SetGlowEffectChips", PhotonTargets.Others, "Untagged", false);
                        SetLocalGlowEffectChips("Untagged", false);
                        this.photonView.RPC("setIsBetting", PhotonTargets.All, false);
                        this.photonView.RPC("setBettingUI", PhotonTargets.Others, false);
                        StartCoroutine(PlayChipSFX());
                    }
                    */
                }
                else if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player0" && (hit.transform.gameObject.layer == LayerMask.NameToLayer("chips")))
                {
                    slider.mSlider.SetActive(true);
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
                    slider.mSlider.SetActive(false);
                    txtBet.SetActive(false);
                    slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
                    // Raaaaaaging
                    if (slotID < 0)
                        slotID += 6; // max number of players.

                    //Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);
                    //Transform _cards = slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
                    if (gameMaster.playerSlots[slotID].chips.gameObject.GetActive() && isBetting)
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

                        //this.photonView.RPC("TogglePeekingSprite", PhotonTargets.Others, true);
                    }
                    else
                    {
                        // is moving
                        Debug.Log("Mouse is moving");
                        isStationary = false;
                        isDrag = true;
                        Vector3 newPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Input.mousePosition.z));
                        targetLocation = new Vector3(cards.position.x, cards.position.y, newPos.z);
                        if (!isMoved)
                        {
                            cardsFlipSFX.Play();
                            isMoved = true;
                        }
                        
                    }
                }
            }

            // Player released the cards
            if (Input.GetMouseButtonUp(0) && PhotonNetwork.player.ID == this.id)
            {
                isReleased = false;
                isMoved = false;
                //Debug.Log("Mouse released");
                isLetGo = true;
                isHeld = false;
                isDrag = false;
                isStationary = false;
                slotID = (this.id - (PhotonNetwork.player.ID - 1)) % 6;
                // Raaaaaaging
                if (slotID < 0)
                    slotID += 6; // max number of players.
                //Transform slot = GameObject.Find("PlayerSlotPositions").transform.Find("slot" + slotID);

                //Transform _cards = slot.transform.Find("PlayerHandPosition").transform.Find("PlayerHand");
                if (gameMaster.playerSlots[slotID].PlayerHand.gameObject.GetActive())
                {
                    this.photonView.RPC("SetGlowEffectCards", PhotonTargets.Others, "Untagged");
                    SetLocalGlowEffectCards("Untagged");
                }
                this.photonView.RPC("TogglePeekingSprite", PhotonTargets.Others, false);
                cardAnim0.SetBool("IsPeeking", false);
                cardAnim1.SetBool("IsPeeking", false);
            }

            // Lerp the hand back to it's original start position and orientation
            if (isLetGo && !isStationary && !isDrag && !isHeld && PhotonNetwork.player.ID == this.id)
            {
                if (cards != null)
                {
                    cards.position = Vector3.Lerp(cards.position, originalCardPos, 10 * Time.deltaTime);
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
                    
                    if (cards.position.z <= peekPoint.position.z)
                    {
                        cards.position = new Vector3(cards.position.x, cards.position.y, peekPoint.position.z);
                        cardAnim0.SetBool("IsPeeking", true);
                        cardAnim1.SetBool("IsPeeking", true);
                        this.photonView.RPC("TogglePeekingSprite", PhotonTargets.Others, true);
                    }
                    else if (cards.position.z >= foldPoint.z)
                    {
                        cards.position = new Vector3(cards.position.x, cards.position.y, foldPoint.z);                      
                    }
                    else
                    {
                        cardAnim0.SetBool("IsPeeking", false);
                        cardAnim1.SetBool("IsPeeking", false);
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
            if (slot1 != null)
            {
                if (stream.isWriting)
                {
                    // We own this player, send other players our data
                    stream.SendNext(slot1.PlayerHand.localPosition);
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
                // Modulo is negative?
                if (slotID < 0)
                    slotID += 6; // max number of players.

                //this.transform.SetParent(GameObject.Find("PlayerSlotPositions").transform.Find("slot"+ slotID).transform);
                this.transform.SetParent(gameMaster.playerSlots[slotID].gameObject.transform);
                this.transform.localPosition = new Vector3(0, 0, 0);
                this.name = "player";
            }

        }

        [PunRPC]
        public void PostBlind()
        {
            if (this.photonView.isMine)
            {
                slider.mBetButton.SetActive(true);
                slider.mCheckButton.SetActive(false);
                slider.callButton.SetActive(false);
            }

        }

        float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        void OnGUI()
        {
            GUI.Label(new Rect(0, 40, 200, 200), "Player"+(PhotonNetwork.player.ID).ToString());
            //GUI.Label(new Rect(0, 90, 200, 200), "Phase: " + mString);
        }
    }
}

