# iPhone CinematicDepth To 3D

## 1. Overview

This sample consists of two projects:
- DepthExtractor:<br>
A C# sample project for separating Depth and Color from videos shot in Cinematic mode.
- DepthPlayer:<br>
A Unity sample project to display Depth and Color videos in 3D.
<br>
[![](https://img.youtube.com/vi/MR8TF1z-nTg/0.jpg)](https://www.youtube.com/watch?v=MR8TF1z-nTg)

## 2. Cinematic Mode Compatibility
Refer to [Apple Support](https://support.apple.com/ja-jp/HT212778).  
Additionally, the iPhone 15 Series is compatible.

## 3. iPhone Settings

### Camera Settings
1. Settings -> Camera -> Formats
2. Choose "High Efficiency" under the Camera Capture section

### File Transfer Settings
1. Settings -> Photos
2. Select "Keep Originals" under the TRANSFER TO MAC OR PC section

## 4. Transfer Video Files to PC

### [USB Transfer]

#### For Windows
1. Connect the iPhone to your PC with a USB cable
2. Using Explorer, locate the cinematic video you wish to use
3. Copy it to any folder on your PC  
   **Note:** When shooting a video, filenames like IMG_0001.MOV and IMG_E0001.MOV might be generated. Always select files without the "E", such as IMG_0001.MOV.

#### For Mac
1. Connect the iPhone to your Mac using a USB cable
2. Launch the Photos app on your Mac
3. Choose "iPhone" from the list on the left side of the window
4. Select the video you wish to import and click the Import button
5. Choose "Imports" from the list on the left side of the window
6. From the menu bar at the top, select File -> Export
7. Click on "Export Unmodified Original for 1 Video"
8. Hit the Export button
9. Save to your desired folder

### [Network Transfer (iOS 17 and above)]
1. Launch the Photos app on your iPhone
2. Select the cinematic video and tap the Share button
3. Tap "Export Unmodified Original"
4. Select a destination folder and tap the Save button
5. Open the Files app
6. Share the video saved in step 4 as you see fit, e.g., through Google Drive.

## 5. Separating Depth and Color Videos (Windows Only)
1. Navigate to the DepthExtractor_Win folder
2. Start DepthExtractor.exe from the Executable folder or run the project in the Project folder using Visual Studio
3. Click the Open button and select a cinematic video
4. Hit the Convert button and wait for "Done" to be displayed
5. `color_output.webm` and `depth_output.webm` will be generated in the same directory as the video you selected in step 3.  
   **Note:** The depth video size is either 512x288 or 288x512, while the color video size is 512x512.

## 6. Display in 3D within Unity
1. Open the DepthPlayer project in Unity
2. Place `color_output` and `depth_output` into any directory under the Assets folder  
   e.g., within the VideoFiles directory
3. Double-click on DepthPlayer inside the Scenes directory
4. In the Hierarchy, select the [Main] object
5. Drag & drop `color_output` onto "Color Video" in the Inspector
6. Drag & drop `depth_output` onto "Depth Video" in the Inspector
7. At the top of the UnityEditor, click the Play button  
   **Note 1:** Adjust the displayed object size by changing the Scale of the [DepthMesh] object.  
   **Note 2:** To solely alter the Depth Scale, you can modify the Depth Scale value (Default=1.5) of the [Main] object.

## 7. License
While this sample is under the MIT License, be aware of the following dependencies and their respective licenses:
- [GPAC2.2](https://gpac.wp.imt.fr/) - License: [GNU Lesser General Public License, version 2.1]
- [FFmpeg](https://ffmpeg.org/) - License: [GNU Lesser General Public License, version 2.1]
