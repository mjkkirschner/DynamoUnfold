DynamoUnfold
============

Library for building topology from sets of surfaces and unfolding them using Protogeometry tools in Dynamo.

This project includes references to:

- DynamoCore
- Nunit
- DynamoText
- DynamoPack
- Protogeometry


![alt tag](https://github.com/holyjewsus/DynamoUnfold/blob/master/unfold%20images/Screen%20Shot%202014-08-02%20at%204.20.53%20PM.png)



###Building / Installing###
Building this library requires that you have also cloned the Dynamo Project and DynamoText Project.

**These projects (Dynamo,DynamoText) must be located in the same directory that DynamoUnfold has been cloned into.**

#####Build Steps
1. **Clone** Dynamo [Dynamo](https://github.com/DynamoDS/Dynamo)
2. **Build** Dynamo 
3. **Clone** DynamoText [DynamoText](https://github.com/holyjewsus/DynamoText/tree/fixnetversion)
4. **Build** DynamoText
5. **Clone** DynamoPack [DynamoPack](https://github.com/holyjewsus/DynamoPack/tree/structurePackLikeDynamotext)
6. **Build** DynamoPack 
7. **Clone** DynamoUnfold - *you are here*
8. **Build** DynamoUnfold

The reason for this build order is to ensure that both DynamoText and DynamoUnfold are referencing the same version of the geometry library interfaces and the Dynamo Core.  You can actually clone all repos at the same time, but **Dynamo** must be built before the others.

###Importing Into Dynamo###
Once DynamoUnfold is finished building, navigate to **DynamoUnfold/bin/...** and import the DynamoUnfold.dll into Dynamo using library import.

###Pre-built###
If you are just looking for the necessary .dll's to import through **Zero-Touch Import** into Dynamo check the Pre-built folder. These .dll's will be updated periodically - **but they may be out of snyc with the version of dynamo you are using!** Also Does Not Exist YET!

###Running Unit Tests###
TODO


####Known Issues####
- There is a speed issue when unfolding a tesselated surface with more than approximately 1000 faces, stems from the conversion of triangle data to surfaces.
- There is an intersection bug that occurs intermittently - more so with large numbers of surfaces, this is being looked into. 

## Dynamo License ##

Those portions created by Ian are provided with the following copyright:

Copyright 2014 Ian Keough

Those portions created by Autodesk employees are provided with the following copyright:

Copyright 2014 Autodesk


Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.


###NUnit####

http://www.nunit.org/

http://www.nunit.org/index.php?p=license&r=2.6.2  

###DynamoPack###
This library makes use of the Nuclex Framework's [CygonRectanglePacker](https://devel.nuclex.org/framework/wiki/RectanglePacking).

[Eclipse CPL](http://www.ibm.com/developerworks/library/os-cpl.html)
