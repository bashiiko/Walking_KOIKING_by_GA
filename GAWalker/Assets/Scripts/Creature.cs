using UnityEngine;
using System.Collections;

public class Creature : MonoBehaviour {

	public finRotator[] fins = new finRotator[6];
	//  fins Back,FinL,FinR,Body（背びれ、胸びれ*2、体*3）

  //  set,getはプロパティ構文内でのみキーワード扱いされる
	//  valueはset,get内でのみキーワード扱いされる
	//　それ以外の文脈では変数名として用いることができる
	protected CreatureParam param_;
	public CreatureParam Param {
		get {return param_;}
		set {
			for( int i=0; i<6; i++ ) {
				fins[i].Param = value.finParams[i];
			}
		}
	}
}
