@ECHO OFF
cd data\colladaConv
mkdir model
copy *.dae model
copy *.tga model
copy *.jpg model
copy *.png model
copy *.dds model
colladaconv.exe model -type model
colladaconv.exe model -type anim

cd model
move *.jpg  ..\..\textures
move *.tga  ..\..\textures
move *.png  ..\..\textures
move *.dds  ..\..\textures

move *.geo  ..\..\models
move *.scene.xml  ..\..\models

move *.material.xml  ..\..\materials

move *.anim  ..\..\animations

move *.particle.xml  ..\..\particles

move *.shader  ..\..\shaders
move *.glsl ..\..\shaders

move *.pipeline.xml  ..\..\pipelines

del . /q
cd..
rmdir model


pause
