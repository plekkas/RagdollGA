using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundHitCheck : MonoBehaviour {

    public RagdollController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ground")
        {
            controller.GroundHit();
        }
    }
}
