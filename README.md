DynamoUnfold
============

Library for building topology from sets of surfaces and unfolding them using Protogeometry tools in Dynamo

Includes References to
* DynamoCore
* Nunit
* DynamoText
* Protogeometry
* 
![alt tag](https://github.com/holyjewsus/DynamoUnfold/blob/master/unfold%20images/Screen%20Shot%202014-08-02%20at%204.20.53%20PM.png)



###Building / Installing###
Building this library requires that you have also cloned the Dynamo Project and DynamoText Project

**These projects must be located in the same directory as DynamoUnfold has been cloned to.**

####Steps
- **Clone** Dynamo [DynamoRepo](https://github.com/DynamoDS/Dynamo)
- **Build** Dynamo
- **Clone** DynamoText [DynamoTextRepo](https://github.com/DynamoDS/DynamoText)
- **Build** DynamoText
- **Clone** DynamoUnfold - *you are here*
- **Build** DynamoUnfold

The reason for this build order is to ensure that both DynamoText and DynamoUnfold are referencing the same version of the geometry library interfaces and the Dynamo Core.

###Importing Into Dynamo###
Once DynamoUnfold is finished building, navigate to **DynamoUnfold/Bin/...** and import the DynamoUnfold.dll into Dynamo using library import.

##Pre-built##
If you are looking for the necessary dlls to import through library import into Dynamo check the Pre-built folder. These .dlls will be updated periodically **but they may be out of snyc with the version of dynamo you are using!**

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
