using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace GameHolic.PUNTutorial
{

    public class Launcher : MonoBehaviourPunCallbacks
    {

        #region Private Serializable Fields
        ///<summary>
        ///The max number of player per room. When room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The max number of player per room. When room is full, it can't be joined by new players, and so new room will be created.")]
        [SerializeField]
        private byte maxPlayerPerRoom = 4;
        [Tooltip("The UI panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressPanel;


        #endregion
        #region Private Fields  
        ///<summary>
        ///This client's version number. Users are separated from each other by gameVersion (Which allows you to make breaking changes)
        /// </summary>
        private string gameVersion = "1";
        ///<summary>
        ///Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        ///we need to keep track of this to properly adjust the behaviour when we receive call back by Photon.
        ///Typically this is used for the OnConnectedToMaster() callback
        /// </summary>
        bool isConnecting;

        #endregion

        #region MonoBehaviour CallBacks

        ///<summary>
        ///Monobehaviour method called on GameObject by Unity during early initialization phase
        /// </summary>
        private void Awake()
        {
            //#Critical
            //This makes sure we can use PhotonNetwork.LoadLevel() on the master client. 
            // And all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        ///<summary>
        ///Monobehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        private void Start()
        {
            progressPanel.SetActive(false);
            controlPanel.SetActive(true);
        }

        public override void OnConnectedToMaster()
        {
            //We don't want to do anything if we aren't attempting to join a room
            //This case where isConnecting is false is typically when you lost or quit the game,
            //when this level is loaded, OnConnectedToMaster will be called,
            //in that case we don't want to do anything
            if (isConnecting)
            {
                //#Critical: The first we try to do is to join a potential existing room.
                //If there is, good. Else, we'll be called back with OnJoinRandomFailed()
                PhotonNetwork.JoinRandomRoom();
                Debug.Log("Pun Basic Tutorial / Launcher: OnConnectedToMaster was called by PUN");
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            progressPanel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("Pun Basic Tutorial / Launcher: Disconnected was called by PUN with reason {0}", cause);
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. " +
                "No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");


            //#Critical: We Failed to join a random room. Maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions());
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

            //#Critical: We only load if we are the first player, 
            //else we rely on 'PhotonNetwork.AutomaticallySyncScene' to sync our instance scene

            //if(PhotonNetwork.CurrentRoom.PlayerCount==1)
            //{
            //    Debug.Log("We load the 'Room for 1'");

            //    //#Critical
            //    //Load the room level
            //    PhotonNetwork.LoadLevel("Room for 1");
            //}

            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
        }
        #endregion
        
        #region Public Methods

        /// <summary>
        /// Start the connection process. (Connection to Photon Cloud)
        /// - If already connected, we attempt joining a random room
        /// - If not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
            //Keep track of the will to join a room,
            //Because when we come back from the game we will get a callback that we are connected,
            //so We need to know what to do then
            isConnecting = true;
            progressPanel.SetActive(true);
            controlPanel.SetActive(false);
            //we check if we are connected or not, we join if we are,
            //else we initiate the connection to the server.
            if(PhotonNetwork.IsConnected)
            {
                //#Critical we need at this point to attempt joining a Random room.
                //If it fails, we'll get notified in OnJoinRandomFailed()
                //and we'll create one.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                //#Critical, we must first and foremost connect to Photon Online Server.
                PhotonNetwork.GameVersion = gameVersion;
                PhotonNetwork.ConnectUsingSettings();
            }
        }
        #endregion

    }


}
