using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using System.Text;

namespace Icon.Generator
{
    [ExecuteInEditMode]
    public class IconGenerator : MonoBehaviour
    {
        #region SERIALIZE_FIELDS
        [Header("Components")]
        [SerializeField] private RenderTexture rendererTexture = null;
        [SerializeField] private Camera bakeCam = null;
        [SerializeField] private MeshRenderer meshRenderer = null;
        #endregion

        #region PUBLIC_METHODS
        public void Generate()
        {
            string path = SaveLocal();
            StringBuilder st = new StringBuilder();
            st.Append(path);
            st.Append(meshRenderer.gameObject.name);

            bakeCam.targetTexture = rendererTexture;
            RenderTexture currentTexture = RenderTexture.active;
            bakeCam.targetTexture.Release();
            RenderTexture.active = bakeCam.targetTexture;
            bakeCam.Render();

            Texture2D imgPng = new Texture2D(bakeCam.targetTexture.width, bakeCam.targetTexture.height, TextureFormat.ARGB32, false);
            imgPng.ReadPixels(new Rect(0, 0, bakeCam.targetTexture.width, bakeCam.targetTexture.height), 0, 0);
            imgPng.Apply();

            RenderTexture.active = currentTexture;
            byte[] bytesPng = imgPng.EncodeToPNG();
            System.IO.File.WriteAllBytes(st.ToString() + ".png", bytesPng);

            Debug.Log(meshRenderer.gameObject.name + " generated!");
        }

        public void CenterObjectInCamera()
        {
            Vector3 objectCenter = meshRenderer.bounds.center;
            Vector3 cameraCenterWorld = bakeCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, bakeCam.nearClipPlane + 1));
            Vector3 offset = cameraCenterWorld - objectCenter;
            meshRenderer.transform.position += offset;
        }
        #endregion

        #region PRIVATE_METHODS
        private string SaveLocal()
        {
            string saveLocal = Application.streamingAssetsPath + "/Icons/";

            if (!Directory.Exists(saveLocal))
            {
                Directory.CreateDirectory(saveLocal);
            }

            return saveLocal;
        }
        #endregion
    }

    #region CUSTOM_EDITOR
    [CustomEditor(typeof(IconGenerator))]
    public class IconGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector(); 

            IconGenerator script = (IconGenerator)target;
           
            GUILayout.Space(20);

            GUIStyle textStyle = new GUIStyle(GUI.skin.label);
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.fontSize = 20;
            GUILayout.Label("Generate", textStyle);

            if (GUILayout.Button("Centralize Item"))
            {
                script.CenterObjectInCamera();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Generate Icon"))
            {
                script.Generate();
            }
        }
    }
    #endregion
}
