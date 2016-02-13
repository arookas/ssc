-- options
newoption {
	trigger = "lib-dir",
	description = "Sets the path for the dependencies",
	value = "PATH"
}

newoption {
	trigger = "clean-symbols",
	description = "Cleans up the symbol table from unused symbols"
}

if not _OPTIONS["lib-dir"] then
	_OPTIONS["lib-dir"] = "lib"
end

-- main workspace
workspace "ssc"
	configurations { "Debug", "Release" }
	targetdir "bin/%{cfg.buildcfg}"
	libdirs { _OPTIONS["lib-dir"] }
	
	startproject "frontend"
	
	filter "configurations:Debug"
		defines { "DEBUG" }
		flags { "Symbols" }
	
	filter "configurations:Release"
		defines { "RELEASE" }
		optimize "On"
	
	-- sunscript compiler API library
	project "ssc"
		kind "SharedLib"
		language "C#"
		namespace "arookas"
		location "ssc"
		
		links { "System", "arookas", "grammatica-1.6" }
		
		-- apply options
		if _OPTIONS["clean-symbols"] then
			defines { "SSC_CLEAN_SYMBOLS" }
		end

		files {
			"ssc/**.cs",
			"ssc/**.grammar",
			"ssc/**.bat",
		}
		
		excludes {
			"ssc/bin/**",
			"ssc/obj/**",
		}
		
		prebuildcommands {
			-- regenerate grammatica classes before compilation begins
			"{CHDIR} \"%{prj.location}\"",
			"java -jar grammatica.jar \"sunscript.grammar\" --csoutput \".\\generated\" --csnamespace \"arookas\" --csclassname \"__sun\"",
		}

	-- frontend project (example command-line interface)
	project "frontend"
		kind "ConsoleApp"
		language "C#"
		entrypoint "arookas.SSC"
		namespace "arookas"
		location "frontend"
		
		links { "System", "arookas", "SSC" }
		
		files {
			"frontend/**.cs",
		}
		
		excludes {
			"frontend/bin/**",
			"frontend/obj/**",
		}
		
		postbuildcommands {
			-- copy stdlib to frontend output so users can import the scripts
			"{RMDIR} \"%{cfg.buildtarget.directory}ssc\"",
			"{COPY} \"%{wks.location}stdlib\" \"%{cfg.buildtarget.directory}ssc\"",
		}

	-- sbdump utility
	project "sbdump"
		kind "ConsoleApp"
		language "C#"
		entrypoint "arookas.sbdump"
		namespace "arookas"
		location "sbdump"
		
		links { "System", "arookas" }
		
		files {
			"sbdump/**.cs",
		}
		
		excludes {
			"sbdump/bin/**",
			"sbdump/obj/**",
		}
