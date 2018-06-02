#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.Build;

/// <summary>
/// handle build
/// </summary>
class MyCustomBuildProcessor : IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        // Do the preprocessing here

    }
}

#endif