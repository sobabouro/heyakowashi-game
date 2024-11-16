using UnityEngine;

public class TextClash : MonoBehaviour {
	// このスクリプトがアタッチされているオブジェクトが他のオブジェクトと衝突したときに呼び出される
	[SerializeField]
	private GameObject mainObject;
	[SerializeField]
	private GameObject otherObject;
	private void OnCollisionEnter(Collision collision) {

		// 両方のオブジェクトのアクティブ状態を反転
		mainObject.SetActive(!gameObject.activeSelf); // 自分自身のアクティブ状態を反転
		otherObject.SetActive(!otherObject.activeSelf);
	}
}
