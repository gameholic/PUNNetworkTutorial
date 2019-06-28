using UnityEngine;
using UnityEngine.EventSystems;

using System.Collections;

using Photon.Pun;


namespace GameHolic.PUNTutorial
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {

        #region Private Fields

        [Tooltip("The Beams GameObject to control")]
        [SerializeField]
        private GameObject beams;
        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        private GameObject playerUiPrefab;  


        //True, when the user is firing
        bool IsFiring;

        #endregion

        #region Public Fields

        [Tooltip("The current Health of our player")]
        public float Health = 1f;
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion

        #region MonoBehaviour Callbacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        private void Awake()
        {
            if(beams==null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }

            //#Important
            //used in GameManager.cs: We keep track of the local player instance to prevent instantiation when levels are synchronized
            if(photonView.IsMine)
            {
                PlayerManager.LocalPlayerInstance = this.gameObject;
            }

            //#Critical
            //We flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when level load.
            DontDestroyOnLoad(this.gameObject);
        }   

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        private void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();
            if(_cameraWork !=null)
            {
                if(photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }

            if(playerUiPrefab !=null)
            {
                GameObject _uiGo = Instantiate(playerUiPrefab);

                //If we can't find receiver component, we will be alerted
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }

#if UNITY_5_4_OR_NEWER
            //Unity 5.4 has a new scene management. Register a method to call CalledOnLevelWasLoaded.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadingMode) =>
            {
                this.CalledOnLevelWasLoaded(scene.buildIndex);
            };
            #endif
            
        }

        /// <summary>
        /// Monobehaviour method called on GameObject by Unity on every frame
        /// </summary>
        private void Update()
        {
            if(photonView.IsMine)
            {
                ProcessInputs();
            }


            if(Health<=0f)
            {
                GameManager.singleton.LeaveRoom();
            }
            //Trigger beams active state
            if(beams!=null && IsFiring!=beams.activeSelf)
            {
                beams.SetActive(IsFiring);
            }


        }


        /// <summary>
        /// Monobehaviour method called when the Collider 'other' enters the trigger.
        /// Affect Health of the Player if the collider is a beam
        /// Note: When jumping and firing at the same, you'll find that the player's own beam intersects with itself.
        /// One could move the collider further away to prevent this or check if the beam belongs to the player
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if(!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // we should be using tags but for the sake of distribution, let's simply check by name.
            if(!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f;
        }

        /// <summary>
        /// Monobehaviour method called once per frame for every Collider 'other' that is touching the trigger.
        /// We're going to affect health while the beams are touching the player
        /// </summary>
        /// <param name="other">Other</param>
        private void OnTriggerStay(Collider other)
        {
            // We don't do anything if we are not the local player.
            if(!photonView.IsMine)
            {
                return;
            }
            // We are only interested in Beamers
            // We should be using tags but for the sake of distribution, let's simple check by name
            if(!other.name.Contains("Beam"))
            {
                return;
            }
            //we slowly affect health when beam is constantly hitting us, so player has to move to prevent death.
            Health -= 0.1f * Time.deltaTime;



        }

#if UNITY_5_4_OR_NEWER
       
        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }

        void CalledOnLevelWasLoaded(int level)
        {
            //check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if(!Physics.Raycast(transform.position,-Vector3.up,5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }

            GameObject _uiGo = Instantiate(this.playerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

#endif

        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        private void ProcessInputs()
        {
            if(Input.GetButtonDown("Fire1"))
            {
                if(!IsFiring)
                {
                    IsFiring = true;
                }
            }
            if(Input.GetButtonUp("Fire1"))
            {
                if(IsFiring)
                {
                    IsFiring = false;
                }
            }
        }
#endregion

#region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if(stream.IsWriting)
            {
                //We Own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }



#endregion
    }

}

