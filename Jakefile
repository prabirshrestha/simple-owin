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
	})
}, { async: true })

