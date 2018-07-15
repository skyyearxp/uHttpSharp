# BUILD

## Build tools required

You can simply install the latest visual studio version (2017 atm).

Or, if you don't want to install everything but just build with `build.bat` or a third party IDE, you can install msbuild tools :

https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2017

With the following options :

- MsBuild tools
- .Net Core build tools
- Individual components :
  - .Net framework 4.6.1 SKD
  - .Net framework 4.6.1 Targeting pack

## Manual build command

```bat
build.bat
```

## Additionnal remarks

*Why are the libraries like Oetools.Packager targetting explicitly net461 AND netstandard2.0?*

This question is legit, according to this [net-implementation-support table](https://docs.microsoft.com/en-us/dotnet/standard/net-standard#net-implementation-support), we should be able to make our libraries target netstandard2.0 and that's it. Since our application is targetting v4.6.1 and since v4.6.1 implements netstandard2.0 we should be good. But nop! This is explained here :

- https://www.youtube.com/watch?v=u67Eu_IgEMs&list=PLRAdsfhKI4OWx321A_pr-7HhRNk7wOLLY&index=8
- https://stackoverflow.com/questions/47365136/why-does-my-net-standard-nuget-package-trigger-so-many-dependencies/47366401#47366401
- also for reference on pb with nuget : https://github.com/dotnet/standard/issues/481

So yeah. I have to explicitly target net461.

## Creating a releasing

### Manual chores

- Update the VersionPrefix in uHttpSharp.csproj, the format should be X.X.X (VersionSuffix)
- Check the default Config + UserCommunication.Notify()
- Create a new tag : `git tag vX.X.X` (the last digit must be left empty for the tag name!)
- Push the tag `git push --tags` : this will trigger the build with the deployment on the appveyor
- Wait for the build to be done, edit the newly created https://github.com/jcaillon/3P/releases
  - don't forget to check/uncheck the prerelease option
  - Find a cool title
  - past the body from the `NEXT_RELEASE_NOTES.md` file
  - Publish the release!

### Done automatically by appveyor

- Clean/Rebuild in release mode, not debug for both x86 and x64 versions
- Create 2 .zip in the release on github :
  - "3P.zip" containing the 3P.dll (32 bits) and eventually the .pdb file
  - "3P_x64.zip" containg the 3P.dll (64 bits!) and eventually the .pdb file

# DEPLOYMENT TEST

## Update Notepad++ plugin manager

- For the x86 version, it happens here : https://npppm.bruderste.in/
- For the x64 version, at the moment, it happens here : https://github.com/bruderstein/npp-plugins-x64

### How to test the plugin manager

Dev list : http://nppxmldev.bruderste.in/pm/xml/plugins.zip

Normal list : http://nppxml.bruderste.in/pm/xml/plugins.zip

Mock the files download by the plugin manager with fiddler :

 - Set plugin manager to work with development list of plugins (in plugin manager settings).
 - install fiddler, start capturing traffic (File -> Capture); also activate Tools -> Options -> HTTPS -> decrypt https
 - Right panel, tab AutoResponder, import the .farx file describes below (replace the correct directory) :

```xml
<?xml version="1.0" encoding="utf-8"?>
<AutoResponder LastSave="2018-03-05T16:20:08.6128501+01:00" FiddlerVersion="5.0.20173.50948">
  <State Enabled="true" Fallthrough="true" UseLatency="false">
    <ResponseRule Match="EXACT:https://nppxmldev.bruderste.in/pm/xml/plugins64.zip" Action="C:\Users\Julien\Desktop\pm\xml\plugins64.zip" Enabled="true" />
    <ResponseRule Match="EXACT:https://nppxmldev.bruderste.in/pm/xml/plugins64.md5.txt" Action="C:\Users\Julien\Desktop\pm\xml\plugins64.md5.txt" Enabled="true" />
    <ResponseRule Match="regex:https://nppxmldev\.bruderste\.in/pm/validate\?md5=.+" Action="C:\Users\Julien\Desktop\pm\xml\203_Response.txt" Latency="80" Enabled="true" />
    <ResponseRule Match="EXACT:https://nppxmldev.bruderste.in/pm/xml/plugins2.md5.txt" Action="C:\Users\Julien\Desktop\pm\xml\plugins2.md5.txt" Latency="374" Enabled="true" />
    <ResponseRule Match="EXACT:https://nppxmldev.bruderste.in/pm/xml/plugins.zip" Action="C:\Users\Julien\Desktop\pm\xml\plugins.zip" Latency="169" Enabled="true" />
  </State>
</AutoResponder>
```

- generate the real md5 with git bash `md5sum myfile`
- 203_response.txt mock the validate md5 (validate\?md5=)

```http
HTTP/1.1 200 OK
Server: nginx/1.11.13
Date: Mon, 05 Mar 2018 15:35:35 GMT
Content-Type: text/plain; charset=utf-8
Content-Length: 2
Connection: keep-alive
etag: "8fb12585774dd5672d03239d08c49e5bd6d36088"
last-modified: 2018-03-05T15:33:23.561Z
cache-control: max-age=14400, must-revalidate, public
accept-ranges: bytes

ok
```

- plugins2.md5.txt/plugins64.md5.txt contains md5 checksum for plugin.zip/plugin64.zip, just put something random so it is re-downloaded each time
- plugin.zip/plugin64.zip contains 1 file which is the plugins list for each version :

```bat
copy plugins64.xml PluginManagerPlugins.xml
7z a "plugins64.zip" "PluginManagerPlugins.xml"

copy plugins.xml PluginManagerPlugins.xml
7z a "plugins.zip" "PluginManagerPlugins.xml"
```

### x32 plugin xml

```xml
<?xml version="1.0" encoding="UTF-8"?>
<plugins>
	<plugin name="3P - Progress Programmers Pal">
	  <unicodeVersion>1.8.1</unicodeVersion>
	  <aliases>
	     <alias name="3P" />
	  </aliases>
	  <description>[Requires .net framework 4.6.2]\n\n3P is a notepad++ plug-in designed to help writing OpenEdge ABL (formerly known as Progress 4GL) code. It provides :\n\n- a powerful auto-completion\n- tool-tips on every words\n- a code explorer to quickly navigate through your code\n- a file explorer to easily access all your sources\n- the ability to run/compile and even PROLINT your source file with an in-line visualization of errors\n- more than 50 options to better suit your needs\n- and so much more!\n\nVisit http://jcaillon.github.io/3P/ for more details on the plugin</description>
	  <author>Julien Caillon</author>
	  <homepage>http://jcaillon.github.io/3P/</homepage>
	  <sourceUrl>https://github.com/jcaillon/3P</sourceUrl>
	  <latestUpdate>More infos here :\nhttps://github.com/jcaillon/3P/releases/tag/v1.8.1</latestUpdate>
	  <stability>Good</stability>
	  <install>
	     <unicode>
	        <download>https://github.com/jcaillon/3P/releases/download/v1.8.1/3P.zip</download>
	        <copy from="3P.dll" to="$PLUGINDIR$\" validate="true" />
	        <run file="NetFrameworkChecker.exe" arguments="-ShowOnlyIfNotInstalled" outsideNpp="0" />
	     </unicode>
	  </install>
	  <remove>
	     <unicode>
	        <delete file="$PLUGINDIR$\3P.dll" />
	     </unicode>
	  </remove>
	</plugin>
</plugins>
```

### x64 plugin xml

```xml
<?xml version="1.0" encoding="UTF-8"?>
<plugins>
	<plugin name="3P - Progress Programmers Pal">
		<x64Version>1.8.1</x64Version>
		<aliases>
			<alias name="3P"/>
		</aliases>
		<description>[Requires .net framework 4.6.2]\n\n3P is a notepad++ plug-in designed to help writing OpenEdge ABL (formerly known as Progress 4GL) code. It provides :\n\n- a powerful auto-completion\n- tool-tips on every words\n- a code explorer to quickly navigate through your code\n- a file explorer to easily access all your sources\n- the ability to run/compile and even PROLINT your source file with an in-line visualization of errors\n- more than 50 options to better suit your needs\n- and so much more!\n\nVisit http://jcaillon.github.io/3P/ for more details on the plugin</description>
		<author>Julien Caillon</author>
		<homepage>http://jcaillon.github.io/3P/</homepage>
		<sourceUrl>https://github.com/jcaillon/3P</sourceUrl>
		<latestUpdate>More infos here :\nhttps://github.com/jcaillon/3P/releases/tag/v1.8.1</latestUpdate>
		<stability>Good</stability>
		<install>
			<x64>
				<download>https://github.com/jcaillon/3P/releases/download/v1.8.1/3P_x64.zip</download>
	            <copy from="3P.dll" to="$PLUGINDIR$\" validate="true" />
				<run file="NetFrameworkChecker.exe" arguments="-ShowOnlyIfNotInstalled" outsideNpp="0" />
			</x64>
		</install>
		<remove>
			<x64>
	            <delete file="$PLUGINDIR$\3P.dll" />
			</x64>
		</remove>
	</plugin>
</plugins>
```