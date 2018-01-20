//-------------------------------------------------------
//　知的情報処理の課題提出用GAアルゴリズム
//　コイキングがより高く飛び跳ねるプログラムを目指す
//　参考コード等：http://developer.wonderpla.net/entry/blog/engineer/GeneticAlgorithm/
//　　　　　　　：http://www.nicovideo.jp/watch/sm16597051（動作を繰り返す点を参考）
//       　　　：http://www.sist.ac.jp/~kanakubo/research/evolutionary_computing/ga_operators.html
//　　　　　　　　（選択・交叉に関する研究内容を参考）
//　役割分担：秋元【select() 320行～】
//           池田【Mutation() 430行～】
//           小林【その他、GenManager.cs以外のスクリプト】
//-------------------------------------------------------


﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GenManager : MonoBehaviour {

	//-----------------------------------------------------
	//【定数定義】
	//-----------------------------------------------------
	public GenParam param;

	public GameObject creature;
	public int creatureCount   = 100;　　//　個体の総数
	public int surviveCount  = 0;　 　  //　親世代から子世代へ受け継がれる個体の数　
	public int mutationCount = 1;　　 　//　突然変異する個体の数

	protected Creature[] currentCreatures;

	public float genDuration = 20;      //　1世代あたりの時間<s>
	protected float genDurationLeft;    //　現在の世代の残り時間

	public string namePrefix;
	public string importPath;

	public int genCount;　　　　　 　　　//　世代数

	public Vector3 finRotateLimit = new Vector3(40,40,40);　　　//　ひれの回転の制限（度）
  public Vector3 bodyRotateLimit = new Vector3(10,10,10);　　　//　体の回転の制限（度）
  //public float[] max_position_y;
  //public float[] max_position_z;

	protected float[] bestScores = new float[100];
	protected int[] bestScoreIds = new int[100];

	protected float ave;　　　　　　　　  // スコアの平均値
	protected float sum;                //　スコアの合計値
	//protected float[] score_calc_ver2 = new float[100];

	public int targetID = -1;           //　カメラの中心となる個体

	//-----------------------------------------------------
	//【関数定義】初期化（オブジェクト起動時に実行される）（担当：小林）
	//-----------------------------------------------------
	void Start () {

		if( importPath.Length > 0 ) {
			// 遺伝データを読み込む
			System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer( typeof(GenParam) );
			System.IO.FileStream fs = new System.IO.FileStream(importPath, System.IO.FileMode.Open);
			GenParam p = (GenParam)xs.Deserialize(fs);

			this.param = p;
			this.genCount = p.genCount;
		}
		else {
			param.creatureCount = this.creatureCount;
			param.surviveCount = this.surviveCount;
			param.mutationCount = this.mutationCount;

			initParam();
		}

		//　新しく個体をcreaturecount個生成
		currentCreatures = new Creature[param.creatureCount];

		prepareCreatures();
		genDurationLeft = genDuration;

	  //　カメラの初期位置（右が進行方向）
 　 Camera.main.transform.position = new Vector3(100, 10, 0);
    Camera.main.transform.LookAt(new Vector3(-100, 10, 0));

	}


	//-----------------------------------------------------
	//【関数定義】パラメータの初期化（担当：小林）
	//-----------------------------------------------------
	void initParam() {

		param.genCount = 1;
		param.creatureParams = new CreatureParam[param.creatureCount];
    //max_position_y = new float[param.creatureCount];

　　// 生物の各個体にパラメータを設定
		for( int i=0; i<param.creatureCount; ++i ) {
			//max_position_y[i] = 0;
			param.creatureParams[i].finParams = new finParam[6];

　　　 //　関節は6個
			for( int j=0; j<6; ++j ) {

				finParam lp = param.creatureParams[i].finParams[j];　　
				lp.RotRange = new Vector3[4];
				if( j<3 ){
					// 4動作分の値を保持する
					  for(int k=0; k<4; k++){
				      lp.RotRange[k] = new Vector3(
					      	Random.Range(-bodyRotateLimit.x, bodyRotateLimit.x),
						      Random.Range(-bodyRotateLimit.y, bodyRotateLimit.y),
						      Random.Range(-bodyRotateLimit.z, bodyRotateLimit.z));
						}
					}else{
						for(int k=0; k<4; k++){
						  lp.RotRange[k] = new Vector3(
							  	Random.Range(-finRotateLimit.x, finRotateLimit.x),
								  Random.Range(-finRotateLimit.y, finRotateLimit.y),
								  Random.Range(-finRotateLimit.z, finRotateLimit.z));
						 }
					}

				param.creatureParams[i].finParams[j] = lp;
			}
		}

	}


	//-----------------------------------------------------
	//【関数定義】次の世代の準備（毎世代の初めに呼ばれる）（担当：小林）
	//-----------------------------------------------------
	void prepareCreatures() {
		for( int i=0; i<param.creatureCount; ++i ) {
			// プレハブを取得
      GameObject creature = (GameObject)Resources.Load ("Prefabs/Creature");
			//　プレハブの生成
			//　プレハブ名、position,Quaternion.identity
			GameObject obj = Instantiate(creature, new Vector3((i - param.creatureCount/2) * 15, 5.0f, 0),Quaternion.identity);
			Creature ws = obj.GetComponent<Creature>();
			ws.Param = param.creatureParams[i];
			currentCreatures[i] = ws;
		  //max_position_z[i] = 0;
		  //max_position_y[i] = 0;
		}
	}


	//-----------------------------------------------------
	//【関数定義】個体の削除（担当：小林）
	//-----------------------------------------------------
	void deleteCreatures() {
		for( int i=0; i<param.creatureCount; ++i ) {
			Destroy(currentCreatures[i].gameObject);
			currentCreatures[i] = null;
		}
	}


	//-----------------------------------------------------
	//【関数定義】更新（残り時間の計算、外部ファイルへの記録、カメラの移動）（担当：小林）
	//-----------------------------------------------------
	// 0.02sに一回呼び出される
	void FixedUpdate () {

		genDurationLeft -= Time.fixedDeltaTime;
		calcscore();

    /*
		//　各個体の高さの最高値を記録
	  for( int i=0; i<param.creatureCount; ++i ) {
			Creature creature = currentCreatures[i];
			if( max_position_y[i] < creature.transform.position.y )
   		  max_position_y[i] = creature.transform.position.y;
		}
		*/

    //　カメラの移動
		if( targetID < 100 && 0 <= targetID){
		  Vector3 tp = currentCreatures[targetID].transform.position;
		  tp.y = 0;
   	  Camera.main.transform.position = new Vector3(tp.x + 10, tp.y + 10, tp.z);
		  Camera.main.transform.LookAt(tp);
    }else{
	 　 Camera.main.transform.position = new Vector3(100, 10, 0);
	    Camera.main.transform.LookAt(new Vector3(-100, 10, 0));
		}

		//　残り時間が0になった場合の処理
		if( genDurationLeft < 0 ) {
			calcNextGenParams();
			deleteCreatures();
			prepareCreatures();
			genDurationLeft = genDuration;
			param.genCount++;

			// 20世代毎にデータを保存
			if( param.genCount % 20 == 0 ) {
				string dateStr = System.DateTime.Now.ToString("yyyyMMddhhmm");
				string path = string.Format("Export/Gen_{0}_{1}.xml", param.genCount, dateStr);

				System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer( typeof(GenParam) );
				System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
				xs.Serialize(fs, param);
				fs.Close();
			}

			//　スコアの平均値と最大値をCSVファイルに記録
		　/*
			try {
			  using (var sw = new System.IO.StreamWriter(@"Export/result7.csv", true)){
				  sw.WriteLine("{0},{1}", bestScores[0], ave);
	 		  }
	    }
			catch (System.Exception e) {
			 // ファイルを開くのに失敗したときエラーメッセージを表示
			 System.Console.WriteLine(e.Message);
	    }
			*/

		}
	}

	//-----------------------------------------------------
	//【関数定義】 文字表示（世代数など）（担当：小林）
	//-----------------------------------------------------
	void OnGUI() {
		string str = "";
		str += string.Format("Generation: {0}\n", param.genCount);
		str += string.Format("Time: {0}\n", genDurationLeft);
		str += string.Format("\n", genDurationLeft);
		str += string.Format("Best score\n");
		for(int i=0; i<10; ++i ) {
			str += string.Format("  {0:D2}: {1:F2}\n", bestScoreIds[i], bestScores[i]);
		}

		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.black;
		GUI.Label(new Rect(10, 10, 100, 40), str, style);
	}

	//-----------------------------------------------------
  //【関数定義】ベストスコアの計算（担当：小林）
	//-----------------------------------------------------
  void calcscore(){
	  //　辞書型のリストを定義
	  //　creatureCount<int>, score<float>を格納する

		Dictionary<int, float> scoreList = new Dictionary<int, float>();

		sum = 0;

		//-----------------------------------------------------
		//　【１】現在地のz軸方向の値で評価（大きいほど良い）
		//-----------------------------------------------------
	  for( int i=0; i<param.creatureCount; ++i ) {
		  Creature creature = currentCreatures[i];
			// スコア（進んだ距離）を計算
		  float score = creature.transform.position.z;
			sum += score;
		  // スコア（飛んだ高さ）を計算
		  //float score = max_position_y[i] ;
		  scoreList.Add(i, score);
		}

		ave = sum / param.creatureCount; // 今世代のスコアの平均値

		int sc = 0;
		// OrderByDescending : 降順にソート(valueをソート時のキーとする)
		foreach(KeyValuePair<int, float> pair in scoreList.OrderByDescending(p => p.Value) ) {

			//　優秀な個体を10個記録する
			if( sc < 10) {
				bestScores[sc] = pair.Value;　//　進んだ距離
				bestScoreIds[sc] = pair.Key;　//　個体番号
			}
			sc++;

		}


		//-----------------------------------------------------
		//　【２】目的地との差で評価（小さいほど良い）
		//-----------------------------------------------------
		/*
		for( int i=0; i<param.creatureCount; ++i ) {
			Creature creature = currentCreatures[i];
			// スコア（目的地:150mとの差）を計算
      float x_pos = creature.transform.position.x;
      float z_pos = creature.transform.position.z;
      float x_first = (i - param.creatureCount/2) * 15;

			float score = (float)System.Math.Sqrt( System.Math.Pow( ( x_pos-x_first ),2 ) + System.Math.Pow( ( z_pos-150 ),2 ) );

			sum += score;
			score_calc_ver2[i] = score;
			scoreList.Add(i, score);
		}

		ave = sum / param.creatureCount; // 今世代のスコアの平均値

		int sc = 0;
		// OrderByDescending : 昇順にソート(valueをソート時のキーとする)
		foreach(KeyValuePair<int, float> pair in scoreList.OrderBy(p => p.Value) ) {

			//　優秀な個体を10個記録する
			if( sc < param.creatureCount ) {
				bestScores[sc] = pair.Value;　//　目的地との差
				bestScoreIds[sc] = pair.Key;　//　個体番号
			}
			sc++;

		}
		*/

	}


　//-----------------------------------------------------
	//【関数定義】選択（担当：秋元）
　//-----------------------------------------------------
  protected CreatureParam[] Select(){

		CreatureParam[] surviver = new CreatureParam[param.surviveCount];
		calcscore();

		//　エリート保存戦略
		for( int i = 0; i<param.surviveCount; i++){
			int sc = bestScoreIds[i];
 		  surviver[i]= param.creatureParams[sc];
		}

		return surviver;
	}


	//-----------------------------------------------------
	//【関数定義】ルーレット選択（担当：小林）
	//-----------------------------------------------------
	protected int Roulette()
	{
		int i;

    //　評価方法【２】の場合のみ
		/*
		sum = 0;
		for( i=0; i<param.creatureCount; ++i ){
			sum += 1/score_calc_ver2[i];
		}
		*/


		//　0~sum（適合度の合計）の間でランダムな数を選択
		float rand = Random.Range(0,sum);
		float fitness = 0;

		//　個体を一つずつ取り出し、適合度（スコア）がrandを越えたら終了
		for( i=0; i<param.creatureCount; ++i ){
			Creature creature = currentCreatures[i];
			fitness += creature.transform.position.z;
			//fitness += 1/score_calc_ver2[i];
			if( fitness > rand ) break;
		}

		return i;
	}


	//-----------------------------------------------------
	//【関数定義】交叉（担当：小林）
	//-----------------------------------------------------
  protected CreatureParam[] Cross(CreatureParam[] surviver){

		　//-----------------------------------------------------
　　　//　【１】親をエリート世代から選択する場合
　　　//-----------------------------------------------------
      /*
			List<int> indices = new List<int>();
			for( int j=0; j<param.surviveCount; ++j ) {
				indices.Add(j);
			}

		　//　今世代の優秀な個体のうち、2体をランダムに選ぶ　＝　親
			int i1 = indices[Random.Range(0,param.surviveCount)];
			indices.Remove(i1);
			int i2 = indices[Random.Range(0,param.surviveCount-1)];

			CreatureParam[] cp = new CreatureParam[]{ surviver[i1], surviver[i2] };
      */

			//-----------------------------------------------------
		  //　【２】親をルーレット方式で選択する場合
		  //-----------------------------------------------------
      //　親を2体選択
			int parent1 = Roulette();
			int parent2 = Roulette();

			CreatureParam[] cp　= new CreatureParam[]{ param.creatureParams[parent1], param.creatureParams[parent2] };


			//-----------------------------------------------------
 		  //　【１】【２】共通
 		  //-----------------------------------------------------
			CreatureParam[] np = new CreatureParam[2];

			np[0].finParams = new finParam[6];
      np[1].finParams = new finParam[6];

			//　6つの関節に関してそれぞれ、かつ4つの動作についてそれぞれ上記で選んだ親のどちらかのパラメータをコピーする
			//　現在は一様交叉→進化が速い一方、比較的優秀な個体が破壊される危険も
			//　進化の前半にとどめておくべきかも
			for( int j=0; j<6; ++j ) {
				np[0].finParams[j].RotRange = new Vector3[4];
				np[1].finParams[j].RotRange = new Vector3[4];
				for( int k=0; k<4; k++){
					int mask = Random.Range(0,2);
				  np[0].finParams[j].RotRange[k] = cp[ mask ].finParams[j].RotRange[k];

					if( mask == 0 ) mask = 1;
					else mask = 0;
				  np[1].finParams[j].RotRange[k] = cp[ mask ].finParams[j].RotRange[k];
				}
			}

		  return np;
	}


  //-----------------------------------------------------
	//【関数定義】突然変異（担当：池田）
	//-----------------------------------------------------
　protected CreatureParam Mutation(CreatureParam np){
	  //　関節のパラメータを1~4回ランダムで変更する
	  //　（同じ関節が複数回変更されることもある）
	  int mpc = Random.Range(1,4);
	  for( int j=0; j<mpc; ++j ) {

　　//　パラメータを突然変異させる関節をランダムに選ぶ
		int lr = Random.Range(0,6);
		finParam lp = np.finParams[lr];
		lp.RotRange = new Vector3[4];
		if( lr<3 ){
			  for(int k=0; k<4; k++){
				  lp.RotRange[k] = new Vector3(
						  Random.Range(-bodyRotateLimit.x, bodyRotateLimit.x),
						  Random.Range(-bodyRotateLimit.y, bodyRotateLimit.y),
						  Random.Range(-bodyRotateLimit.z, bodyRotateLimit.z));
				}
			}else{
				for(int k=0; k<4; k++){
				  lp.RotRange[k] = new Vector3(
					  	Random.Range(-finRotateLimit.x, finRotateLimit.x),
						  Random.Range(-finRotateLimit.y, finRotateLimit.y),
						  Random.Range(-finRotateLimit.z, finRotateLimit.z));
				}
			}

    np.finParams[lr] = lp;
	  }

	  return np;
  }


	//-----------------------------------------------------
	//【関数定義】次の世代の個体の設定（担当：小林）
	//-----------------------------------------------------
	protected void calcNextGenParams() {
　　
		//-----------------------------------------------------
	　//　選択
		//-----------------------------------------------------
    CreatureParam[] surviver = Select();

	  //　今世代の優秀な個体を、次の世代にコピーする
		for( int i=0; i<param.surviveCount; ++i ) {
			param.creatureParams[i] = surviver[i];
		}

		//　次の世代に新たに生成する個体の数を計算
		int newCount = param.creatureCount - param.surviveCount;

    int mutation = 0;

		//-----------------------------------------------------
	　//　交叉
		//-----------------------------------------------------
		for( int i=0; i<newCount; i+=2 ) {
			CreatureParam[] np = Cross(surviver);

			//-----------------------------------------------------
		　//　突然変異
			//-----------------------------------------------------
			if( mutation < param.mutationCount ) {
				np[0] = Mutation(np[0]);
				np[1] = Mutation(np[1]);
				mutation+=2;
      }

			//　交叉、突然変異した個体を次の世代に登録
		  param.creatureParams[i + param.surviveCount] = np[0];
			if( i+1 < newCount)
		  param.creatureParams[i + 1 + param.surviveCount] = np[1];

		}
	}
}
