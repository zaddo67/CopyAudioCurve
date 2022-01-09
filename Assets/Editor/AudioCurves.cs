using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;


namespace ZoneOfFun.Utilitis
{

    public class AudioCurves : EditorWindow
    {

        static EditorWindow window;
        static bool m_initialised = false;
        static bool m_GUIUpdated = false;
        static GameObject m_lastGameObject;
        static AudioSource m_SelectedAudioSource;

        static SerializedProperty m_DopplerLevelProp;
        static SerializedProperty m_MinDistanceProp;
        static SerializedProperty m_MaxDistanceProp;
        static SerializedProperty m_Pan2DProp;
        static SerializedProperty m_RolloffModeProp;
        static SerializedProperty m_RolloffCustomCurveProp;
        static SerializedProperty m_panLevelCustomCurveProp;
        static SerializedProperty m_SpreadCustomCurveProp;
        static SerializedProperty m_ReverbZoneMixCustomCurveProp;

        static string m_copiedObjectName = string.Empty;


        [MenuItem("Window/AudioCurves")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(AudioCurves));
        }

        public static void Init()
        {
            window = EditorWindow.GetWindow(typeof(AudioCurves));
            window.wantsMouseMove = true;
            window.Show();


            m_initialised = true;
        }

        void OnEnable()
        {
            titleContent.text = "Audio Curves";
            // manually call the event handler when the window is first loaded so its contents are up-to-date
            OnHierarchyChange();
        }

        public void OnFocus()
        {
            if (window != null)
            {
                window = EditorWindow.GetWindow(typeof(AudioCurves));
                window.wantsMouseMove = true;
            }
        }

        public void OnGUI()
        {

            DisplayGUIHeader();
            if (!string.IsNullOrEmpty(m_SelectedAudioSource.name)) DisplayGUICopy();
            if (!string.IsNullOrEmpty(m_SelectedAudioSource.name) && m_DopplerLevelProp != null) DisplayGUIPaste();

            // Show tooltip
            Vector2 mousePos = Event.current.mousePosition;
            if (!string.IsNullOrEmpty(GUI.tooltip))
                GUI.Box(new Rect(mousePos.x + 20, mousePos.y + 20, 200, 20), GUI.tooltip);


        }

        private void Update()
        {
            CheckSelectedObject();

        }

        private void CheckSelectedObject()
        {
            if (m_lastGameObject != Selection.activeGameObject)
            {
                AudioSource audioSource = null;
                m_lastGameObject = Selection.activeGameObject;
                if (m_lastGameObject != null)
                {
                    audioSource = m_lastGameObject.GetComponent<AudioSource>();
                }
                m_SelectedAudioSource = audioSource;
                Repaint();
            }
        }

        void OnHierarchyChange()
        {
            var all = Resources.FindObjectsOfTypeAll(typeof(GameObject));
            var m_NumberVisible =
                all.Where(obj => (obj.hideFlags & HideFlags.HideInHierarchy) != HideFlags.HideInHierarchy).Count();
        }

        void DisplayGUIHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();


            Color saveColor = GUI.color;
            Color saveBackgroung = GUI.backgroundColor;
            Color saveContent = GUI.contentColor;

            GUI.skin.box.alignment = TextAnchor.MiddleLeft;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;

            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;

            GUILayout.BeginHorizontal();
            GUILayout.Box(new GUIContent("Select an audio source to copy from or to."), GUILayout.Width(520), GUILayout.Height(40));
            GUILayout.EndHorizontal();

