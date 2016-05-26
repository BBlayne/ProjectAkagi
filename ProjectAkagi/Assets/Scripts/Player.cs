﻿using UnityEngine;
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
        Vector3 lastTouch;

        private Vector3 currentPos;

        public Vector3 originalCardPos;

        bool isLetGo = false;
        bool isHeld = false;
        bool isStationary = false;
        bool isDrag = false;

        private Vector3 targetLocation;
        private Vector3 playerPos;

        Vector3 flopPos;
        GameObject slider;
        GameObject txtBet;

        public bool isPeeking = false;
        public bool isBetting = false;

        Vector3 foldPoint;


        public void SendBetting()
        {
            if (OnClicked != null)
            {
                OnClicked(id);
            }
        }

        [PunRPC]
        void SetId(int _id)
        {            
            id = _id;
            originalCardPos = GameObject.Find("PlayerHandPosition" + (id - 1)).transform.position;
            //cards = GameObject.Find("Hands").transform.Find("PlayerHandPosition" + (id - 1));
            cards = GameObject.Find("PlayerHandPosition" + (id - 1)).transform;
            targetLocation = originalCardPos;
            playerPos = GameObject.Find("PlayerPositions").transform.Find("playerPos" + (id - 1)).position;
            playerPos = new Vector3(playerPos.x, 0, playerPos.z);
            foldPoint = GameObject.Find("foldPoint").transform.position;
            flopPos = GameObject.Find("Flop").transform.position;
            slider = GameObject.Find("Canvas").transform.Find("Panel").transform.Find("scrollBet").gameObject;
            txtBet = GameObject.Find("Canvas").transform.Find("txtBet").gameObject;
        }

        [PunRPC]
        void InitColour(int id)
        {
            Debug.Log("InitColour " + id);
            //this.enabled = true;
            if (PhotonNetwork.player.ID == id)
            {
                //this.GetComponent<Renderer>().material = gameMaster.mats[id - 1];
            }
            else
            {

            }

            this.GetComponent<Renderer>().material = gameMaster.mats[id - 1];
        }

        void Awake()
        {
            Debug.Log("Awake");
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
        void LateUpdate () {
            RaycastHit hit;

            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse down");
                currentPos = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player" + (id - 1) && hit.transform.gameObject.layer != LayerMask.NameToLayer("chips"))
                {
                    initX = Input.mousePosition.x;
                    initY = Input.mousePosition.y;
                    initialDistance = (cards.position - playerPos).magnitude;
                    Debug.Log("Clicked Hand for player" + (id - 1));
                    isHeld = true;
                    slider.SetActive(false);
                    txtBet.SetActive(false);
                    cards.transform.Find("Card0").Find("Occludee0").tag = "Occludee";
                    cards.transform.Find("Card0").Find("Occludee1").tag = "Occludee";
                    cards.transform.Find("Card1").Find("Occludee0").tag = "Occludee";
                    cards.transform.Find("Card1").Find("Occludee1").tag = "Occludee";
                    Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
                }
                else if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player" + (id - 1) && (hit.transform.gameObject.layer == LayerMask.NameToLayer("chips")))
                {
                    slider.SetActive(true);
                    txtBet.SetActive(true);
                    GameObject.Find("mChips").transform.Find("Chips0").tag = "Occludee";
                    GameObject.Find("mChips").transform.Find("Chips1").tag = "Occludee";
                    GameObject.Find("mChips").transform.Find("Chips2").tag = "Occludee";
                    Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
                }
                else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Default") && EventSystem.current.currentSelectedGameObject == null)
                {                    
                    slider.SetActive(false);
                    txtBet.SetActive(false);
                    GameObject.Find("mChips").transform.Find("Chips0").tag = "Untagged";
                    GameObject.Find("mChips").transform.Find("Chips1").tag = "Untagged";
                    GameObject.Find("mChips").transform.Find("Chips2").tag = "Untagged";
                    Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
                }

            }

            // if we're holding down the left mouse button
            // we are either holding stationary, or dragging.
            if (Input.GetMouseButton(0))
            {
                if (isHeld)
                {
                    // if is not moving
                    if (Mathf.Approximately(Input.mousePosition.x, currentPos.x) && 
                            Mathf.Approximately(Input.mousePosition.y, currentPos.y))
                    {
                        Debug.Log("Mouse is stationary");       
                        if (cards != null)
                        {

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
                    }
                }

                if (txtBet.GetActive())
                {
                    Vector3 handlePos = slider.transform.Find("Sliding Area").Find("Handle").transform.position;
                    txtBet.transform.position = new Vector3(handlePos.x, handlePos.y + 50, handlePos.z);
                }
            }

            txtBet.GetComponent<Text>().text = (slider.GetComponent<Scrollbar>().value * 500).ToString();

            // Player released the cards
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Mouse released");
                isLetGo = true;
                isHeld = false;
                isDrag = false;
                isStationary = false;
                cards.transform.Find("Card0").Find("Occludee0").tag = "Untagged";
                cards.transform.Find("Card0").Find("Occludee1").tag = "Untagged";
                cards.transform.Find("Card1").Find("Occludee0").tag = "Untagged";
                cards.transform.Find("Card1").Find("Occludee1").tag = "Untagged";
                Camera.main.GetComponent<HighlightsPostEffect>().RefreshHighlight();
            }

            // Lerp the hand back to it's original start position and orientation
            if (isLetGo && !isStationary && !isDrag && !isHeld)
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
            else if (isDrag || isStationary)
            {
                
                
                // Updating our coordinates, still needs polish              
                if (cards != null)
                {
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
            
            /*
            if (Input.touchCount > 0)
            {                
                touch = Input.touches[0];
                if (touch.phase == TouchPhase.Began)
                {
                    Ray ray = Camera.main.ScreenPointToRay(touch.position);
                    
                    if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player" + (id-1))
                    {
                        
                        initX = touch.position.x;
                        initY = touch.position.y;
                        cards = hit.transform;
                        Debug.Log("X Movement %: " + map(touch.position.x, initX, Screen.width / 2, 0, 100));
                        Debug.Log("Y Movement %: " + map(touch.position.y, initY, Screen.height / 2, 0, 100));
                        screenPoint = Camera.main.WorldToScreenPoint(cards.position);
                        offset = cards.position - Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, screenPoint.z));

                        flag = true;
                    }
                }

                if ((touch.phase == TouchPhase.Stationary) && cards != null && flag)
                {
                    //cards.localPosition = Vector3.Lerp(cards.localPosition, target, 5 * Time.deltaTime);    
                    cards.position = Vector3.Lerp(cards.position, target, 5 * Time.deltaTime);
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    if (flag)
                    {
                        //lastTouch = Camera.main.ScreenToWorldPoint(new Vector3(Input.GetTouch(0).position.x,
                        //Input.GetTouch(0).position.y, Camera.main.nearClipPlane));
                        Vector3 cursorPoint = new Vector3(touch.position.x, touch.position.y, screenPoint.z);
                        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;


                        Debug.Log("X Movement %: " + map(touch.position.x, initX, Screen.width / 2, 0, 100));
                        Debug.Log("Y Movement %: " + map(touch.position.y, initY, Screen.height / 2, 0, 100));
                        //cards.position = new Vector3(lastTouch.x, -0.618f, -0.9154338f);
                        cards.position = cursorPosition;


                    }                        

                    scrollPosition.y += touch.deltaPosition.y * 1;
                    scrollPosition.x += touch.deltaPosition.x * 1;

                }
            }
            else
            {
                flag = false;
                scrollPosition.y = 0;
                scrollPosition.x = 0;
            }
            */
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
            if (cards != null)
            {
                if (stream.isWriting)
                {
                    // We own this player, send other players our data
                    stream.SendNext(cards.position);
                }
                else
                {
                    // Network player, recieve data            
                    cards.position = (Vector3)stream.ReceiveNext();
                }
            }
        }

        float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        void OnGUI()
        {
            //GUI.Label(new Rect(0, 40, 200, 200), scrollPosition.y.ToString());
            //GUI.Label(new Rect(0, 80, 200, 200), scrollPosition.x.ToString());
            //GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString
            GUI.Label(new Rect(0, 40, 200, 200), "Player"+(this.photonView.ownerId - 1).ToString());
        }
    }
}

