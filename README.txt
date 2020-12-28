HTK (c) mjt, 2015

Released under MIT license.

Htk/
	htk.sln				solution file for VS2010 (express)
	Data/				datas
	Game/				small test game
	Htk/				C# htk framework
	HSceneEditor/		scene editor
	Tests/				some tests
	Libs/				libraries and dlls

Tests are written with C#, using Horde3D+OpenTK.

Converting models:	
 copy .dae model and textures to 
 Data/colladaConv/ directory and run Conv.bat 
 which converts and moves datas to their own directories.

Editor bug:
 if using models that doesnt have texture, and File->New, it doesnt
 work right. Solution: use only textured models.
 

* 3 ways to load data:
  Horde3DUtils.loadResourcesFromDisk(Settings.ContentDir); ## load datas and use directories within .xml files
  Util.LoadResourcesFromDisk(Settings.ContentDir);         ## load datas and use own dirs (ie load *.jpg under textures/ )
  Util.LoadResourcesFromDisk("Content", "Content.zip");    ## load datas from zip (under Content/ )
