using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crash : MonoBehaviour
{
    // �j���̃I�u�W�F�N�g���Ăяo�����̃t���O
    [SerializeField]
    private bool _canCallBrokenObject;
    // �I�u�W�F�N�g�̔j���ɌĂяo�����I�u�W�F�N�g
    [SerializeField]
    private GameObject _brokenObjectPrefab;
    // �I�u�W�F�N�g�j�󎞂ɌĂяo���C�x���g�o�^
    public UnityEvent onBreakEvent;
    // �j���̃I�u�W�F�N�g���Ăяo���ۂɉ�����O�����̗�
    [SerializeField]
    private float _addImpulse = 1;

    // Start is called before the first frame update
    void Start()
    {
        onBreakEvent.AddListener(DebugMessage);
    }

    // �󑮐��ɂ��I�u�W�F�N�g�̔j�󏈗����Ăяo�����ۂɌĂяo��
    public void CallCrash()
    {
        // ���g�̓����蔻�������������
        this.gameObject.GetComponent<Collider>().enabled = false;


        // �j�󎞂ɌĂяo�����C�x���g���Ăяo��
        onBreakEvent?.Invoke();

        // �t���O�ɂ���Ĕj���̃I�u�W�F�N�g���Ăяo�����肷��
        if (_canCallBrokenObject)
        {
            CallBrokenObject();
        }

        // �I�u�W�F�N�g��j�󂷂�
        Debug.Log("CrashDestroy! : " + this.gameObject);
        Destroy(this.gameObject);
    }

    // �j���ɃI�u�W�F�N�g�����ۂɌĂяo��
    private void CallBrokenObject()
    {
        Debug.Log("CallBrokenObject!");
        // �j���ɌĂяo���I�u�W�F�N�g�𐶐����āA�O���Ɍ����Ă�����x�̗�(_addForce)�����ăI�u�W�F�N�g�𓮂���
        Transform parentTransform = _brokenObjectPrefab.transform;

        Debug.Log("ParentTagCheck : " + _brokenObjectPrefab.tag);

        if (_brokenObjectPrefab.CompareTag("BreakableObject"))
        {
            GameObject createObject = Instantiate(_brokenObjectPrefab, this.gameObject.transform.position + parentTransform.localPosition, this.gameObject.transform.rotation * parentTransform.localRotation);

            Rigidbody rigidbody = createObject.GetComponent<Rigidbody>();

            rigidbody.AddForce(_addImpulse * Vector3.Normalize(parentTransform.localPosition), ForceMode.Impulse);
        }
        else
        {
            // �q�I�u�W�F�N�g��S�Ď擾����(����̊K�w��ΏۂƂ���BreakableObject�̃^�O���t�������m���Ăяo��)
            foreach (Transform child in parentTransform)
            {
                Debug.Log("ChildTagCheck : " + child.tag);
                if (child.CompareTag("BreakableObject") == false) continue;
                GameObject createObject = Instantiate(child.gameObject, this.gameObject.transform.position + child.localPosition, this.gameObject.transform.rotation * child.localRotation);

                Rigidbody rigidbody = createObject.GetComponent<Rigidbody>();

                rigidbody.AddForce(_addImpulse * Vector3.Normalize(child.localPosition), ForceMode.Impulse);
            }
        }
    }

    private void DebugMessage()
    {
        Debug.Log("onBreakEvent.Invoke!");
    }

}
