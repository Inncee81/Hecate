# Hecate

Build systems are often a messy set of scripts and configuration files that let you build, test, 
package, deliver, and install your code. As a developer, you either love or loathe build systems. 
Hecate is the software development engine and so, the primary heart of the entire  SDK. 
Software development is a complex activity that involves tasks such as code management, code generation,  
automated source code checks, documentation generation, compilation,linking, packaging, creating  
binary releases, source-code releases and deployment. Every change a developer, graphic artist, 
technical writer, or any other person that creates source content makes, must trigger at least a 
partial rebuild of the entire game.

Hecate is a custom tool that manages every aspect of a game related software project from setting  up  
the source base, installing and managing modules up to the process of building source code across 
a variety of build configurations. It also contains a customization of NPM, a package manager for 
source packages. A package contains all the files you need for a module which are language 
independent code libraries you can include in your project

## Convention over Configuration

Convention over configuration is a principle that has successfully governed in domains like web  
frameworks, reducing the learning curve and increasing developer productivity. It demands that a 
project is organized in a consistent way. Regular directory structure is the key principle on which Hecate  
rests. Even in the most complicated systems, there is usually a relatively small high-level directory 
structure that contains special purpose directories. Usually the build system should be aware of the location 
and names of the top-level directories and their meaning to the project.

The SDK is split into many modules. Each module has a base directory that controls how it is built, which 
files belong to the module, a package.json defining module dependencies, a well set unique name and version 
used by NPM lookups.Each directory usually contains a small number of file types that are automatically 
discovery based on extension. The build system usually knows what files to expect, how to handle each file 
type and performs many tasks on your behalf automatically. In particular, it doesn't even need a build file 
in each directory that tells it what files are in it, how to build them or even a monolithic “solution” that 
points to every single directory. The regular directory structure, combined with knowledge of files types 
(e.g., .cpp or .h files), allows the build system to figure out what files it needs to take into account, 
so developers just need to make sure the right files are in the right directory.

Managing dependencies can be simple or complicated depending onthe project. In any case, missing a dependency 
leads to linking errors that are often hard to resolve. This build system analyzes the language specific 
statements in the source files and recursively creates a complete dependencies tree. The dependencies tree 
determines what static libraries a dynamic library or executable needs to link against. However, Hecate 
is designed in a global fashion so that it can resolve dependencies to modules that are installed into the 
SDK directory as well as those defined per project.

Different IDEs, as well as command-line based tools like Make, use different build files to represent the meta 
information needed to build the software. Hecate  maintains  the  same information via its inherent knowledge 
combined with the regular directory structure and can generate build files for any other build system by populating 
the appropriate templates. This approach lets developers build the software via their favorite IDE (like Visual Studio) 
without the hassle involved in adding files,setting dependencies, and specifying compiler and linker flags. It is 
important to understand that the build process executes independently of any project files for the development 
environment, such as .sln or .vcproj files (for Visual Studio). These files are useful to have for editing purposes 
though, so there is a tool being provided to generate them dynamically (based on the contents of your project 
directory tree)

## Data Driven Action Trees

Regardless how a build system is setup for a project, all of them have something in common. They perform a set of
actions or nodes that may relate to the output of a previously finished nodes up to the final compilation, building
and deployment steps. Each node consists of tasks executed in sequence to produce some sort of output.

Hecate envelopes these nodes to an actor stream that eliminates many manual steps from the process and enables a smooth,
automated flow of data from one node to the next. The data stream is related to the reactive programming paradigm, which 
allows actors to subscribe for certain kind of data enables annotations for the type of machine that nodes are supposed 
to be executed on, providing a list of recipients for failure notifications if a step fails, and groups nodes that should 
only be executed after an explicit action finished. It automates the processes involved in extracting, transforming, 
combining, validating, and loading data for further execution.

Nodes can consist of two types of objects, regular actors that perform lightweight tasks and perform for example message 
routing to distinct end points over the network, and the so-called Processor Units. It is different from a regular actor 
in such a way that it is assigned to a single kind of processor family that manages exactly one type of data. They can 
be defined and used by Hecate from different sources like user code or a plugin which determines their logical “location”
in the hierarchy, regardless of where the code is originated in.

PUs of the same family can co-exist in one pipeline because they are ordered into a hierarchy that determines which instance 
is currently addressed to the data. Built-in PUs are usually defined by a tool or module in the SDK and form the lowest 
level of the stack. Those that are located at the top of the SDK are prioritized above built-in ones but after those that 
are in the project root. Finally, PUs related to certain path anywhere in the SDK or a project have the highestpriority 
and are chosen if data from the pipeline is covered by the path a PU is assigned to.

The build system’s inherent knowledge combined with the regular directory structure can fulfill a lot of purposes based on 
the built-in default behavior. Anyways, there might be the need of partially altering certain behavior. Defining an 
override for the built-in PU to be altered is the way to achieve that

## Requirements

* Minimum C# 5 on .Net Framework 4.0 or equivalent Mono version
* Windows or Linux Operation System

## Repository

The release branch is usually extensively tested. However, bugs are everywhere and we work hard to make releases stable and
reliable, and aim to publish new releases frequently.

The master branch is our primary branch of changes from all our development efforts and may be buggy - they may not even compile.
Be aware that code taken from here is not supposed to work under all circumstances. Pull requests should always target this branch
to make integration easier for our active GitHub supervision team.

Other short-lived branches may pop-up from time to time as we stabilize new releases or hotfixes

## Contribution

We welcome any contributions to any of our projects development through pull requests on GitHub. Most of our active development is 
in the master branch, so we prefer to take pull requests there (particularly for new features). We try to make sure that all new
code adheres to the [Schroedinger Entertainment Coding Conventions](https://github.com/SchroedingerEntertainment/Docs/blob/master/Guidelines/Code%20Conventions.md) and GNU Affero General Public License v3.0 (or higher) (AGPL). 
All contributions are governed by the terms of these license.

Please note that support commercial game developers and game tool/ engine creators or similar, regardless of their profession and 
experience, and so want to ensure that those persons and studios can continue making the beautiful games we all like. For this reason,
we can't accept any contributions that don't follow the additional terms of our [GNU Affero General Public License Exception](https://github.com/SchroedingerEntertainment/Docs/blob/master/Licenses/AGPLv3%20Exception.md)
