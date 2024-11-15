using UnityEngine;

public class TextClash : MonoBehaviour {
	[SerializeField, Tooltip("再生するアニメーションを持つ Animator")]
	private Animator animator;

	[SerializeField, Tooltip("アニメーションのトリガー名")]
	private string animationTriggerName = "PlayAnimation";

	private void OnCollisionEnter(Collision collision) {
		// 衝突時にアニメーションを再生
		if (animator != null) {
			animator.SetTrigger(animationTriggerName);
		}
	}
}
