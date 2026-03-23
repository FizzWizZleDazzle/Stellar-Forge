#if !KSA_REAL
using System.Numerics;

namespace Brutal.ImGuiAPI
{
    public struct ImString
    {
        public string Value;

        public ImString(string value) => Value = value;

        public static implicit operator ImString(string s) => new(s);
    }

    public enum ImGuiWindowFlags
    {
        None = 0
    }

    public static class ImGui
    {
        public static bool Begin(ImString title, ImGuiWindowFlags flags) => false;
        public static void End() { }
        public static void Text(ImString text) { }
        public static void TextColored(Vector4 color, ImString text) { }
        public static bool Button(ImString label) => false;
        public static bool SliderInt(ImString label, ref int value, int min, int max) => false;
        public static bool SliderFloat(ImString label, ref float value, float min, float max) => false;
        public static bool InputText(ImString label, byte[] buffer, uint bufferSize) => false;
        public static void ProgressBar(float fraction) { }
        public static void Separator() { }
        public static void BeginDisabled() { }
        public static void EndDisabled() { }
        public static bool Checkbox(ImString label, ref bool value) => false;
        public static void SameLine() { }
        public static bool TreeNode(ImString label) => false;
        public static void TreePop() { }
        public static void Columns(int count) { }
        public static void NextColumn() { }
        public static void SetColumnWidth(int index, float width) { }
        public static bool BeginCombo(ImString label, ImString previewValue) => false;
        public static void EndCombo() { }
        public static bool Selectable(ImString label, bool selected) => false;
    }
}
#endif
