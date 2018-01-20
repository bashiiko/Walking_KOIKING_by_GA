using UnityEngine;
using System.Collections;

public class finRotator : MonoBehaviour {

	public finParam Param { get; set; }

	protected float[] Limit;
	protected int[] dir = new int[3]; //　ひれの進む方向（正か負か）
    protected float[] rot = new float[3]{ 0, 0, 0 }; //　現在のrotation
	protected int[] order = new int[3]{ 0, 0, 0 }; //　現在の動作が何番目か（0~3の状態を繰り返す。x,y,z方向それぞれに対して値を持つ
    protected int[] trip = new int[3]{ 0, 0, 0 }; //　動作が往復していれば1,一回目なら0


	// Use this for initialization
	void Start () {
    Limit = new float[]{ Param.RotRange[order[0]].x, Param.RotRange[order[1]].y, Param.RotRange[order[2]].z };

    for(int i=0; i<3; i++){
   		if( Limit[i] < 0) dir[i] = -1;
		  else dir[i] = 1;
    }

	}

	// Update is called once per frame
	void FixedUpdate () {

    /*
		for(int i=0; i<3; i++){
      if( System.Math.Abs(rot[i]) > System.Math.Abs(Limit[i])){
				dir[i] *= -1; //　進行方向の逆転
			}
			rot[i] += dir[i];
		}
		*/
		for(int i=0; i<3; i++){
          if( System.Math.Abs(rot[i]) > System.Math.Abs(Limit[i])){
				if( trip[i] == 0 ){
					dir[i] *= -1;
					trip[i] = 1;
				}else{
				  //　動作の状態を次へ進める。状態3まできたら状態0に戻る
				  if( order[i] < 3 ) order[i]++;
				  else order[i] = 0;

			    Limit = new float[]{ Param.RotRange[order[0]].x, Param.RotRange[order[1]].y, Param.RotRange[order[2]].z };

			  	//　現在位置と目的位置の位置関係から進行方向を決定
          if( (Limit[i] - rot[i]) < 0 ) dir[i] = -1;
				  else dir[i] = 1;
					trip[i] = 0;
				}
			}
			rot[i] += dir[i];
	     	}


    //　現在の回転値の取得（Quaternion→Vector）
    Vector3 pos = this.transform.rotation.eulerAngles;
		pos.x += 3*dir[0];
		pos.y += 3*dir[1];
		pos.z += 3*dir[2];
		//　Vector→Quatanion
		Quaternion q = Quaternion.Euler(pos);
		this.transform.rotation = q;

	}
}
