using UnityEngine;

namespace LookingGlass {
    /// <summary>
    /// Prevents this field from being changed in Unity's inspector.<br />
    /// Instead of being editable, the field will be drawn with disabled editor GUI.
    /// </summary>
    public class ReadOnlyProperty : PropertyAttribute {
        public ReadOnlyProperty() { }

        public virtual bool IsReadOnly() => true;
    }
}
