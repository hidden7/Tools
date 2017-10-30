vrCluster tools
===============

Welcome to the **[vrCluster](http://vrcluster.io)** tools source code! 




Description
-----------

vrCluster tools collection is a part of vrCluster.

This is the root repository for vrCluster tools development. From here, you can clone and build next application:

| Tool              | Purpose |
|-------------------|---------|
| vrClusterManager  | Cluster control, cluster configurations management |
| vrClusterListener | Handles cluster control commands on remote machines |




Documentation
-------------
We have some documentation available for the vrCluster on the web. If you're looking for the answer to something, you may want to start here: 

[Documentation page](http://vrcluster.io/documentation/)

If you need more, just ask!

[Write us](http://vrcluster.io/#contact)




Tags
--------

The **release tags** with naming pattern **release_X.Y** where *X.Y* is MAJOR.MINOR version of tools collection. Release tags specify codebase of vrCluster tools public releases.




Branches
--------

We keep source for the vrCluster tools in the next branches:

The **[master branch](https://github.com/vitaliiboiko/Tools/tree/master)** tracks [live changes](https://github.com/vitaliiboiko/Tools/commits/master) by our team. 


The **[release branch](https://github.com/vitaliiboiko/Tools/tree/release)** tracks public releases.


The **release candidate branches** with naming pattern **rc_X.Y**. Those short-lived branches contain code for pre-release actions (testing, hotfixes).


Other short-lived branches may pop-up from time to time as we stabilize new releases or hotfixes.




Getting up and running
----------------------

The steps below will take you through cloning your own private fork, then compiling and running the tools:

### Windows

1. Install **[GitHub for Windows](https://windows.github.com/)** then **[fork and clone our repository](https://guides.github.com/activities/forking/)**. 
   To use Git from the command line, see the [Setting up Git](https://help.github.com/articles/set-up-git/) and [Fork a Repo](https://help.github.com/articles/fork-a-repo/) articles.

   If you'd prefer not to use Git, you can get the source with the 'Download ZIP' button on the right. The built-in Windows zip utility will mark the contents of zip files 
   downloaded from the Internet as unsafe to execute, so right-click the zip file and select 'Properties...' and 'Unblock' before decompressing it. Third-party zip utilities don't normally do this.

1. Install **Visual Studio 2017**. 
   All desktop editions of Visual Studio 2017 can build UE4, including [Visual Studio Community 2017](http://www.visualstudio.com/products/visual-studio-community-vs), which is free for small teams and individual developers.
  
1. Open *vrClusterTools.sln* solution file in Visual Studio.

1. Select *Release* configuration from dropdown list on a toolbar.

1. From menu, press *Build* -> *Build Solution*.

1. That's it! Find your executables in *bin\Release* subdirectories.
