﻿using UnityEngine;

namespace ArucoUnity
{
  /// \addtogroup aruco_unity_package
  /// \{

  namespace Utility
  {
    /// <summary>
    /// Manages a webcam device and updates its associated textures each frame.
    /// Based on: http://answers.unity3d.com/answers/1155328/view.html
    /// </summary>
    public class CameraDevice : ArucoCamera
    {
      // Editor fields

      [SerializeField]
      [Tooltip("The id of the camera device to use.")]
      private int deviceId = 0;

      // ArucoCamera properties implementation

      /// <summary>
      /// The correct image orientation.
      /// </summary>
      public override Quaternion ImageRotation
      {
        get
        {
          return Quaternion.Euler(0f, 0f, -WebCamTexture.videoRotationAngle);
        }
      }

      /// <summary>
      /// The image ratio.
      /// </summary>
      public override float ImageRatio
      {
        get
        {
          return WebCamTexture.width / (float)WebCamTexture.height;
        }
      }

      /// <summary>
      /// Allow to unflip the image if vertically flipped (use for image plane).
      /// </summary>
      public override Mesh ImageMesh
      {
        get
        {
          Mesh mesh = new Mesh();

          mesh.vertices = new Vector3[]
          {
            new Vector3(-0.5f, -0.5f, 0.0f),
            new Vector3(0.5f, 0.5f, 0.0f),
            new Vector3(0.5f, -0.5f, 0.0f),
            new Vector3(-0.5f, 0.5f, 0.0f),
          };
          mesh.triangles = new int[] { 0, 1, 2, 1, 0, 3 };

          Vector2[] defaultUv = new Vector2[]
          {
            new Vector2(0.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(0.0f, 1.0f)
          };
          Vector2[] verticallyMirroredUv = new Vector2[]
          {
            new Vector2(0.0f, 1.0f),
            new Vector2(1.0f, 0.0f),
            new Vector2(1.0f, 1.0f),
            new Vector2(0.0f, 0.0f)
          };
          mesh.uv = WebCamTexture.videoVerticallyMirrored ? verticallyMirroredUv : defaultUv;

          mesh.RecalculateNormals();

          return mesh;
        }
      }

      /// <summary>
      /// Allow to unflip the image if vertically flipped (use for canvas).
      /// </summary>
      public override Rect ImageUvRectFlip
      {
        get
        {
          Rect defaultRect = new Rect(0f, 0f, 1f, 1f),
               verticallyMirroredRect = new Rect(0f, 1f, 1f, -1f);
          return WebCamTexture.videoVerticallyMirrored ? verticallyMirroredRect : defaultRect;
        }
      }

      /// <summary>
      /// Mirror front-facing camera's image horizontally to look more natural.
      /// </summary>
      public override Vector3 ImageScaleFrontFacing
      {
        get
        {
          Vector3 defaultScale = new Vector3(1f, 1f, 1f),
                  frontFacingScale = new Vector3(-1f, 1f, 1f);
          return WebCamDevice.isFrontFacing ? frontFacingScale : defaultScale;
        }
      }

      // Properties

      /// <summary>
      /// The associated webcam device.
      /// </summary>
      public WebCamDevice WebCamDevice { get; protected set; }

      /// <summary>
      /// The texture of the associated webcam device.
      /// </summary>
      public WebCamTexture WebCamTexture { get; protected set; }

      public int DeviceId { get { return deviceId; } set { deviceId = value; } }

      // MonoBehaviour methods

      /// <summary>
      /// Make adjustments to image every frame to be safe, since Unity isn't 
      /// guaranteed to report correct data as soon as device camera is started.
      /// </summary>
      protected void Update()
      {
        if (!Started)
        {
          // Skip making adjustment for incorrect camera data
          if (WebCamTexture.width < 100)
          {
            Debug.Log(gameObject.name + ": Still waiting another frame for correct info.");
            return;
          }
          else
          {
            // Initialize the Texture2D
            Texture2D = new Texture2D(WebCamTexture.width, WebCamTexture.height, TextureFormat.RGB24, false);

            // Notify that the camera has started
            Started = true;
            RaiseOnStarted();
          }
        }
        else
        {
          // Update the Texture2D content
          Texture2D.SetPixels32(WebCamTexture.GetPixels32());
        }
      }

      // ArucoCamera methods

      /// <summary>
      /// Populate <see cref="CameraParameters"/> from a previously saved camera parameters XML file.
      /// </summary>
      /// <param name="cameraParametersFilePath">The file path to load.</param>
      /// <returns>If the camera parameters has been successfully loaded.</returns>
      public override bool LoadCameraParameters(string cameraParametersFilePath)
      {
        CameraParameters = CameraParameters.LoadFromXmlFile(cameraParametersFilePath);
        return CameraParameters != null;
      }

      /// <summary>
      /// Configurate the camera device and its textures.
      /// </summary>
      /// <returns>If the operation has been successfull.</returns>
      public override bool Configurate()
      {
        if (Started)
        {
          Debug.LogError(gameObject.name + ": Stop the camera to configurate it.");
          return false;
        }

        // Try to check for the camera device
        WebCamDevice[] webcamDevices = WebCamTexture.devices;
        if (webcamDevices.Length < DeviceId)
        {
          Debug.LogError(gameObject.name + ": The camera device with the id '" + DeviceId + "' is not found.");
          return false;
        }

        // Switch the camera device
        WebCamDevice = webcamDevices[DeviceId];
        WebCamTexture = new WebCamTexture(WebCamDevice.name);

        return true;
      }

      /// <summary>
      /// Start the camera and the associated webcam device.
      /// </summary>
      public override bool StartCamera()
      {
        if (Started)
        {
          return false;
        }

        WebCamTexture.Play();
        Started = false; // Need some frames to be started, see Update()

        return true;
      }

      /// <summary>
      /// Stop the camera and the associated webcam device, and notify of the stopping.
      /// </summary>
      public override bool StopCamera()
      {
        if (!Started)
        {
          return false;
        }

        WebCamTexture.Stop();
        Started = false;
        RaiseOnStopped();

        return true;
      }
    }
  }

  /// \} aruco_unity_package
}