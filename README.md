# iPhone CinematicDepth To 3D

## 1. Overview

This sample consists of two projects:
- DepthExtractor:<br>
A C# sample project to separate Depth and Color from videos shot in Cinematic mode.
- DepthPlayer:<br>
An Unity sample project to display Depth and Color videos in 3D.

## 2. Cinematic Mode Compatibility
See [Apple Support](https://support.apple.com/ja-jp/HT212778)  
Also, iPhone 15 Series are compatible.

## 3. iPhone Settings

### Camera Settings
1. Settings -> Camera -> Formats
2. Choose "High Efficiency" under the Camera Capture section

### File Transfer Settings
1. Settings -> Photos
2. Select "Keep Originals" under the TRANSFER TO MAC OR PC section

## 4.Transfer Video Files to PC

### [USB Transfer]

#### For Windows
1. Connect iPhone to PC with USB cable
2. Find the cinematic video which you want to use by using Explorer
3. Copy it to any folder on the PC  
   **Note:** When shooting a video, filenames like IMG_0001.MOV and IMG_E0001.MOV might be generated. Make sure to select files without the "E", like IMG_0001.MOV.

#### For Mac
1. Connect iPhone to Mac with USB cable
2. Open Mac's Photos app
3. Choose "Phone" from the list on the left side of the window
4. Select the video you want to import and click the Import button
5. Select "Imports" from the list on the left side of the window
6. From the menu bar at the top of the screen, choose File -> Export
7. Click "Export Unmodified Original for 1 Video"
8. Click the Export button
9. Save to any folder

### [Network Transfer (iOS 17 and above)]
1. Launch the Photos app of a iPhone
2. Select a cinematic video and tap the Share button
3. Tap "Export Unmodified Original"
4. Choose a folder and tap Save button
5. Launch the Files app
6. Share the video saved in step 4 in the way you prefer, e.g., through Google Drive.

## 5. Separating Depth and Color Videos (Only for Windows)
1. Open the DepthExtractor_Win folder
2. Launch DepthExtractor.exe in the Executable folder or run the project in the Project folder by using Visual Studio
3. Click the Open button and select a cinematic video
4. Click the Convert button and wait until "Done" is displayed
5. color_output.webm and depth_output.webm will be generated in the same folder as the video selected in step 3.  
   **Note:** The size of the depth video is 512x288 or 288x512, and the color video size is 512x512.

## 6. Display as 3D image in Unity
1. Open the DepthPlayer project in Unity
2. Add color_output and depth_output to any folder under the Assets folder  
   e.g., Inside the VideoFiles folder
3. Double-click DepthPlayer in the Scenes folder
4. Select the [Main] object in the Hierarchy
5. Drag & drop color_output onto "Color Video" in the Inspector
6. Drag & drop depth_output onto "Depth Video" in the Inspector
7. Click the Play button at the top of the UnityEditor to play  
   **Note 1:** To change the display object size, modify the Scale of the [DepthMesh] object.  
   **Note 2:** To change only the Depth Scale, you can also modify the Depth Scale value (Default=1.5) of the [Main] object.

## 7. License
This project utilizes the following open-source software:
- [GPAC2.2](https://gpac.wp.imt.fr/) - License: [GNU Lesser General Public License, version 2.1]
- [FFmpeg](https://ffmpeg.org/) - License: [GNU Lesser General Public License, version 2.1]
