import smtplib
import string
import subprocess
import datetime
import sys
import shutil
import re
import os
import fnmatch
from optparse import OptionParser


def main():

	parser = OptionParser()
	parser.add_option("-j","--justbuild",dest="justbuildbool",help = "bool toggle to just build if clone already occured", default = False, action = "store_true")
	parser.add_option("-r", "--remote", dest="remote", help="The URI of the git remote.", metavar="FILE", default="https://github.com/holyjewsus/DynamoUnfold.git")
	parser.add_option("-d", "--date", dest="repo_date", default=date_string(), help="Override the date of the build")
	parser.add_option("-n", "--repo_name", dest="repo_name", default="DynamoUnfold" , help="The name of the repo.  Don't screw this up.")
	parser.add_option("-p", "--sol_path", dest="solution_path", default="src/DynamoUnfold.sln", help="Solution path relative to repo root.")
	parser.add_option("-b", "--msbuild_path", dest="msbuild_path", default="C:/Windows/Microsoft.NET/Framework/v4.0.30319/MSBuild.exe", help="Path to MSBuild.exe")
	
	
	(options, args) = parser.parse_args()

	executing_path = os.path.dirname(os.path.realpath(__file__))
	sandbox_path = form_path(['repos','sandbox'])
	sandbox_path = form_path([executing_path,sandbox_path])
	repo_root = form_path( [sandbox_path, options.repo_name ] ) 
	solution_path = form_path( [repo_root, options.solution_path] ) 


	print "I am going to build DynamoUnold in", solution_path
	if options.justbuildbool == False:
		rm_dir(sandbox_path)


	# do auto-build!
	setup(sandbox_path)

	##pull all dependencies
	depNames = ['Dynamo','DynamoText','DynamoPack']
	depUris = ["https://github.com/DynamoDS/Dynamo.git","https://github.com/holyjewsus/DynamoText.git","https://github.com/holyjewsus/DynamoPack.git"]
	depSolRelPaths = ["src/Dynamo.All.2012.sln", "DynamoText.sln","src/DynamoPack.sln"]
	depSolFullPaths = []
	for name in depNames:
		index = depNames.index(name)
		cur_repo_root = form_path( [sandbox_path, name ] ) 
		cur_solution_path = form_path( [cur_repo_root, depSolRelPaths[index]] ) 
		depSolFullPaths.append(cur_solution_path)
	
	print "justbuildbool was false, cloning",options.justbuildbool
	if options.justbuildbool == False:
		dynamo_pull = clone_single_branch( depUris[0], sandbox_path, depNames[0],"master" ) 
		# for now grab specific branches of these repos
		dynamotext_pull = clone_single_branch( depUris[1], sandbox_path, depNames[1], "fixnetversion") 
		dynamopack_pull = clone_single_branch( depUris[2], sandbox_path, depNames[2],"structurePackLikeDynamotext" ) 

		## pull DynamoUnfold
		pull_result = clone( options.remote, sandbox_path, options.repo_name ) 


	#build dependencies in order

	dynamobuild_result = build( options.msbuild_path, depSolFullPaths[0], build_config = "Release" )
	dynamobuild_result_debug = build( options.msbuild_path, depSolFullPaths[0], build_config = "Debug" )

	dynamotextbuild_result = build( options.msbuild_path, depSolFullPaths[1], build_config = "Release" )
	dynamotextbuild_result_debug = build( options.msbuild_path, depSolFullPaths[1], build_config = "Debug" )

	dynamopackbuild_result = build( options.msbuild_path, depSolFullPaths[2], build_config = "Release" )
	dynamopackbuild_result_debug = build( options.msbuild_path, depSolFullPaths[2], build_config = "Debug" )

	# # then build DynamoUnfold

	build_result = build( options.msbuild_path, solution_path, build_config = "Release" )
	build_result_debug = build( options.msbuild_path, solution_path, build_config = "Debug" )

	print ("finished building DynamoUnfold to " ,sandbox_path)

	
def setup(path):
	mkdir(path)

def cleanup(pull_to):
	rm_dir(pull_to)

def rm_dir(path):

	if os.path.exists(path):
		print "removing", path
		run_cmd(['rmdir', '/S', '/Q', path])

def mkdir(path):

	if os.path.exists(path):
		return

	os.makedirs( path )

def date_string():
	return datetime.datetime.now().strftime("%Y%m%dT%H%M%S")
	
def clone( git_repo_path_https, pull_dir, repo_name ):
	print 'cloning...','at', pull_dir
	return run_cmd(['git','clone', git_repo_path_https], cwd = pull_dir )		

def clone_single_branch( git_repo_path_https, pull_dir, repo_name,branch_name):
	print 'cloning...a single branch','at', pull_dir
	return run_cmd(['git', 'clone', git_repo_path_https, '-b', branch_name, '--single-branch'], cwd = pull_dir)

def form_path(elements):
	return '\\'.join( elements )

def interpret_build(result):

	errors = int( re.search( r"[0-9]+ Error", result).group(0).split(' ')[0] )
	warnings = int( re.search( r"[0-9]+ Warning", result).group(0).split(' ')[0] )

	return {'result': result, 'errors': errors, 'warnings': warnings, 'success': ( errors == 0) }

def build( compiler_path, solution_path, build_config = "Release" ):

	print 'building....'
	print "the cd is", solution_path
	return interpret_build( run_cmd([compiler_path, '/p:Configuration='+build_config, solution_path]) )
		
def run_cmd( args, printOutput = True, cwd = None ):	
	p = subprocess.Popen(args, shell=True, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, cwd = cwd)
	
	out = ''

	while True:
	    line = p.stdout.readline()
	   
	    if not line:
	        break
	    out = out + line
	    print ">>> " + line.rstrip()
		
	return out
	

if __name__ == "__main__":
    main()

