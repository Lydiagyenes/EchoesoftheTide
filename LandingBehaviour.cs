using UnityEngine;

public class LandingBehaviour : StateMachineBehaviour
{
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerMovement playerMovement = animator.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMove = false; // Csak a mozgást tiltjuk le
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        PlayerMovement playerMovement = animator.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMove = true; // Visszaengedélyezzük a mozgást
        }
    }
}
