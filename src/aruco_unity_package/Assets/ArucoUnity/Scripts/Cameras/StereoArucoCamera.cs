﻿namespace ArucoUnity
{
  /// \addtogroup aruco_unity_package
  /// \{

  namespace Cameras
  {
    /// <summary>
    /// Captures every frame the images of stereo camera system.
    /// </summary>
    public abstract class StereoArucoCamera : ArucoCamera
    {
      // IArucoCamera properties

      public override int CameraNumber { get { return 2; } }

      // Properties

      /// <summary>
      /// Gets the id of the first camera.
      /// </summary>
      public static int CameraId1 { get { return 0; } }

      /// <summary>
      /// Gets the id of the second camera.
      /// </summary>
      public static int CameraId2 { get { return 1; } }
    }
  }

  /// \} aruco_unity_package
}