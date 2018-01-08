using UnityEngine;
using System.Collections;

public class Walker : MonoBehaviour {

	public LegRotator[] legs = new LegRotator[8];	// Legs FRU, FRD, FLU, FLD, BRU, BRD, BLU, BLD

	public float upSideDownCount;
	public float backwardCount;

	protected WalkerParam param_;
	public WalkerParam Param { 
		get {return param_;} 
		set {
			for( int i=0; i<8; i++ ) {
				legs[i].Param = value.legParams[i];
			}
		}
	}
	
	// Use this for initialization
	void Start () {
		upSideDownCount = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		//Debug.Log(this.transform.rotation.eulerAngles);

		Vector3 up = new Vector3(0,1,0);
		up = this.transform.rotation * up;

		if( up.y < 0 ) {
			upSideDownCount += Time.fixedDeltaTime;
		}

		Vector3 back = new Vector3(0,0,1);
		back = this.transform.rotation * back;
		if( back.z < 0.5f ) {
			backwardCount += Time.fixedDeltaTime * (0.5f-back.z);
		}

	}
}
