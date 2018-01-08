using UnityEngine;
using System.Collections;

public class LegRotator : MonoBehaviour {

	//public int targetCount = 3;
	//public Vector3[] targetRot;

	public LegParam Param { get; set; }

	public float loopCount = 2.0f;
	protected float count;

	protected float sleepTime;

	// Use this for initialization
	void Start () {

		//this.rigidbody.maxAngularVelocity = 10;
		//this.rigidbody.mass = Param.mass;

		sleepTime = 1.0f;
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if( sleepTime > 0.0f ) {
			sleepTime -= Time.fixedDeltaTime;
			return;
		}

		count += Time.fixedDeltaTime;
		if( count >= loopCount ) count -= loopCount;

		float sc = (count / loopCount) * Param.targetCount;

		int lid = Mathf.FloorToInt(sc);
		float lr = sc - lid;

		int nid = (lid >= Param.targetCount-1) ? 0 : lid + 1;
		float rr = Mathf.Cos(lr * Mathf.PI) * 0.5f + 0.5f;


		Vector3 tr1 = Param.targetRot[lid];
		Vector3 tr2 = Param.targetRot[nid];
		Vector3 cr = tr1 * rr + tr2 * (1.0f - rr);

		//float rr = Mathf.Cos(r * Mathf.PI) * 0.5f + 0.5f;
		//float crx = rx1 * rr + rx2 * (1.0f - rr);
		//float crz = rz1 * rr + rz2 * (1.0f - rr);

		Quaternion q = Quaternion.Euler(cr);
		this.transform.localRotation = q;

		//this.rigidbody.angularVelocity = cr * Time.fixedDeltaTime * 200;
	}
}
