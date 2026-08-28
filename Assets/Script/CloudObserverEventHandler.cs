/*============================================================================== 
Copyright (c) 2021 PTC Inc. All Rights Reserved.

Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
==============================================================================*/
using UnityEngine;
using Vuforia;

public class CloudObserverEventHandler : DefaultObserverEventHandler
{
    CloudRecoBehaviour mCloudRecoBehaviour;
    CloudContentManager mCloudContentManager;
    AnimationsManager mAnimationsManager;
    bool mIsAugmentationVisible;
    
    protected override void Start()
    {
        base.Start();

        mCloudRecoBehaviour = FindObjectOfType<CloudRecoBehaviour>();
        mCloudContentManager = FindObjectOfType<CloudContentManager>();
        mAnimationsManager = FindObjectOfType<AnimationsManager>();

        // Hide the Canvas Augmentation
        base.OnTrackingLost();
    }
    
    public void OnReset()
    {
        Debug.Log("<color=blue>OnReset()</color>");

        // Changing CloudRecoBehaviour.enabled to true will call CloudRecoObserver.Activate()
        // and also call all registered ICloudRecoEventHandler.OnStateChanged() with true.
        mCloudRecoBehaviour.enabled = true;
        mIsAugmentationVisible = false;

        // Hide the Canvas Augmentation
        base.OnTrackingLost();
        mCloudRecoBehaviour.ClearObservers(false);
        //mCloudContentManager.PanelAugmented.SetActive(false);
    }
    
    /// <summary>
    /// Method called from the CloudRecoEventHandler
    /// when a new target is created
    /// </summary>
    public void TargetCreated(CloudRecoBehaviour.CloudRecoSearchResult targetSearchResult)
    {
        mAnimationsManager.SetInitialAnimationFlags();
        string poiName = targetSearchResult.TargetName;
        mCloudContentManager.HandleMetadata(targetSearchResult.MetaData,poiName);
    }
    
    protected override void OnTrackingFound()
    {
        Debug.Log("<color=blue>OnTrackingFound()</color>");

        base.OnTrackingFound();

        mIsAugmentationVisible = true;
        // Starts playing the animation to 3D
        //mAnimationsManager.PlayAnimationTo3D(transform.GetChild(0).gameObject);
    }

    protected override void OnTrackingLost()
    {
        Debug.Log("<color=blue>OnTrackingLost()</color>");
        // Checks that the book info is displayed
        if (mIsAugmentationVisible)
        {
            // Starts playing the animation to 2D
            mAnimationsManager.PlayAnimationTo2D(transform.GetChild(0).gameObject);
            
        }





    }
    
}


