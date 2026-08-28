using TriLibCore.Utils;
using UnityEngine;

namespace TriLibCore.Samples
{
    /// <summary>
    /// This sample loads chained (consecutively) models from different URLs.
    /// </summary>
    public class ChainedURLDownloadSample : MonoBehaviour
    {
        // Original Models: https://github.com/jaanga/3d-models
        /// <summary>
        /// Contains the list of models to be downloaded.
        /// </summary>
        public string[] URLs = new string[]
        {
            "https://ricardoreis.net/trilib/demos/sample/a-330.obj",
            "https://ricardoreis.net/trilib/demos/sample/factory-complex.obj",
            "https://ricardoreis.net/trilib/demos/sample/villa-savoye.zip",
        };

        /// <summary>
        /// Contains the Asset Loader Options to use when loading the models.
        /// You can leave this field empty, so TriLib will create the Asset Loader Options automatically with default values.
        /// </summary>
        public AssetLoaderOptions AssetLoaderOptions;

        /// <summary>
        /// Contains the GameObject that will be the parent to the loaded models.
        /// You can leave this field empty, so GameObjects will be created on the scene root level.
        /// </summary>
        public GameObject WrapperGameObject;

        /// <summary>
        /// Represents the index to the URLs array from the model that is currently downloading.
        /// </summary>
        private int _urlIndex;

        /// <summary>
        /// Assigns the default Asset Loader Options in case no one is assigned, then proceeds to load the first model.
        /// </summary>
        private void Start()
        {
            AssetLoaderOptions = AssetLoaderOptions ?? AssetLoader.CreateDefaultLoaderOptions();
            DownloadNextModel();
        }

        /// <summary>
        /// Downloads the next model in the URL list and advances the model URL index counter.
        /// </summary>
        private void DownloadNextModel()
        {
            if (_urlIndex >= URLs.Length)
            {
                return;
            }
            var url = URLs[_urlIndex++];
            var fileExtension = FileUtils.GetFileExtension(url, false);
            var webRequest = AssetDownloader.CreateWebRequest(url);
            AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, WrapperGameObject, AssetLoaderOptions, null, fileExtension);
            Debug.Log($"Begin downloading:{URLs[_urlIndex - 1]}");
        }

        /// <summary>
        /// This method is called when there was any error while loading the current model,
        /// it then displays the error that was generating while loading the model and advances to the next model loading.
        /// </summary>
        /// <param name="contextualizedError"></param>
        private void OnError(IContextualizedError contextualizedError)
        {
            Debug.LogError($"There was an error downloading:{URLs[_urlIndex - 1]}");
            Debug.LogError(contextualizedError.GetInnerException());
            DownloadNextModel();
        }

        /// <summary>
        /// You can track the model loading progress in this method.
        /// </summary>
        private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
        {

        }

        /// <summary>
        /// This method is called when the model has loaded, but may still have resources to load.
        /// </summary>
        private void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            
        }

        /// <summary>
        /// This method is called when the model is successfully loaded, it then tries to load the next model loading.
        /// You can access the loaded GameObject with the assetLoaderContext.RootGameObject field.
        /// </summary>
        private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            Debug.Log($"Successfully downloaded:{URLs[_urlIndex - 1]}");
            DownloadNextModel();
        }
    }
}