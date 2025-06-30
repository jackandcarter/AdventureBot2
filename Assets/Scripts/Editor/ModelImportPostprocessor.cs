using UnityEditor;

namespace Evolution.Editor
{
    public class ModelImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            ModelImporter importer = (ModelImporter)assetImporter;
            if (assetPath.StartsWith("Assets/Models/Tiles"))
            {
                importer.importNormals = ModelImporterNormals.Calculate;
            }
        }
    }
}
