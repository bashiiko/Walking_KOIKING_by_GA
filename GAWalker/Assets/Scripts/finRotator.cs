using UnityEngine;
using System.Collections;

public class finRotator : MonoBehaviour {

	public finParam Param { get; set; }

	protected float[] Limit;
	protected int[] dir = new int[3]; //　ひれの進む方向（正か負か）
  protected float[] rot = new float[3]{ 0, 0, 0 }; //　現在のrotation

	// Use this for initialization
	void Start () {
    Limit = new float[]{ Param.RotRange.x, Param.RotRange.y, Param.RotRange.z };

    for(int i=0; i<3; i++){
   		if( Limit[i] < 0) dir[i] = -1;
		  else dir[i] = 1;
    }

	}

	// Update is called once per frame
	void FixedUpdate () {

		for(int i=0; i<3; i++){
      if( System.Math.Abs(rot[i]) > System.Math.Abs(Limit[i])){
				dir[i] *= -1; //　進行方向の逆転
			}
			rot[i] += dir[i];
		}

    //　現在の回転値の取得（Quaternion→Vector）
    Vector3 pos = this.transform.rotation.eulerAngles;
		pos.x += dir[0];
		pos.y += dir[1];
		pos.z += dir[2];
		//　Vector→Quatanion
		Quaternion q = Quaternion.Euler(pos);
		this.transform.rotation = q;

	}
}
