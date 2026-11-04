using UnityEngine;
using System.Collections;

public class CercaniaCuervo : MonoBehaviour {
	
	void OnTriggerEnter (Collider col) {
		if(col.tag == "lb_bird"){
			col.SendMessage("CrowIsClose");
		}
	}

}
