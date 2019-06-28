using System.Collections;
using UnityEngine;


namespace GameHolic.PUNTutorial
{
    public class PlayerAnimatorManager : MonoBehaviour
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
            if (!animator)
                return;

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