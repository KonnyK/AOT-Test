using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookTrigger : MonoBehaviour
{
    private Hook_Script Parent = null;
    public void setParent(Hook_Script newParent){ Parent = newParent; }
    
    private void Update()
    {
        if (Parent == null) return;
        if ((transform.position-Parent.transform.position).magnitude > Parent.MaxRopeLength) Parent.resetHook();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Hookable")
        {
            foreach (Rigidbody RB in GetComponentsInChildren<Rigidbody>()) { RB.useGravity = false; RB.constraints = RigidbodyConstraints.FreezeAll; }
            Parent.OnHookHit();
        }
    }
}