            GUI.contentColor = Color.white;
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            GUI.color = saveColor;
            GUI.backgroundColor = saveBackgroung;
            GUI.contentColor = saveContent;

        }

        void DisplayGUICopy()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Copy"))
            {
                if (m_SelectedAudioSource != null) CopyCustomAudioRolloff();
            }
            if (!string.IsNullOrEmpty(m_copiedObjectName))
                GUILayout.Label($"Copied({m_copiedObjectName})");
            GUILayout.EndHorizontal();
        }

        void DisplayGUIPaste()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Paste"))
            {
                if (m_SelectedAudioSource != null && m_RolloffCustomCurveProp != null) PasteCustomAudioRolloff();
            }

            GUILayout.Label(m_SelectedAudioSource.name);

            GUILayout.EndHorizontal();
        }


        private void CopyCustomAudioRolloff()
        {
            if (m_SelectedAudioSource != null)
            {
                var serializedObject = new SerializedObject(m_SelectedAudioSource);

                m_DopplerLevelProp = serializedObject.FindProperty("DopplerLevel");

                m_MinDistanceProp = serializedObject.FindProperty("MinDistance");
                m_MaxDistanceProp = serializedObject.FindProperty("MaxDistance");
                m_Pan2DProp = serializedObject.FindProperty("Pan2D");
                m_RolloffModeProp = serializedObject.FindProperty("rolloffMode");

                m_RolloffCustomCurveProp = serializedObject.FindProperty("rolloffCustomCurve");
                m_panLevelCustomCurveProp = serializedObject.FindProperty("panLevelCustomCurve");
                m_SpreadCustomCurveProp = serializedObject.FindProperty("spreadCustomCurve");
                m_ReverbZoneMixCustomCurveProp = serializedObject.FindProperty("reverbZoneMixCustomCurve");

                AnimationCurve curve = m_RolloffCustomCurveProp.animationCurveValue;

                m_copiedObjectName = m_SelectedAudioSource.name;
            }
        }

        private void PasteCustomAudioRolloff()
        {
            var serializedObject = new SerializedObject(m_SelectedAudioSource);
            serializedObject.FindProperty("DopplerLevel").floatValue = m_DopplerLevelProp.floatValue;

            serializedObject.FindProperty("MinDistance").floatValue = m_MinDistanceProp.floatValue;
            serializedObject.FindProperty("MaxDistance").floatValue = m_MaxDistanceProp.floatValue;
            serializedObject.FindProperty("Pan2D").floatValue = m_Pan2DProp.floatValue;
            var rolloffModeProp = serializedObject.FindProperty("rolloffMode");
            if ((AudioRolloffMode)m_RolloffModeProp.enumValueIndex != (AudioRolloffMode)rolloffModeProp.enumValueIndex)
                serializedObject.FindProperty("rolloffMode").enumValueIndex = m_RolloffModeProp.enumValueIndex;

            serializedObject.FindProperty("rolloffCustomCurve").animationCurveValue = m_RolloffCustomCurveProp.animationCurveValue;
            serializedObject.FindProperty("panLevelCustomCurve").animationCurveValue = m_panLevelCustomCurveProp.animationCurveValue;
            serializedObject.FindProperty("spreadCustomCurve").animationCurveValue = m_SpreadCustomCurveProp.animationCurveValue;
            serializedObject.FindProperty("reverbZoneMixCustomCurve").animationCurveValue = m_ReverbZoneMixCustomCurveProp.animationCurveValue;

            serializedObject.ApplyModifiedProperties();
        }

        /// A logarithmic curve starting at /timeStart/, /valueStart/ and ending at /timeEnd/, /valueEnd/
        private static AnimationCurve Logarithmic(float timeStart, float timeEnd, float logBase)
        {
            float value, slope, s;
            List<Keyframe> keys = new List<Keyframe>();
            // Just plain set the step to 2 always. It can't really be any less,
            // or the curvature will end up being imprecise in certain edge cases.
            float step = 2;
            timeStart = Mathf.Max(timeStart, 0.0001f);
            for (float d = timeStart; d < timeEnd; d *= step)
            {
                // Add key w. sensible tangents
                value = LogarithmicValue(d, timeStart, logBase);
                s = d / 50.0f;
                slope = (LogarithmicValue(d + s, timeStart, logBase) - LogarithmicValue(d - s, timeStart, logBase)) / (s * 2);
                keys.Add(new Keyframe(d, value, slope, slope));
            }

            // last key
            value = LogarithmicValue(timeEnd, timeStart, logBase);
            s = timeEnd / 50.0f;
            slope = (LogarithmicValue(timeEnd + s, timeStart, logBase) - LogarithmicValue(timeEnd - s, timeStart, logBase)) / (s * 2);
            keys.Add(new Keyframe(timeEnd, value, slope, slope));

            return new AnimationCurve(keys.ToArray());
        }

        private static float LogarithmicValue(float distance, float minDistance, float rolloffScale)
        {
            if ((distance > minDistance) && (rolloffScale != 1.0f))
            {
                distance -= minDistance;
                distance *= rolloffScale;
                distance += minDistance;
            }
            if (distance < .000001f)
                distance = .000001f;
            return minDistance / distance;
        }

    }
}