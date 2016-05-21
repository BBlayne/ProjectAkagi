using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Poker
{
    public class Player : Photon.MonoBehaviour {
        public List<Poker.Card> hand = new List<Card>();
        Vector2 scrollPosition;
        Touch touch;
        bool flag = false;
        public int wallet = 0;      
        public static int id;
        float initX = 0;
        float initY = 0;
        Vector3 target = new Vector3(-0.611f, -0.12f, -0.43f);
        // Events for turn action handling
        public delegate void IsBetting(int playerId);
        public static event IsBetting OnClicked;
        public GameController gameMaster = null;
        Transform cards;
        Vector3 lastTouch;
        private Vector3 screenPoint;
        private Vector3 offset;
        private Vector2 worldStartPoint;
        private Vector3 currentPos;
        public Vector3 originalCardPos;
        bool isLetGo = false;
        bool isHeld = false;
        bool isStationary = false;
        bool isDrag = false;
        private Vector3 targetLocation;

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
            targetLocation = originalCardPos;
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
                //Debug.Log("Mouse down");
                currentPos = Input.mousePosition;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit) && hit.transform.tag == "player" + (id - 1))
                {
                    initX = Input.mousePosition.x;
                    initY = Input.mousePosition.y;
                    Debug.Log("Mouse down");
                    cards = hit.transform;
                    screenPoint = Camera.main.WorldToScreenPoint(cards.position);
                    offset = cards.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                }       
                else if (!isLetGo)
                {
                    cards = null;
                }        

            }

            // if we're holding down the left mouse button
            // we are either holding stationary, or dragging.
            if (Input.GetMouseButton(0))
            {

                Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);                               
                Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;

                // if is not moving
                if (Mathf.Approximately(Input.mousePosition.x, currentPos.x) && 
                        Mathf.Approximately(Input.mousePosition.y, currentPos.y) && 
                        Mathf.Approximately(Input.mousePosition.z, currentPos.z))
                {
                    Debug.Log("Mouse is stationary");       
                    if (cards != null)
                    {
                        target = new Vector3(cards.position.x, target.y, cursorPosition.z);
                        if (!Mathf.Approximately(cards.position.y, target.y))
                        {
                            isStationary = true;
                            targetLocation = Vector3.Lerp(cards.position, target, 10 * Time.deltaTime);
                        }
                    }                 

                }
                else
                {
                    // is moving
                    Debug.Log("Mouse is moving");
                    isStationary = false;
                    isDrag = true;
                    targetLocation = new Vector3(cursorPosition.x, cards.position.y, cursorPosition.z);
                    Debug.Log("X Movement %: " + map(Input.mousePosition.x, initX, Screen.width / 2, 0, 100));
                    Debug.Log("Y Movement %: " + map(Input.mousePosition.y, initY, Screen.height / 2, 0, 100));
                    float yCardPos = map(Input.mousePosition.x, initX, Screen.width / 2, 0, 100);
                    float xCardPos = map(Input.mousePosition.y, initY, Screen.height / 2, 0, 100);

                    if ((yCardPos < 50.0f) && (xCardPos > 0.0f))
                    {
                        float rot = Mathf.LerpAngle(0, 180, (xCardPos/100));
                        Debug.Log("rotating by: " + rot);
                        if (cards != null)
                        {
                            cards.Find("Card0").transform.Rotate(new Vector3(0, rot, 0));
                        }
                    }
                    else
                    {
                        //cards.Find("Card0").transform.Rotate(new Vector3(0, 1, 0), 0, Space.Self);
                    }
                }

            }


            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Mouse released");
                isLetGo = true;
                isHeld = false;
                isDrag = false;
                isStationary = false;
            }

            if (isLetGo && !isStationary && !isDrag)
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
            else if (isDrag || isStationary)
            {
                Debug.Log("Updating position");                
                if (cards != null)
                {
                    cards.position = targetLocation;
                    screenPoint = Camera.main.WorldToScreenPoint(cards.position);
                    offset = cards.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
                }
            }

            currentPos = Input.mousePosition;

            /*
            if (Input.GetMouseButton(0))
            {
                if (isLetGo)
                {
                    if (!(Mathf.Approximately(Input.mousePosition.x, currentPos.x) && Mathf.Approximately(Input.mousePosition.y, currentPos.y)
                    && Mathf.Approximately(Input.mousePosition.z, currentPos.z)))
                    {
                        Debug.Log("Mouse drag");

                        if (cards != null)
                        {
                            Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
                            //target.z = cards.position.z;
                            //Vector3 curPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                            //   Input.mousePosition.y, Camera.main.nearClipPlane));
                            //cards.position = new Vector3(curPosition.x, -0.618f, -0.9154338f);
                            cards.position = new Vector3(cursorPosition.x, cards.position.y, cursorPosition.z);                            
                            isHeld = true;
                            isDrag = true;
                        }

                    }
                    else
                    {

                        if (cards != null)
                        {
                            Debug.Log("Mouse held");
                            target = new Vector3(cards.position.x, target.y, target.z);
                            if (!Mathf.Approximately(cards.position.y, target.y))
                            {
                                cards.position = Vector3.Lerp(cards.position, target, 10 * Time.deltaTime);
                                isHeld = true;
                            }
                        }
                    }

                }
                else
                {
                    if (!(Mathf.Approximately(Input.mousePosition.x, currentPos.x) && Mathf.Approximately(Input.mousePosition.y, currentPos.y)
                        && Mathf.Approximately(Input.mousePosition.z, currentPos.z)))
                    {
                        Debug.Log("Mouse drag");
                        
                        if (cards != null)
                        {
                            Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
                            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;

                            //Vector3 curPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                            //   Input.mousePosition.y, Camera.main.nearClipPlane));
                            //cards.position = new Vector3(curPosition.x, -0.618f, -0.9154338f);
                            cards.position = new Vector3(cursorPosition.x, cards.position.y, cursorPosition.z);
                            //target.z = cards.position.z;
                            isHeld = true;
                            isDrag = true;
                        }
                        
                    }
                    else
                    {                    
                        if (cards != null)
                        {
                            Debug.Log("Mouse held");
                            target = new Vector3(cards.position.x, target.y, target.z);
                            if (!Mathf.Approximately(cards.position.y, target.y))
                            {
                                cards.position = Vector3.Lerp(cards.position, target, 10 * Time.deltaTime);
                                isHeld = true;
                            }
                            
                        }

                    }  
                }           

            }

            currentPos = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("Mouse released"+ originalCardPos);
                isLetGo = true;
                isHeld = false;
                isDrag = false;

            }

            if (isLetGo && !isHeld && !isDrag)
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
            */
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

        }

        float map(float s, float a1, float a2, float b1, float b2)
        {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        void OnGUI()
        {
            GUI.Label(new Rect(0, 40, 200, 200), scrollPosition.y.ToString());
            GUI.Label(new Rect(0, 80, 200, 200), scrollPosition.x.ToString());
            //GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }
    }
}

