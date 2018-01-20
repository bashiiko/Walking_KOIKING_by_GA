using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GenManager))]
public class GenManagerEditor : Editor {

	public override void OnInspectorGUI() {

		GenManager obj = target as GenManager;

		EditorGUILayout.LabelField("Generations: ", obj.genCount.ToString());
		obj.creatureCount = EditorGUILayout.IntSlider("CreatureCount", obj.creatureCount, 1,200);
		obj.surviveCount = EditorGUILayout.IntSlider("SurvivorCount", obj.surviveCount,0,100);
		obj.mutationCount = EditorGUILayout.IntSlider("MutationCount", obj.mutationCount,0,100);

		obj.genDuration = EditorGUILayout.Slider("GenDuration", obj.genDuration, 1, 60);
		obj.finRotateLimit = EditorGUILayout.Vector3Field("finRotateLimit", obj.finRotateLimit);
		obj.bodyRotateLimit = EditorGUILayout.Vector3Field("bodyRotateLimit", obj.bodyRotateLimit);
		obj.targetID = EditorGUILayout.IntField("targetID", obj.targetID);

		if( GUILayout.Button("Export") ) {

			ExportParams(obj.param);
		}
		if( GUILayout.Button("Import") ) {

			ImportParams(obj);
		}
		if( GUILayout.Button("Reset") ) {
			obj.importPath = "";
			obj.genCount = 0;
		}

	}

	protected void ExportParams(GenParam param) {

		string dateStr = System.DateTime.Now.ToString("yyyyMMddhhmm");
		string defaultXmlFile = string.Format("Gen_{0}_{1}.xml",
		    param.genCount, dateStr);

		string path = EditorUtility.SaveFilePanel(
			"Save block parameters",
			"Export",
			defaultXmlFile,
			"xml");

		if( path.Length != 0 ) {
			System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer( typeof(GenParam) );
			System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Create);
			xs.Serialize(fs, param);
			fs.Close();
		}
	}

	protected void ImportParams(GenManager manager) {
		string path = EditorUtility.OpenFilePanel(
			"Save block parameters",
			"Export",
			"xml");

		if(path.Length != 0) {

			System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer( typeof(GenParam) );
			System.IO.FileStream fs = new System.IO.FileStream(path, System.IO.FileMode.Open);
			GenParam param = (GenParam)xs.Deserialize(fs);

			manager.param = param;
			manager.genCount = param.genCount;
			manager.importPath = path;
		}
	}

}
