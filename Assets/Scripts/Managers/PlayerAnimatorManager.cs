using System.Collections;
using UnityEngine;

using Photon.Pun;
namespace GameHolic.PUNTutorial
{
    public class PlayerAnimatorManager : MonoBehaviourPun
    {

        #region Private Fields

        private Animator animator;

        #endregion

        #region Private Serializable Fields

        [SerializeField]
        private float directionDampTime = 0.25f;

        #endregion


        #region MonoBehaviour Callbacks

        // Use for initialization
        void Start()
        {
            animator = GetComponent<Animator>();
            if(!animator)
            {
                Debug.LogError("PlayerAnimatorManager is Missing Animator Component ", this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(photonView.IsMine==false && PhotonNetwork.IsConnected==true)
            {
                return;
            }
            if (!animator)
                return;

            // deal with jump
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // only allow jumping if we are running.
            if (stateInfo.IsName("Base Layer.Run"))
            {
                // when using triggerr parameter
                if (Input.GetButtonDown("Fire2"))
                {
                    animator.SetTrigger("Jump");
                }
            }
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (v < 0)
            {
                v = 0;
            }

            animator.SetFloat("Speed", h * h + v * v);
            animator.SetFloat("Direction", h, directionDampTime, Time.deltaTime);
        }
        #endregion


    }
}