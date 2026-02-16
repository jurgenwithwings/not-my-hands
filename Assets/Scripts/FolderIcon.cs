using UnityEngine;

#if UNITY_EDITOR
namespace UnityEditorVisualExtentions {
    public class FolderIcon : MonoBehaviour {
        [Button(nameof(Null), "Does nothing, just for the icon in the hierarchy", ButtonAttribute.ButtonDisplay.Overwrite, 0f, 0f, 0f)]
        [SerializeField]
        private bool i;

        private void Null() { }
    }
}
#endif
