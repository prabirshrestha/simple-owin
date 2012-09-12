var fs = require('fs'),
    path = require('path'),
    njake = require('./src/njake'),
    msbuild = njake.msbuild,
    nuget = njake.nuget,
    config = {
        rootPath: __dirname,
        version: fs.readFileSync('VERSION', 'utf-8')
    };

console.log('SimpleOwin v' + config.version)

msbuild.setDefaults({
    properties: { Configuration: 'Release' },
    processor: 'x86',
    version: 'net4.0'
})

nuget.setDefaults({
    _exe: 'src/.nuget/NuGet.exe',
    verbose: true
})

task('default', ['build'])

directory('dist/')

desc('Build')
task('build', function () {
	msbuild({
		file: 'src/SimpleOwin.sln',
		targets: ['Build']
	})
}, { async: true })

desc('Clean all')
task('clean', function () {
	msbuild({
		file: 'src/SimpleOwin.sln',
		targets: ['Clean']
	}, function(code) {
		if (code !== 0) fail('msbuild failed')
		jake.rmRf('bin/')
		jake.rmRf('dist/')
	})
}, { async: true })

namespace('nuget', function () {

	task('pack', ['nuget:pack:SimpleOwin.Extensions'])

    namespace('pack', function () {

        task('SimpleOwin.Extensions', ['dist', 'build'], function () {
            nuget.pack({
                nuspec: 'src/nuspec/SimpleOwin.Extensions.nuspec',
                version: config.version,
                outputDirectory: 'dist/'
            })
        }, { async: true })

	})

})

