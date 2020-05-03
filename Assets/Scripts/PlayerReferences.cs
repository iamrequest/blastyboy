using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Stores references to the player's blaster, blaster grapple, etc.
// This is a handy way for enemies to get a reference to these gameobjects/components
// One example, is ragdoll enemy grabbable limbs, which require a reference to the 
//  player's blaster grappler
public class PlayerReferences : MonoBehaviour {
    public Blaster blaster;
    public BlasterGrappler blasterGrappler;
}
