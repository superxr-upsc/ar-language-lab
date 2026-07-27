using System;
using System.Collections.Generic;
using CodeBase.Gameplay.ARObjects;
using UnityEditor;
using UnityEngine.XR.ARSubsystems;

namespace CodeBase.ARFoundation.ImageTracking.Editor
{
    [CustomEditor(typeof(TrackableImagesConfig))]
    public class TrackableImagesConfigCustomEditor : UnityEditor.Editor
    {
        private List<XRReferenceImage> referenceImages = new();
        private bool isExpanded = true;

        public override void OnInspectorGUI()
        {
            var behaviour = serializedObject.targetObject as TrackableImagesConfig;

            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            var libraryProperty = serializedObject.FindProperty("ImageLibrary");
            EditorGUILayout.PropertyField(libraryProperty);
            
            var library = libraryProperty.objectReferenceValue as XRReferenceImageLibrary;
            if (library == null)
            {
                referenceImages.Clear();
                behaviour.ClearTrackableImagesCache();
                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            if (HasLibraryChanged(library)) 
                UpdateTrackableImagesCache(library, behaviour);

            InitializeReferenceImagesList(library);

            isExpanded = EditorGUILayout.Foldout(isExpanded, "Prefab List");
            if (isExpanded) ShowPrefabList(library, behaviour);

            serializedObject.ApplyModifiedProperties();
        }

        private bool HasLibraryChanged(XRReferenceImageLibrary library)
        {
            if (library == null)
                return referenceImages.Count == 0;

            if (referenceImages.Count != library.count)
                return true;

            for (int i = 0; i < library.count; i++)
            {
                if (referenceImages[i] != library[i])
                    return true;
            }

            return false;
        }

        private static void UpdateTrackableImagesCache(XRReferenceImageLibrary library, TrackableImagesConfig behaviour)
        {
            var tempDictionary = new Dictionary<Guid, ARObjectBase>();
                
            foreach (var referenceImage in library) 
                tempDictionary.Add(referenceImage.guid, behaviour.GetPrefabForReferenceImage(referenceImage));

            behaviour.SetTrackableImagesCache(tempDictionary);
        }

        private void InitializeReferenceImagesList(XRReferenceImageLibrary library)
        {
            referenceImages.Clear();
            foreach (var referenceImage in library) 
                referenceImages.Add(referenceImage);
        }

        private void ShowPrefabList(XRReferenceImageLibrary library, TrackableImagesConfig behaviour)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();

                var tempDictionary = new Dictionary<Guid, ARObjectBase>();
                foreach (var image in library)
                {
                    var prefab = (ARObjectBase)EditorGUILayout.ObjectField(image.name, behaviour.GetPrefabForReferenceImage(image), typeof(ARObjectBase), false);
                    tempDictionary.Add(image.guid, prefab);
                }

                if (!EditorGUI.EndChangeCheck())
                    return;
                
                Undo.RecordObject(target, "Update Prefab");
                behaviour.SetTrackableImagesCache(tempDictionary);
                EditorUtility.SetDirty(target);
            }
        }
    }
}