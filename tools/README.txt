ABOUT

autoclone_andbuild.py is a python script in /tools/ that will help you build DynamoUnfold without cloning any repos yourself.
It uses git at the command line to clone all the repos necessary (Dynamo,DynamoText,DynamoPack,and DynamoUnfold) and then fires up MSBuild, builds both release and debug.

USAGE

At the command line, you should be able to run:

	autoclone_andbuild.py or python autoclone_andbuild.py

You can get help by typing:

	autoclone_andbuild.py –h

	autoclone_andbuild.py -j will just build all the libraries and skip the cloning step.

This will print out additional command line arguments.  

DEPENDENCIES

You’ll need the following installed to use it:

	python - http://www.python.org/download/releases/2.7/
	MSBuild – http://www.microsoft.com/en-us/download/details.aspx?id=8279
	GIT - I tested this script using http://msysgit.github.io, when installing make sure to enable calling git from the command line.